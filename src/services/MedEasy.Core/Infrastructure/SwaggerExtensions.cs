namespace MedEasy.Core.Infrastructure
{
    using MedEasy.Ids;

    using Microsoft.Extensions.DependencyInjection;

    using Swashbuckle.AspNetCore.SwaggerGen;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Swagger extensions methods
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Configures Swagger map types to handle strongly type ids
        /// </summary>
        /// <param name="options"></param>
        /// <typeparam name="TStronglyTypedId">Strongly typed id</typeparam>
        public static void ConfigureForStronglyTypedIdsInAssembly<TStronglyTypedId>(this SwaggerGenOptions options)
        {
            Assembly assembly = typeof(TStronglyTypedId).Assembly;
            Type[] types = assembly.GetTypes().Where(type => type.IsAssignableToGenericType(typeof(StronglyTypedId<>)))
                                              .ToArray();

            ConfigureForStronglyTypedIds(options, types);
        }

        /// <summary>
        /// Configures Swagger map types to handle strongly type ids
        /// </summary>
        /// <param name="options"></param>
        /// <param name="types"></param>
        public static void ConfigureForStronglyTypedIds(this SwaggerGenOptions options, params Type[] types)
        {
            types.ForEach(item => options.MapType(item, () => new Microsoft.OpenApi.Models.OpenApiSchema { Format = "uuid", Type = "string" }));
        }
    }
}
