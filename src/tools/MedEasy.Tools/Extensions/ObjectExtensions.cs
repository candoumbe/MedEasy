using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Deeply clone the <paramref name="source"/> 
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="source"/></typeparam>
        /// <param name="source">The object to clone</param>
        /// <returns>A deep copy of the object</returns>
        public static T DeepClone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            T clone = ReferenceEquals(source, null)
                ? default
                : JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));

            return clone;
        }

        /// <summary>
        /// This method is intend is to parse an object to extract its properties.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="IDictionary{TKey, TValue}"/></returns>

        public static IDictionary<string, object> ParseAnonymousObject(this object obj)
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();

            if (obj != null)
            {
                if (obj is IEnumerable enumerable)
                {
                    dictionary = new Dictionary<string, object>();
                    IEnumerator enumerator = enumerable.GetEnumerator();
                    int count = 0;
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current != null)
                        {
                            Type currentType = current.GetType();
                            TypeInfo currentTypeInfo =currentType.GetTypeInfo();
                            if (currentTypeInfo.IsPrimitive || currentTypeInfo.IsEnum || currentType == typeof(string))
                            {
                                dictionary.Add($"{count}", current);
                            }
                            //else 
                            //{
                            //    dictionary.Add($"{count}", ParseAnonymousObject(current));
                            //}
                            count++;
                        }
                    }
                    
                }

                else
                {
                    IEnumerable<PropertyInfo> properties = obj.GetType()
                            .GetRuntimeProperties()
                            .Where(pi => pi.CanRead && !pi.GetMethod.IsStatic && pi.GetValue(obj) != null);

                    dictionary = properties.ToDictionary(
                        pi => pi.Name,
                        pi =>
                        {
                            object value = pi.GetValue(obj);
                            Type valueType = value.GetType();
                            TypeInfo valueTypeInfo = valueType.GetTypeInfo();


                            if (!(valueTypeInfo.IsEnum || valueTypeInfo.IsPrimitive || valueType == typeof(string) || DictionaryExtensions.PrimitiveTypes.Contains(valueType)))
                            {
                                value = ParseAnonymousObject(value);
                            }

                            return value;
                        }
                    ); 
                }

            }
            return dictionary
                .OrderBy(x => x.Key)
                .ToDictionary(x=> x.Key, x => x.Value);
        }



        /// <summary>
        /// Converts an object to its string representation suitable for appending as query string in a url
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <returns>the query string representation preceeded with the "?" or an empty string</returns>
        public static string ToQueryString(this object obj)
            => DictionaryExtensions.ToQueryString(obj.ParseAnonymousObject());


        /// <summary>
        /// Performs a "safe cast" of the specified object to the specified type.
        /// </summary>
        /// <typeparam name="TSource">Type of the object to be converted</typeparam>
        /// <typeparam name="TDest">targeted type</typeparam>
        /// <param name="obj">The object to cast</param>
        /// <returns>The "safe cast" result</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="obj"/> is <c>null</c></exception>
        public static TDest As<TSource, TDest>(this TSource obj) => (TDest)As(obj, typeof(TDest));


        /// <summary>
        /// Performs a "safe cast" of <paramref name="obj"/> to the type <paramref name="targetType"/>.
        /// </summary>
        /// <typeparam name="TSource">type of the object to cast </param>
        /// <param name="targetType">type to cast </param>
        /// <param name="obj">The object to cast</param>
        /// <returns>The "safe cast" result</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="obj"/> or <paramref name="targetType"/> is <c>null</c></exception>
        public static object As<TSource>(this TSource obj, Type targetType)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            object safeCastResult = null;

            Type sourceType = typeof(TSource);

            if (targetType == sourceType || sourceType.GetTypeInfo().IsAssignableFrom(targetType.GetTypeInfo()))
            {
                ParameterExpression param = Expression.Parameter(obj.GetType());
                Expression asExpression = Expression.TypeAs(param, targetType);
                LambdaExpression expression = Expression.Lambda(asExpression, param);
                safeCastResult = expression.Compile().DynamicInvoke(obj);
            }

            return safeCastResult;
        }



    }
}