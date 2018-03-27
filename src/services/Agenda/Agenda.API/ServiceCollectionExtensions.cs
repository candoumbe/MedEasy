using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.DataStores;
using Agenda.Mapping;
using Agenda.Validators;
using AutoMapper;
using FluentValidation.AspNetCore;
using MedEasy.Abstractions;
using MedEasy.Core.Filters;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using MediatR;
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

namespace Agenda.API
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
        public static void AddCustomizedMvc(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(FormatFilter));
                options.Filters.Add(typeof(ValidateModelActionFilter));
                options.Filters.Add(typeof(HandleErrorAttribute));
                options.Filters.Add(typeof(AddCountHeadersFilterAttribute));
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

            })
            .AddFluentValidation(options =>
            {
                options.LocalizationEnabled = true;
                options
                    .RegisterValidatorsFromAssemblyContaining<PaginationConfigurationValidator>()
                    .RegisterValidatorsFromAssemblyContaining<NewAppointmentInfoValidator>()
                    ;
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
            services.Configure<AgendaApiOptions>((options) =>
            {
                options.DefaultPageSize = configuration.GetValue("ApiOptions:DefaultPageSize", 30);
                options.MaxPageSize = configuration.GetValue("ApiOptions:MaxPageSize", 100);
            });
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
        /// Adds required dependencies to access API datastores
        /// </summary>
        /// <param name="services"></param>
        /// 
        /// 
        public static void AddDataStores(this IServiceCollection services)
        {
            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<AgendaContext>>(serviceProvider =>
            {
                IHostingEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
                DbContextOptionsBuilder<AgendaContext> builder = new DbContextOptionsBuilder<AgendaContext>();
                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    string dbName = $"InMemoryDb_{Guid.NewGuid()}";
                    builder.UseInMemoryDatabase(dbName);
                }
                else
                {
                    IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    builder.UseSqlServer(configuration.GetConnectionString("Agenda"), b => b.MigrationsAssembly("Agenda.API"));
                }
                builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
                builder.ConfigureWarnings(options =>
                {
                    options.Default(WarningBehavior.Log);
                });

                return new EFUnitOfWorkFactory<AgendaContext>(builder.Options, (options) => new AgendaContext(options));

            });
        }

        /// <summary>
        /// Configure dependency injections
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomizedDependencyInjection(this IServiceCollection services)
        {
            services.AddMediatR(typeof(CreateAppointmentInfoCommand).Assembly);
            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton(provider => AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(provider => provider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);
            services.AddSingleton<IDateTimeService, DateTimeService>();
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
                    Description = "REST API for Agenda API",
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
