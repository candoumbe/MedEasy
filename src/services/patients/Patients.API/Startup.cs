namespace Patients.API
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Patients.API.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Hosting;
using MassTransit;

    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        ///<inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCustomMvc(_configuration, _hostingEnvironment)
                    .AddDataStores(_configuration)
                    .AddDependencyInjection()
                    .AddCustomApiVersioning()
                    .AddCustomizedSwagger(_hostingEnvironment, _configuration)
                    .AddCustomAuthenticationAndAuthorization(_configuration)
                    .AddCustomMassTransit(_hostingEnvironment,_configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory, IHostApplicationLifetime applicationLifetime)
        {
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
            else
            {
                if (env.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(opt =>
                    {
                        opt.RoutePrefix = string.Empty;
                        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Patients API V1");
                    });
                }
            }
            app.UseRouting();

            app.UseCors("AllowAnyOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(routeBuilder =>
            {
                routeBuilder.MapControllerRoute(RouteNames.Default, "/{controller=root}/{action=index}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneByIdApi, "/{controller}/{id}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllApi, "/{controller}/");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "/{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "/{controller}/{id}/{action}/");
                routeBuilder.MapControllerRoute(RouteNames.DefaultSearchResourcesApi, "/{controller}/search/");
            });
        }
    }
}
