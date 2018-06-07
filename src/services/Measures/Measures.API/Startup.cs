using Measures.API.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.CustomizeMvc(Configuration);
            services.AddDataStores();
            services.CustomizeDependencyInjection();
            services.AddCustomizedAuthentication(Configuration);

            if (HostingEnvironment.IsDevelopment())
            {
                services.AddCustomizedSwagger(HostingEnvironment, Configuration);
            }
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="applicationLifetime"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            app.UseHttpMethodOverride();
            applicationLifetime.ApplicationStopping.Register(() =>
            {

            });

            if (env.IsProduction() || env.IsStaging())
            {
                app.UseResponseCaching();
                app.UseResponseCompression();
            }
            else
            {
                loggerFactory.AddDebug();
                loggerFactory.AddConsole();
                app.UseAuthentication();
                app.UseBrowserLink();

                if (env.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(opt =>
                    {
                        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "MedEasy REST API V1");
                    }); 
                }
            }

            app.UseCors("AllowAnyOrigin");
            app.UseAuthentication();
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute(RouteNames.Default, "measures/{controller=root}/{action=index}");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneByIdApi, "measures/{controller}/{id}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllApi, "measures/{controller}/");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "measures/{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "measures/{controller}/{id}/{action}/");
                routeBuilder.MapRoute(RouteNames.DefaultSearchResourcesApi, "measures/{controller}/search/");
            });
        }
    }
}
