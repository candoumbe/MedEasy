using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MedEasy.Handlers.Core.Search.Queries;
using MedEasy.Handlers.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;

namespace MedEasy.Search.API
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get;  }

        public ILoggerFactory LoggerFactory { get;  }

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            HostingEnvironment = env;
            LoggerFactory = loggerFactory;

        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();


            services.AddScoped<IHandleSearchQuery, HandleSearchQuery>();

            services.AddLogging();
            services.AddCors();

            services.AddRouting(opt => opt.LowercaseUrls = true);

            //services.AddResponseCaching();
            //services.AddResponseCompression(options =>
            //{
            //    options.EnableForHttps = true;
            //    options.Providers.Add<GzipCompressionProvider>();
            //});

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            
            services.AddScoped<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

            services.AddScoped<IUrlHelper>(provider =>
            {
                IActionContextAccessor actionContextAccessor = provider.GetRequiredService<IActionContextAccessor>();
                IUrlHelperFactory urlHelperFactory = provider.GetRequiredService<IUrlHelperFactory>();

                return urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            });


            if (HostingEnvironment.IsDevelopment())
            {
                ApplicationEnvironment app = PlatformServices.Default.Application;
                services.AddSwaggerGen(config =>
                {
                    config.SwaggerDoc("v1", new Info
                    {
                        Title = app.ApplicationName,
                        Description = "Search API for MedEasy",
                        Version = "v1",
                        Contact = new Contact
                        {
                            Email = Configuration.GetValue<string>("Swagger:Contact:Email"),
                            Name = Configuration.GetValue<string>("Swagger:Contact:Name"),
                            Url = Configuration.GetValue<string>("Swagger:Contact:Url")
                        }
                    });

                    config.IgnoreObsoleteActions();
                    config.IgnoreObsoleteProperties();
                    config.IncludeXmlComments(Path.Combine(app.ApplicationBasePath, $"{app.ApplicationName}.xml"));
                    config.DescribeStringEnumsInCamelCase();
                    config.DescribeAllEnumsAsStrings();
                });
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
