using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;
namespace MedEasy.Core.Middlewares
{
    /// <summary>
    /// Suppress content from <see cref="HttpResponseMessage"/> that were caused by <c>HEAD</c> requests
    /// </summary>
    public class HeadMiddleware
    {

        private readonly RequestDelegate _next;

        /// <summary>
        /// Builds a new <see cref="HeadMiddleware"/> instance
        /// </summary>
        /// <param name="next"></param>
        public HeadMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        public async Task InvokeAsync(HttpContext httpContext)
        {
            await _next(httpContext).ConfigureAwait(false);

            if (HttpMethods.IsHead(httpContext.Request.Method) && httpContext.Response.StatusCode == Status200OK)
            {
                HttpResponse response = httpContext.Response;
                response.StatusCode = Status204NoContent;
                if (!response.HasStarted)
                {
                    response.ContentLength = 0;
                    response.Body = new MemoryStream();
                }

            }
        }
    }
}
