using FluentValidation.AspNetCore;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Patients.Context;
using Patients.API.Controllers;
using Patients.API.Routing;
using Patients.API.StartupRegistration;
using Patients.Mapping;
using Patients.Validators.Features.Patients.DTO;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using static Newtonsoft.Json.DateFormatHandling;
using static Newtonsoft.Json.DateTimeZoneHandling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace Patients.API
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCustomMvc(_configuration, _hostingEnvironment)
                    .AddDataStores()
                    .AddDependencyInjection()
                    .AddCustomizedSwagger(_hostingEnvironment, _configuration)
                    .AddCustomAuthenticationAndAuthorization(_configuration);
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
                if (env.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(opt =>
                    {
                        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Patients API V1");
                    })
                    ;
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
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllApi, "{controller}/");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapControllerRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "{controller}/{id}/{action}/");
                routeBuilder.MapControllerRoute(RouteNames.DefaultSearchResourcesApi, "{controller}/search/");
            });
        }
    }
}
