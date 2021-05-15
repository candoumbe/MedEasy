namespace MedEasy.IntegrationTests.Core
{
    using Microsoft.Extensions.DependencyInjection;

    using System.Collections.Generic;
    using System.Linq;

    public static class ServiceCollectionsExtensions
    {
        /// <summary>
        /// Removes all <typeparamref name="TType"/> implementations from <paramref name="services"/>.
        /// </summary>
        /// <typeparam name="TType">Type of services to remove</typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection Remove<TType>(this IServiceCollection services)
        {
            IEnumerable<ServiceDescriptor> descriptors = services.Where(d => d.ServiceType.IsAssignableFrom(typeof(TType)))
                                                                 .ToArray();

            foreach (ServiceDescriptor item in descriptors)
            {
                services.Remove(item);
            }

            return services;
        }
    }
}
