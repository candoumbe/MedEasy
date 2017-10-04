using System;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;
using static MedEasy.Data.DataFilterOperator;
using static System.StringSplitOptions;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.Data
{
    /// <summary>
    /// Extensions methods class which allow to build expression out of a <see cref="IDataFilter"/> instances.
    /// </summary>
    public static class DataFilterExtensions
    {
        /// <summary>
        /// Builds an <see cref="Expression{Func{T}}"/> tree from a <see cref="IDataFilter"/> instance.
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="filter"><see cref="IDataFilter"/> instance to build an <see cref="Expression{TDelegate}"/> tree from.</param>
        /// <returns><see cref="Expression{TDelegate}"/></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="filter"/> is <c>null</c>.</exception>
        public static Expression<Func<T, bool>> ToExpression<T>(this IDataFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), $"{nameof(filter)} cannot be null");
            }
            Expression<Func<T, bool>> filterExpression = null;

            if (filter is DataFilter)
            {
                DataFilter df = filter as DataFilter;
                if (df.Field == null)
                {
                    filterExpression = x => true;
                }
                else
                {
                    Type type = typeof(T);
                    ParameterExpression pe = Parameter(type, "item");

                    string[] fields = df.Field.Split(new[] { '.' });
                    MemberExpression property = null;
                    Type memberType;
                    foreach (string field in fields)
                    {
                        property = property == null
                            ? Property(pe, field)
                            : Property(property, field);
                    }

                    Expression body;
                    memberType = (property.Member as PropertyInfo)?.PropertyType;
                    ConstantExpression constantExpression = Constant(df.Value, memberType);
                    
                    switch (df.Operator)
                    {
                        case NotEqualTo:
                            // 
                            body = NotEqual(property, constantExpression);
                            break;
                        case IsNull:
                            body = Equal(property, Constant(null));
                            break;
                        case IsNotNull:
                            body = NotEqual(property, Constant(null));
                            break;
                        case DataFilterOperator.LessThan:
                            body = LessThan(property, constantExpression);
                            break;
                        case DataFilterOperator.GreaterThan:
                            body = GreaterThan(property, constantExpression);
                            break;
                        case DataFilterOperator.GreaterThanOrEqual:
                            body = GreaterThanOrEqual(property, constantExpression);
                            break;
                        case StartsWith:
                            body = Call(property, typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) }), constantExpression);
                            break;
                        case EndsWith:
                            body = Call(property, typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) }), constantExpression);
                            break;
                        case Contains:
                            body = Call(property, typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) }), constantExpression);
                            break;
                        case IsEmpty:
                            body = Equal(property, Constant(string.Empty));
                            break;
                        case IsNotEmpty:
                            body = NotEqual(property, Constant(string.Empty));
                            break;
                        default:
                            body = Equal(property, constantExpression);
                            break;
                    }

                    filterExpression = Lambda<Func<T, bool>>(body, pe);
                }

            }
            else if (filter is DataCompositeFilter)
            {
                DataCompositeFilter dcf = filter as DataCompositeFilter;
                Expression<Func<T, bool>> expression = null;
                // local function that can combine two expressions using either AND or OR operators
                Func<Expression<Func<T, bool>>, Expression<Func<T, bool>>, Expression<Func<T, bool>>> expressionMerger;

                if (dcf.Logic == DataFilterLogic.And)
                {
                    expressionMerger = (first, second) => first.AndAlso(second);
                }
                else
                {
                    expressionMerger = (first, second) => first.OrElse(second);
                }

                foreach (IDataFilter item in dcf.Filters)
                {
                    expression = expression == null
                        ? item.ToExpression<T>()
                        : expressionMerger(expression, item.ToExpression<T>());
                }

                filterExpression = expression;
            }

            return filterExpression;
        }

        /// <summary>
        /// Builds a <see cref="IDataFilter{T}"/> from <paramref name="queryString"/>
        /// </summary>
        /// <typeparam name="T">Type of element to filter</typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="queryString"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryString"/> is not a valid query string.</exception>
        public static IDataFilter ToFilter<T>(this string queryString)
        {
            if (queryString == null)
            {
                throw new ArgumentNullException(nameof(queryString));
            }

            IDataFilter filter = new DataFilter(field : null, @operator : default, value : null);
            Uri fakeuri = new UriBuilder
            {
                Host = "localhost",
                Query = queryString
            }.Uri;


            if (!string.IsNullOrEmpty(queryString))
            {
                string[] queryStringParts = queryString.Split(new[] { "&" }, RemoveEmptyEntries);
                if (queryStringParts.Length == 1)
                {
                    string[] keyValueParts = queryStringParts[0].Split(new[] { "=" }, RemoveEmptyEntries)
                        .Select(x => Uri.UnescapeDataString(x))
                        .ToArray();
                    if (keyValueParts.Length == 2)
                    {
                        string keyPart = keyValueParts[0];
                        string valuePart = keyValueParts[1]
                            .Replace("!!", "!")
                            .Replace("**", "*");

                        PropertyInfo pi = typeof(T).GetProperties()
                            .SingleOrDefault(x => x.CanRead && x.Name == keyPart);

                        if (pi != null)
                        {
                            TypeConverter tc = TypeDescriptor.GetConverter(pi.PropertyType);
                            
                            if (valuePart.StartsWith("!"))
                            {
                                string localValue = valuePart.Replace("!", string.Empty);
                                object value = tc.ConvertFrom(valuePart);
                                filter = new DataFilter (field : keyPart, @operator : NotEqualTo, value : localValue);
                            }
                            else if (valuePart.Like("* *"))
                            {
                                string[] localValues = valuePart.Split(new[] { ' ' }, RemoveEmptyEntries);

                                IList<IDataFilter> filters = new List<IDataFilter>();

                                foreach (string localValue in localValues)
                                {
                                    filters.Add($"{keyPart}={localValue}".ToFilter<T>());
                                }
                                filter = new DataCompositeFilter
                                {
                                    Logic = DataFilterLogic.Or,
                                    Filters = filters
                                };
                            }
                            else if (valuePart.Contains("*") || valuePart.Contains("?"))
                            {
                                if (valuePart.Contains("*") && !valuePart.Contains("?"))
                                {
                                    string[] values = valuePart.Split(new[] { "*" }, RemoveEmptyEntries);
                                    if (values.Length == 1)
                                    {
                                        filter = new DataFilter(
                                            field :keyPart,
                                            @operator : valuePart.StartsWith("*")
                                                ? EndsWith
                                                : StartsWith,
                                            value : tc.ConvertFrom(values[0])
                                        );   
                                    }
                                }
                            }
                            else
                            {
                                object value = tc.ConvertFrom(valuePart);
                                filter = new DataFilter (field : keyPart, @operator : EqualTo, value: value);
                            }       
                        }
                    }   
                }
            }

            return filter;
        }
    }

}
