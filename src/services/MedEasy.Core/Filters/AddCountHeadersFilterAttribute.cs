using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.Collections;
using static Microsoft.AspNetCore.Http.HttpMethods;

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
        public const string TotalCountHeaderName = "X-Total-Count";

        /// <summary>
        /// Name of the header that holds the number of resources returned by the current result.
        /// </summary>
        public const string CountHeaderName = "X-Count";

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            string method = context.HttpContext.Request.Method;

            (int total, int count) ComputeCountsFromPage(in IGenericPagedGetResponse response) => (response.Total, response.Count);
            (int total, int count) ComputeCountsFromEnumerable(in IEnumerable collection)
            {
                IEnumerator enumerator = collection.GetEnumerator();
                int count = 0;
                while (enumerator.MoveNext())
                {
                    count++;
                }

                return (total: count, count);
            }

            if (IsGet(method) || IsHead(method) || IsOptions(method))
            {
                int? count = null;
                int? totalCount = null;
                switch (context.Result)
                {
                    case ObjectResult okObjectResult:
                        
                        switch (okObjectResult.Value)
                        {
                            case IGenericPagedGetResponse genericPagedGetResponse:
                                {
                                    (int total, int count) counts = ComputeCountsFromPage(genericPagedGetResponse);
                                    count = counts.count;
                                    totalCount = counts.total;
                                }
                                break;

                            case IEnumerable collection:
                                {
                                    (int total, int count) counts = ComputeCountsFromEnumerable(collection);
                                    count = counts.count;
                                    totalCount = counts.total;
                                }
                                break;
                        }
                        break;
                    case IGenericPagedGetResponse genericPagedGetResponse:
                        {
                            (int total, int count) counts = ComputeCountsFromPage(genericPagedGetResponse);
                            count = counts.count;
                            totalCount = counts.total;
                        }
                        break;

                    case IEnumerable collection:
                        {
                            (int total, int count) counts = ComputeCountsFromEnumerable(collection);
                            count = counts.count;
                            totalCount = counts.total;
                        }
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
}
