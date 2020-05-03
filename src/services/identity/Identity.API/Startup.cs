using Identity.API.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Identity.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCustomMvc(_configuration, _hostingEnvironment)
                .AddDataStores()
                .AddCustomOptions(_configuration)
                .AddCustomAuthentication(_configuration)
                .AddCustomApiVersioning()
                .AddDependencyInjection()
                .AddSwagger(_hostingEnvironment, _configuration);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            app.UseApiVersioning();
            app.UseHttpMethodOverride();
            if (env.IsProduction())
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();

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
                        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions.Where(api => !api.IsDeprecated))
                        {
                            opt.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"{env.ApplicationName} REST API {description.GroupName}");
                        }
                    });
                }
            }

            app.UseRouting();
            app.UseCors("AllowAnyOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(routeBuilder =>
            {
                routeBuilder.MapControllerRoute(RouteNames.Default, "v{version:apiVersion}/{controller=root}/{action=index}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneByIdApi, "v{version:apiVersion}/{controller}/{id}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllApi, "v{version:apiVersion}/{controller}/");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "v{version:apiVersion}/{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "v{version:apiVersion}/{controller}/{id}/{action}/");
                routeBuilder.MapControllerRoute(RouteNames.DefaultSearchResourcesApi, "v{version:apiVersion}/{controller}/search/");
            });
        }
    }
}
