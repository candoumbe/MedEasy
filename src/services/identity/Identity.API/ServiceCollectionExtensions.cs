using AutoMapper;
using Identity.CQRS;
using Identity.DataStores.SqlServer;
using Identity.Mapping;
using Identity.Validators;
using MedEasy.Core.Filters;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using static Newtonsoft.Json.DateFormatHandling;
using static Newtonsoft.Json.DateTimeZoneHandling;
using MediatR;
using FluentValidation.AspNetCore;
using Identity.CQRS.Queries.Accounts;

namespace Identity.API
{
    /// <summary>
    /// Provide extension method used to configure services collection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure service for MVC
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void CustomizeMvc(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(FormatFilter));
                options.Filters.Add(typeof(ValidateModelActionFilter));
                options.Filters.Add(typeof(AddCountHeadersFilterAttribute));
                ////options.Filters.Add(typeof(EnvelopeFilterAttribute));
                options.Filters.Add(typeof(HandleErrorAttribute));
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

            })
            .AddFluentValidation(options =>
            {
                options.LocalizationEnabled = true;
                
                options.RegisterValidatorsFromAssemblyContaining<LoginInfoValidator>();
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.DateFormatHandling = IsoDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = Utc;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                    builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                );
            });
            services.AddOptions();
            //services.Configure<IdentityApiOptions>((options) =>
            //{
            //    options.DefaultPageSize = configuration.GetValue("APIOptions:DefaultPageSize", 30);
            //    options.MaxPageSize = configuration.GetValue("APIOptions:MaxPageSize", 100);
            //});
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAnyOrigin"));
            });

            services.AddRouting(options =>
            {
                options.AppendTrailingSlash = false;
                options.LowercaseUrls = true;
            });
        }

        /// <summary>
        /// Adds required dependencies to access APÏ datastores
        /// </summary>
        /// <param name="services"></param>
        /// 
        /// 
        public static void AddDataStores(this IServiceCollection services)
        {
            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<IdentityContext>>(serviceProvider =>
            {
                IHostingEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
                DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    string dbName = $"InMemoryDb_{Guid.NewGuid()}";
                    builder.UseInMemoryDatabase(dbName);
                }
                else
                {
                    IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    builder.UseSqlServer(configuration.GetConnectionString("Identity"), b => b.MigrationsAssembly("Identity.API"));
                }
                builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
                builder.ConfigureWarnings(options =>
                {
                    options.Default(WarningBehavior.Log);
                });

                return new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) => new IdentityContext(options));

            });
        }

        /// <summary>
        /// Configure dependency injections
        /// </summary>
        /// <param name="services"></param>
        public static void CustomizeDependencyInjection(this IServiceCollection services)
        {
            services.AddMediatR(typeof(GetOneAccountByUsernameAndPasswordQuery).Assembly);
            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton(provider => AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(provider => provider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(builder =>
            {
                IUrlHelperFactory urlHelperFactory = builder.GetRequiredService<IUrlHelperFactory>();
                IActionContextAccessor actionContextAccessor = builder.GetRequiredService<IActionContextAccessor>();

                return urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            });


        }

        /// <summary>
        /// Adds Swagger middlewares
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="configuration"></param>
        public static void AddCustomizedSwagger(this IServiceCollection services, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            ApplicationEnvironment app = PlatformServices.Default.Application;
            
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Info
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "REST API for Identity management API",
                    Version = "v1",
                    Contact = new Contact
                    {
                        Email = configuration.GetValue("Swagger:Contact:Email", string.Empty),
                        Name = configuration.GetValue("Swagger:Contact:Name", string.Empty),
                        Url = configuration.GetValue("Swagger:Contact:Url", string.Empty)
                    }
                });

                config.IgnoreObsoleteActions();
                config.IgnoreObsoleteProperties();
                string documentationPath = Path.Combine(app.ApplicationBasePath, $"{app.ApplicationName}.xml");
                if (File.Exists(documentationPath))
                {
                    config.IncludeXmlComments(documentationPath);
                }
                config.DescribeStringEnumsInCamelCase();
                config.DescribeAllEnumsAsStrings();
            });
        }
    }
}
