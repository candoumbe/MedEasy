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
using MedEasy.RestObjects;
using Swashbuckle.Swagger.Model;
using MedEasy.API.Filters;

namespace MedEasy.API
{
    public class Startup
    {

        private IHostingEnvironment HostingEnvironment { get; }
        private IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Builds a new <see cref="Startup"/> instance
        /// </summary>
        /// <param name="env">hosting environment configuration</param>
        public Startup(IHostingEnvironment env)
        {
            HostingEnvironment = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton((provider) => AutoMapperConfig.Build().CreateMapper());

            services.AddTransient<IUnitOfWorkFactory, EFUnitOfWorkFactory>(item =>
            {
                DbContextOptionsBuilder<MedEasyContext> builder = new DbContextOptionsBuilder<MedEasyContext>();
                builder.UseSqlServer(Configuration.GetConnectionString("Default"));
                return new EFUnitOfWorkFactory(builder.Options);

            });

            services.AddScoped(x => AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder);

            services.AddPatientsControllerDependencies();
            services.AddSpecialtiesControllerDependencies();
            services.AddDoctorsControllerDependencies();

            services.AddLogging();

            services.AddRouting();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddOptions();
            services.Configure<MedEasyApiOptions>((options) =>
            {
                options.DefaultPageSize = Configuration.GetValue("APIOptions:DefaultPageSize", 30);
                options.MaxPageSize = Configuration.GetValue("APIOptions:DefaultPageSize", 100);
            });
            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(EnvelopeFilterAttribute));
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
                    config.MapType<GenericGetQuery>(() => new Schema
                    {
                        Properties =
                        {
                            [nameof(GenericGetQuery.Page)] = new Schema
                                {
                                    Type = "int",
                                    Minimum = 1,
                                    Default = 1,
                                    Description = "Index of the page to get"
                                },
                            [nameof(GenericGetQuery.PageSize)] = new Schema
                            {
                                Type = "int",
                                Minimum = 1,
                                Default = 1,
                                Description = "Number of items a page of result can contain at most"
                            }
                        }
                    });
                });
            }



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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
