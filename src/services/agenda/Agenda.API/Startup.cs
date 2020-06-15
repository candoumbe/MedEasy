using Agenda.API.Routing;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Agenda.API
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
        /// Provides information about the host
        /// </summary>
        public IHostEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Builds a new <see cref="Startup"/> instance.
        /// </summary>
        /// <param name="env">Accessor to the current ost's environment </param>
        /// <param name="configuration">configuration provided by the host</param>
        public Startup(IHostEnvironment env, IConfiguration configuration)
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
            services
                .AddCustomizedMvc(Configuration, HostingEnvironment)
                .AddDataStores()
                .AddCustomOptions(Configuration)
                .AddCustomizedDependencyInjection()
                .AddCustomApiVersioning()
                .AddCustomAuthenticationAndAuthorization(Configuration)
                .AddCustomizedSwagger(HostingEnvironment, Configuration);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="applicationLifetime"></param>
        public void Configure(IApplicationBuilder app, IHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            app.UseApiVersioning();

            if (env.IsProduction())
            {
                app.UseHsts();
            }
            
            app.UseHttpMethodOverride();

            if (env.IsProduction() || env.IsStaging())
            {
                app.UseResponseCaching();
                app.UseResponseCompression();
            }
            else
            {
                app.UseDeveloperExceptionPage();

                if (env.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(opt =>
                    {
                        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                        {
                            opt.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Agenda REST API {description.GroupName}");
                        }
                    });
                }
            }

            app.UseRouting();
            app.UseCors("AllowAnyOrigin");

            app.UseAuthentication()
               .UseAuthorization();

            app.UseEndpoints(routeBuilder =>
            {
                routeBuilder.MapControllerRoute(RouteNames.Default, "v{version:apiVersion}/{controller=health}/{action=status}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneByIdApi, "v{version:apiVersion}/{controller}/{id}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllApi, "v{version:apiVersion}/{controller}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "v{version:apiVersion}/{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "v{version:apiVersion}/{controller}/{id}/{action}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultSearchResourcesApi, "v{version:apiVersion}/{controller}/search");
            });
        }
    }
}
