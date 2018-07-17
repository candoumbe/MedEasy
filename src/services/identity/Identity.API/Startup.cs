using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.API.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureMvc(_configuration);
            services.ConfigureDependencyInjection();
            services.ConfigureSwagger(_hostingEnvironment, _configuration);
            services.ConfigureAuthentication(_configuration);
            services.ConfigureDataStores();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            app.UseAuthentication();
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
                app.UseBrowserLink();

                if (env.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(opt =>
                    {
                        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API V1");
                    })
                    ;
                }
            }

            app.UseCors("AllowAnyOrigin");
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute(RouteNames.Default, "identity/{controller=root}/{action=index}");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneByIdApi, "identity/{controller}/{id}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllApi, "identity/{controller}/");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "identity/{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "identity/{controller}/{id}/{action}/");
                routeBuilder.MapRoute(RouteNames.DefaultSearchResourcesApi, "identity/{controller}/search/");
            });
        }
    }
}
