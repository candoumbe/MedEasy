using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Collections.Generic;
using System.Linq;

using Yarp.ReverseProxy.Abstractions;
using Optional.Linq;
using Optional;
using System.Collections.ObjectModel;

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
            const string allowAnyOriginCorsPolicyName = "AllowAnyOrigin";
            IEnumerable<MedEasyApi> apis = _configuration.GetSection("Services").Get<List<MedEasyApi>>();

            IList<ProxyRoute> routes = new List<ProxyRoute>(apis.Count());
            IList<Cluster> clusters = new List<Cluster>(apis.Count());

            for (int i = 0; i < apis.Count(); i++)
            {
                MedEasyApi api = apis.ElementAt(i);
                _configuration.GetServiceUri(api.Name, api.Binding)
                              .SomeNotNull()
                              .MatchSome(address =>
                              {
                                  string clusterName = $"cluster{i+1}";

                                  Cluster cluster = new()
                                  {
                                      Id = clusterName,
                                      Destinations = new Dictionary<string, Destination>
                                      {
                                          [$"{clusterName}/{api.Id}"] = new Destination()
                                          {
                                              Address = address.AbsoluteUri
                                          }
                                      }
                                  };
                                  List<IReadOnlyDictionary<string, string>> transforms = new List<IReadOnlyDictionary<string, string>>();
                                  api.Proxy.Transforms.ForEach(transform => transforms.Add(new Dictionary<string, string>
                                  {
                                      [transform.Name] = transform.Value
                                  }));
                                  ProxyRoute route = new()
                                  {
                                      RouteId = $"route-{api.Id}",
                                      ClusterId = clusterName,
                                      CorsPolicy = allowAnyOriginCorsPolicyName,
                                      Match = new RouteMatch
                                      {
                                          Path = api.Proxy.Match.Path
                                      },
                                      Transforms = transforms
                                  };

                                  routes.Add(route);
                                  clusters.Add(cluster);
                              });
            }

            proxyBuilder.LoadFromMemory(routes, clusters);

            services.AddCors(options =>
            {
                options.AddPolicy(allowAnyOriginCorsPolicyName, builder =>
                {
                    builder.AllowAnyOrigin();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
            });
        }
    }


}
