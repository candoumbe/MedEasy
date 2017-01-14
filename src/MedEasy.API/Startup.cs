using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MedEasy.DAL.Interfaces;
using MedEasy.API.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MedEasy.Mapping;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using MedEasy.API.StartupRegistration;
using MedEasy.API.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Http;
using MedEasy.Data.Converters;
using MedEasy.Handlers;
using System;

namespace MedEasy.API
{
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
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            LoggerFactory = loggerFactory;
        }



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

            services.AddLogging();

            services.AddRouting();

            services.AddResponseCaching();
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddOptions();
            services.Configure<MedEasyApiOptions>((options) =>
            {
                options.DefaultPageSize = Configuration.GetValue("APIOptions:DefaultPageSize", 30);
                options.MaxPageSize = Configuration.GetValue("APIOptions:DefaultPageSize", 100);
            });
            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(FormatFilter));
                options.Filters.Add(typeof(ValidateModelAttribute));
                options.Filters.Add(typeof(EnvelopeFilterAttribute));
                options.Filters.Add(typeof(HandleErrorAttribute));
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

            }).AddJsonOptions(options => {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters.Add(new DataFilterConverter());
                options.SerializerSettings.Converters.Add(new DataCompositeFilterConverter());
                options.SerializerSettings.Converters.Add(new DataFilterOperatorConverter());
            });


            
            

            if (HostingEnvironment.IsDevelopment())
            {
                ApplicationEnvironment app = PlatformServices.Default.Application;
                services.AddSwaggerGen(config =>
                {
                    
                    //config.SingleApiVersion(new Info
                    //{
                    //    Title = app.ApplicationName,
                    //    Description = "REST API for MedEasy",
                    //    Contact = new Contact
                    //    {
                    //        Email = Configuration.GetValue<string>("Swagger:Contact:Email"),
                    //        Name = Configuration.GetValue<string>("Swagger:Contact:Name"),
                    //        Url = Configuration.GetValue<string>("Swagger:Contact:Url"),
                    //    }
                    //});

                    config.IgnoreObsoleteActions();
                    config.IgnoreObsoleteProperties();
                    config.IncludeXmlComments(Path.Combine(app.ApplicationBasePath, $"{app.ApplicationName}.xml"));
                    config.DescribeStringEnumsInCamelCase();
                    config.DescribeAllEnumsAsStrings();
                });
            }



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            LoggerFactory.AddConsole(Configuration.GetSection("Logging"));
            LoggerFactory.AddDebug();

            app.UseResponseCompression();
            app.UseResponseCaching();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            
            app.UseMvc();
            
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUi();
            }

            app.UseWelcomePage();


        }
    }
}
