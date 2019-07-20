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

namespace Patients.API
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

            services.AddCustomMvc(_configuration, _hostingEnvironment)
                .AddDataStores();
            services.AddAuthorization();
            services.AddDependencyInjection();
            services.ConfigureSwagger(_hostingEnvironment, _configuration);
            services.ConfigureAuthentication(_configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            app.UseAuthentication();
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

            app.UseCors("AllowAnyOrigin");
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute(RouteNames.Default, "{controller=root}/{action=index}");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneByIdApi, "{controller}/{id}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllApi, "{controller}/");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "{controller}/{id}/{action}/");
                routeBuilder.MapRoute(RouteNames.DefaultSearchResourcesApi, "{controller}/search/");
            });
        }
    }
}
