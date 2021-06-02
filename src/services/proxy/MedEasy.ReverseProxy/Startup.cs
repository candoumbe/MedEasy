using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Linq;

using Yarp.ReverseProxy.Abstractions;

namespace MedEasy.ReverseProxy
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add the reverse proxy to capability to the server
            IReverseProxyBuilder proxyBuilder = services.AddReverseProxy();

            string[] apis = _configuration.GetSection("services")
                                                     .GetChildren()
                                                     .Select(x => x.Value)
                                                     .ToArray();


            IEnumerable<Cluster> clusters = apis
                .Select(api => (name: api, address: _configuration.GetServiceUri(api)))
                .Where(api => api.address is not null)
                .Select(service => new Cluster
                {
                    Destinations = new Dictionary<string, Destination>
                    {
                        [service.name] = new Destination
                        {
                            Address = service.address.AbsoluteUri,
                        }
                    }
                });

            IEnumerable<ProxyRoute> routes = apis
                .Select(api => (name: api, address: _configuration.GetServiceUri(api)))
                .Where(api => api.address is not null)
                .Select(service => new ProxyRoute
            {
                ClusterId = service.name,
                Match = new RouteMatch
                {
                    Hosts = new[] { $"{service.name}.medeasy.com" }
                }
            });

            proxyBuilder.LoadFromMemory(routes, clusters);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
            });
        }
    }


}
