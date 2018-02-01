using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace MedEasy.Core.Middlewares
{
    public static class HeadMiddlewareExtenrsions
    {
        /// <summary>
        /// Adds <see cref="HeadMiddleware"/> to the request pipeline to remove content
        /// </summary>
        /// <param name="app"></param>
        public static void UseHeadMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<HeadMiddleware>();
    }
}
