using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MedEasy.Core.Filters
{
    /// <summary>
    /// <para>
    /// Adds 'X-Count' and 'X-Total-Count' headers.
    /// </para>
    /// Theses headers are added if a controller returned a collection of resources
    /// </summary>
    public class AddCountHeadersFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Name of the header that holds the total number of resources
        /// </summary>
        public static readonly string TotalCountHeaderName = "X-Total-Count";
        /// <summary>
        /// Name of the header that holds the number of resources returned by the current result.
        /// </summary>
        public static readonly string CountHeaderName = "X-Count";

        private static readonly Type _genericPageResponseType = typeof(GenericPagedGetResponse<>);
        private static readonly Type _enumerableType = typeof(Enumerable);

        public override void OnResultExecuting(ResultExecutingContext context)
        {

            if (context.Result is OkObjectResult okObjectResult && !Equals(okObjectResult.Value, default) && okObjectResult.Value.GetType().IsAssignableToGenericType(_genericPageResponseType))
            {
                Type resultType = okObjectResult.Value.GetType() ?? typeof(void);
                Type pageResponseType = _genericPageResponseType;
                if (resultType.IsAssignableToGenericType(pageResponseType))
                {
                    pageResponseType = pageResponseType.MakeGenericType(resultType.GetGenericArguments()[0]);
                    IEnumerable<PropertyInfo> properties = pageResponseType.GetProperties();
                    
                    PropertyInfo totalCountProperty = properties.Single(x => x.CanRead && x.Name == nameof(GenericPagedGetResponse<int>.Count));

                    object totalCount = totalCountProperty.GetValue(okObjectResult.Value);
                    context.HttpContext.Response.Headers.Add(TotalCountHeaderName, new StringValues(totalCount.ToString()));

                    PropertyInfo itemsProperty = properties.Single(x => x.CanRead && x.Name == nameof(GenericPagedGetResponse<int>.Items));
                    object items = itemsProperty.GetValue(okObjectResult.Value);
                    MethodInfo countMethodInfo = _enumerableType.GetRuntimeMethods()
                        .Single(method => method.IsStatic && method.Name == nameof(Enumerable.Count) && method.GetParameters().Length == 1);
                    countMethodInfo = countMethodInfo.MakeGenericMethod(itemsProperty.PropertyType.GetGenericArguments());

                    object count = countMethodInfo.Invoke(null, new[] { items });
                    context.HttpContext.Response.Headers.Add(CountHeaderName, new StringValues(count.ToString()));
                }

            }
        }
    }
}
