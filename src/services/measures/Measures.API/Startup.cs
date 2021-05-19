namespace Measures.API
{
    using MassTransit;

    using Measures.API.Routing;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using System.Collections.Generic;

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
            services.AddCustomMvc(Configuration, HostingEnvironment)
                    .AddDataStores()
                    .AddDependencyInjection()
                    .AddCustomAuthentication(Configuration)
                    .AddCustomApiVersioning()
                    .AddCustomOptions(Configuration)
                    .AddSwagger(HostingEnvironment, Configuration)
                    .AddCustomMassTransit(Configuration);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="applicationLifetime"></param>
        /// <param name="provider"></param>
        public void Configure(IApplicationBuilder app, IHostEnvironment env, IHostApplicationLifetime applicationLifetime, IApiVersionDescriptionProvider provider)
        {

            app.UseApiVersioning();
            app.UseHttpMethodOverride();

            if (env.IsProduction())
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            using IServiceScope scope = app.ApplicationServices.CreateScope();
            IBusControl busControl = scope.ServiceProvider.GetRequiredService<IBusControl>();

            applicationLifetime.ApplicationStarted.Register(async () => await busControl.StartAsync().ConfigureAwait(false));
            applicationLifetime.ApplicationStopping.Register(async () => await busControl.StopAsync().ConfigureAwait(false));

            if (env.IsProduction() || env.IsStaging())
            {
                app.UseResponseCaching();
                app.UseResponseCompression();
            }

            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.RoutePrefix = string.Empty;
                provider.ApiVersionDescriptions
                        .ForEach(description => opt.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Measures REST API {description.GroupName}"));
            });

            app.UseRouting();

            app.UseCors("AllowAnyOrigin");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(routeBuilder =>
            {
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneByIdApi, "/v{version:apiVersion}/{controller}/{id}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllApi, "/v{version:apiVersion}/{controller}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "/v{version:apiVersion}/{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "/v{version:apiVersion}/{controller}/{id}/{action}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultSearchResourcesApi, "/v{version:apiVersion}/{controller}/search");
            });
        }
    }
}
