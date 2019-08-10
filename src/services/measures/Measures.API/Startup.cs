using Measures.API.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Measures.API
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The root configuration 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Provides information about 
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Builds a new <see cref="Startup"/> instance.
        /// </summary>
        /// <param name="env">Accessor to the current ost's environment </param>
        /// <param name="configuration">configuration provided by the host</param>
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
#if !NETCOREAPP2_0
            services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = Configuration.GetValue<int>("HttpsPort", 63796);
                    options.RedirectStatusCode = Status307TemporaryRedirect;
                });
#endif
            services.AddDataStores()
                .ConfigureDependencyInjection()
                .ConfigureAuthentication(Configuration)
                .AddCustomApiVersioning();

            if (HostingEnvironment.IsDevelopment())
            {
                services.ConfigureSwagger(HostingEnvironment, Configuration);
            }
            services.ConfigureMvc(Configuration);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="applicationLifetime"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime, IApiVersionDescriptionProvider provider)
        {
            app.UseApiVersioning();
            app.UseAuthentication();

            if (env.IsProduction())
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseHttpMethodOverride();
            applicationLifetime.ApplicationStopping.Register(() =>
            {
                if (env.IsEnvironment("IntegrationTest"))
                {

                }
            });

            if (env.IsProduction() || env.IsStaging())
            {
                app.UseResponseCaching();
                app.UseResponseCompression();
            }
            else
            {
                
                if (env.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(opt =>
                    {
                        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                        {
                            if (!description.IsDeprecated)
                            {
                                opt.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Agenda REST API {description.GroupName}");
                            }
                        }
                    });
                }
            }

            app.UseCors("AllowAnyOrigin");

            app.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute(RouteNames.DefaultGetOneByIdApi, "/v{version:apiVersion}/{controller}/{id}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllApi, "/v{version:apiVersion}/{controller}/");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "/v{version:apiVersion}/{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "/v{version:apiVersion}/{controller}/{id}/{action}/");
                routeBuilder.MapRoute(RouteNames.DefaultSearchResourcesApi, "/v{version:apiVersion}/{controller}/search/");
            });
        }
    }
}
