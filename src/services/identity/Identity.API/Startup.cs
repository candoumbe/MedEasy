namespace Identity.API
{
    using Identity.API.Routing;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using System.Linq;

    /// <summary>
    /// Startup class
    /// </summary>
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
                    .AddCustomOptions(_configuration)
                    .AddDataStores(_configuration)
                    .AddCustomAuthentication(_configuration)
                    .AddCustomApiVersioning()
                    .AddDependencyInjection()
                    .AddCustomSwagger(_hostingEnvironment, _configuration)
                    .AddCustomHealthChecks();
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
                app.UseHealthChecks("/health");
                if (env.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(opt =>
                    {
                        opt.RoutePrefix = string.Empty;
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
                routeBuilder.MapControllerRoute(RouteNames.Default, "{controller=root}/{action=index}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneByIdApi, "{controller}/{id}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllApi, "{controller}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "{controller}/{id}/{action}/");
                routeBuilder.MapControllerRoute(RouteNames.DefaultSearchResourcesApi, "{controller}/search/");
                routeBuilder.MapHealthChecks("/health");
            });
        }
    }
}
