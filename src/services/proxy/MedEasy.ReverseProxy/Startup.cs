using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Optional;

using System.Collections.Generic;
using System.Linq;

using Yarp.ReverseProxy.Configuration;

namespace MedEasy.ReverseProxy
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private const string AllowAnyOriginCorsPolicyName = "AllowAnyOriginCorsPolicyName";

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
            IEnumerable<MedEasyApi> apis = _configuration.GetSection("Services").Get<List<MedEasyApi>>();

            IList<RouteConfig> routes = new List<RouteConfig>(apis.Count());
            IList<ClusterConfig> clusters = new List<ClusterConfig>(apis.Count());

            for (int i = 0; i < apis.Count(); i++)
            {
                MedEasyApi api = apis.ElementAt(i);
                _configuration.GetServiceUri(api.Name, api.Binding)
                              .SomeNotNull()
                              .MatchSome(address =>
                              {
                                  string clusterName = $"cluster{i + 1}";

                                  ClusterConfig cluster = new()
                                  {
                                      ClusterId = clusterName,
                                      Destinations = new Dictionary<string, DestinationConfig>
                                      {
                                          [$"{clusterName}/{api.Id}"] = new()
                                          {
                                              Address = address.AbsoluteUri
                                          }
                                      },
                                      HttpClient = new HttpClientConfig()
                                      {
                                          DangerousAcceptAnyServerCertificate = api.HttpClient.ThrustSslCertificate
                                      }
                                  };

                                  RouteConfig route = new()
                                  {
                                      RouteId = $"route-{api.Id}",
                                      ClusterId = clusterName,
                                      CorsPolicy = AllowAnyOriginCorsPolicyName,
                                      Match = new RouteMatch
                                      {
                                          Path = $"/api/{api.Id}/{{**catch-all}}"
                                      },
                                      Transforms = new List<IReadOnlyDictionary<string, string>>
                                      {
                                            new Dictionary<string, string>
                                            {
                                                ["PathRemovePrefix"] = $"/api/{api.Id}"
                                            }
                                      }
                                  };

                                  routes.Add(route);
                                  clusters.Add(cluster);
                              });
            }

            proxyBuilder.LoadFromMemory(routes, clusters);

            services.AddCors(options =>
            {
                options.AddPolicy(AllowAnyOriginCorsPolicyName, builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpLogging();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders();
            app.UseRouting();

            app.UseCors(AllowAnyOriginCorsPolicyName);

            app.UseEndpoints(endpoints => endpoints.MapReverseProxy());
        }
    }
}
