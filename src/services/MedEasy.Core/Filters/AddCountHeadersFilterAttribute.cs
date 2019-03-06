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

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            int? count= null;
            int? totalCount = null;
            switch (context.Result)
            {
                case ObjectResult okObjectResult when okObjectResult.Value is IGenericPagedGetResponse genericPagedGetResponse:
                    totalCount = genericPagedGetResponse.Total;
                    count = genericPagedGetResponse.Count;
                    break;

                case IGenericPagedGetResponse genericPagedGetResponse:
                    totalCount = genericPagedGetResponse.Total;
                    count = genericPagedGetResponse.Count;
                    break;
            }

            if (totalCount.HasValue)
            {
                context.HttpContext.Response.Headers.Add(TotalCountHeaderName, new StringValues(totalCount.Value.ToString()));
            }
            if (count.HasValue)
            {
                context.HttpContext.Response.Headers.Add(CountHeaderName, new StringValues(count.Value.ToString()));
            }
        }
    }
}
