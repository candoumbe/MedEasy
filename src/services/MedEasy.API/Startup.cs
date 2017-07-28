﻿using MedEasy.API.Filters;
using MedEasy.API.StartupRegistration;
using MedEasy.API.Stores;
using MedEasy.API.Swagger;
using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Core.Search.Queries;
using MedEasy.Handlers.Search;
using MedEasy.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using static Newtonsoft.Json.DateFormatHandling;
using static Newtonsoft.Json.DateTimeZoneHandling;

namespace MedEasy.API
{
    /// <summary>
    /// Contains all informations to bootstrap the application
    /// </summary>
    public class Startup
    {

        private IHostingEnvironment HostingEnvironment { get; }
        private IConfigurationRoot Configuration { get; }

        private ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Builds a new <see cref="Startup"/> instance
        /// </summary>
        /// <param name="env">hosting environment configuration</param>
        /// <param name="loggerFactory">Logger factory that will be used throughout the lifecycle of the application</param>
        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            HostingEnvironment = env;
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            LoggerFactory = loggerFactory;
        }


        /// <summary>
        /// Configure the D.I. container
        /// </summary>
        /// <param name="services"></param>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton((provider) => AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(x => AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder);

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory>(item =>
            {
                DbContextOptionsBuilder<MedEasyContext> builder = new DbContextOptionsBuilder<MedEasyContext>();
                builder.UseSqlServer(Configuration.GetConnectionString("Default"));

                return new EFUnitOfWorkFactory(builder.Options);

            });

            services.AddScoped<IHandleSearchQuery, HandleSearchQuery>();
            services.AddPatientsControllerDependencies();
            services.AddSpecialtiesControllerDependencies();
            services.AddDoctorsControllerDependencies();
            services.AddDocumentsControllerDependencies();
            services.AddAppointmentsControllerDependencies();

            services.AddLogging();
            services.AddCors();

            services.AddRouting(opt => opt.LowercaseUrls = true);

            services.AddResponseCaching();
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(builder =>
            {

                IUrlHelperFactory urlHelperFactory = builder.GetRequiredService<IUrlHelperFactory>();
                IActionContextAccessor actionContextAccessor = builder.GetRequiredService<IActionContextAccessor>();

                return urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            });



            services.AddOptions();
            services.Configure<MedEasyApiOptions>((options) =>
            {
                options.DefaultPageSize = Configuration.GetValue("APIOptions:DefaultPageSize", 30);
                options.MaxPageSize = Configuration.GetValue("APIOptions:MaxPageSize", 100);
            });
            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(FormatFilter));
                options.Filters.Add(typeof(ValidateModelAttribute));
                //options.Filters.Add(typeof(EnvelopeFilterAttribute));
                options.Filters.Add(typeof(HandleErrorAttribute));
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.DateFormatHandling = IsoDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = Utc;
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            });

            if (HostingEnvironment.IsDevelopment())
            {
                ApplicationEnvironment app = PlatformServices.Default.Application;
                services.AddSwaggerGen(config =>
                {
                    config.SwaggerDoc("v1", new Info
                    {
                        Title = app.ApplicationName,
                        Description = "REST API for MedEasy",
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
                    config.OperationFilter<FileUploadOperationFilter>();
                });
            }
        }
        /// <summary>
        /// Configures the request pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            LoggerFactory.AddConsole(Configuration.GetSection("Logging"));
            LoggerFactory.AddDebug();

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            if (!env.IsDevelopment())
            {
                app.UseResponseCaching();
                app.UseResponseCompression();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();

                app.UseSwagger();
                app.UseSwaggerUI(opt =>
                {
                    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "MedEasy REST API V1");
                });

            }

            app.UseMvc(routeBuilder =>
            {
                MedEasyApiOptions apiOptions = Configuration.Get<MedEasyApiOptions>();
                routeBuilder.MapRoute("default", "api/{controller=root}/{action=index}");
                routeBuilder.MapRoute("defaultGetById", "api/{controller}/{id}");
            });

            app.UseWelcomePage();


        }
    }
}