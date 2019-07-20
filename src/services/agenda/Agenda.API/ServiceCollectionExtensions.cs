using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.DataStores;
using Agenda.Mapping;
using Agenda.Validators;
using AutoMapper;
using FluentValidation.AspNetCore;
using MedEasy.Abstractions;
using MedEasy.Core.Filters;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Newtonsoft.Json.DateFormatHandling;
using static Newtonsoft.Json.DateTimeZoneHandling;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

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
        public static IServiceCollection AddCustomizedMvc(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment env)
        {
            services
                .AddCors(options =>
                {
                    options.AddPolicy("AllowAnyOrigin", builder =>
                        builder.AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin()
                    );
                })
                .AddMvc(options =>
                {
                    options.Filters.Add<FormatFilterAttribute>();
                    options.Filters.Add<ValidateModelActionFilter>();
                    options.Filters.Add<HandleErrorAttribute>();
                    options.Filters.Add<AddCountHeadersFilterAttribute>();
                    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                    options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAnyOrigin"));

                    // This option is just to fix a breaking change in ASP.NETCORE 2.2 Routing (see https://github.com/aspnet/AspNetCore/issues/5055) 
                    options.EnableEndpointRouting = false;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddFluentValidation(options =>
                {
                    options.RunDefaultMvcValidationAfterFluentValidationExecutes = true;
                    options.LocalizationEnabled = true;
                    options
                        .RegisterValidatorsFromAssemblyContaining<PaginationConfigurationValidator>()
                        .RegisterValidatorsFromAssemblyContaining<NewAppointmentModelValidator>()
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

            services.AddOptions();
            services.Configure<AgendaApiOptions>((options) =>
            {
                options.DefaultPageSize = configuration.GetValue($"ApiOptions:{nameof(AgendaApiOptions.DefaultPageSize)}", 30);
                options.MaxPageSize = configuration.GetValue($"ApiOptions:{nameof(AgendaApiOptions.DefaultPageSize)}", 100);
            });
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    IDictionary<string, IEnumerable<string>> errors = context.ModelState
                        .Where(element => !string.IsNullOrWhiteSpace(element.Key))
                        .ToDictionary(item => item.Key, item => item.Value.Errors.Select(x => x.ErrorMessage).Distinct());
                    ValidationProblemDetails validationProblem = new ValidationProblemDetails
                    {
                        Title = "Validation failed",
                        Detail = $"{errors.Count} validation error{(errors.Count > 1 ? "s" : string.Empty)}",
                        Status = context.HttpContext.Request.Method == HttpMethods.Get || context.HttpContext.Request.Method == HttpMethods.Head
                            ? Status400BadRequest
                            : Status422UnprocessableEntity
                    };
                    foreach ((string key, IEnumerable<string> details) in errors)
                    {
                        validationProblem.Errors.Add(key, details.ToArray());
                    }

                    return new BadRequestObjectResult(validationProblem);
                };
            });

            services.Configure<LinkOptions>(options =>
            {
                options.AppendTrailingSlash = false;
                options.LowercaseUrls = true;
            });

            services.AddHsts(options =>
            {
                options.Preload = true;
                if (env.IsDevelopment() || env.IsEnvironment("IntegrationTest"))
                {
                    options.ExcludedHosts.Remove("localhost");
                    options.ExcludedHosts.Remove("127.0.0.1");
                    options.ExcludedHosts.Remove("[::1]");
                }
            });
            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = configuration.GetValue<int>("HttpsPort", 53172);
                options.RedirectStatusCode = Status307TemporaryRedirect;
            });

            return services;
        }

        private static DbContextOptionsBuilder<AgendaContext> BuildDbContextOptions(IServiceProvider serviceProvider)
        {
            IHostingEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
            DbContextOptionsBuilder<AgendaContext> builder = new DbContextOptionsBuilder<AgendaContext>();
            if (hostingEnvironment.IsEnvironment("IntegrationTest"))
            {
                builder.UseInMemoryDatabase($"{Guid.NewGuid()}");
            }
            else
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                builder.UseSqlServer(
                    configuration.GetConnectionString("Agenda"),
                    options => options.EnableRetryOnFailure(5)
                        .MigrationsAssembly(typeof(AgendaContext).Assembly.FullName)
                );
            }
            builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
            builder.ConfigureWarnings(options => options.Default(WarningBehavior.Log));
            return builder;
        }

        /// <summary>
        /// Adds required dependencies to access API datastores
        /// </summary>
        /// <param name="services"></param>
        /// 
        /// 
        public static IServiceCollection AddDataStores(this IServiceCollection services)
        {
            DbContextOptionsBuilder<AgendaContext> BuildDbContextOptions(IServiceProvider serviceProvider)
            {
                IHostingEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
                DbContextOptionsBuilder<AgendaContext> builder = new DbContextOptionsBuilder<AgendaContext>();
                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    builder.UseInMemoryDatabase($"{Guid.NewGuid()}");
                }
                else
                {
                    IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    builder.UseSqlServer(
                        configuration.GetConnectionString("Agenda"),
                        options => options.EnableRetryOnFailure(5)
                            .MigrationsAssembly(typeof(AgendaContext).Assembly.FullName)
                    );
                }
                builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
                builder.ConfigureWarnings(options => options.Default(WarningBehavior.Log));
                return builder;
            }

            services.AddTransient(serviceProvider =>
            {
                DbContextOptionsBuilder<AgendaContext> optionsBuilder = BuildDbContextOptions(serviceProvider);

                return new AgendaContext(optionsBuilder.Options);
            });

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<AgendaContext>>(serviceProvider =>
            {
                DbContextOptionsBuilder<AgendaContext> builder = BuildDbContextOptions(serviceProvider);

                return new EFUnitOfWorkFactory<AgendaContext>(builder.Options, options => new AgendaContext(options));
            });

            return services;
        }

        /// <summary>
        /// Configure dependency injections
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddCustomizedDependencyInjection(this IServiceCollection services)
        {
            services.AddMediatR(typeof(CreateAppointmentInfoCommand).Assembly);
            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton(_ => AutoMapperConfig.Build().CreateMapper());
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

            return services;
        }

        /// <summary>
        /// Adds Swagger middlewares
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddCustomizedSwagger(this IServiceCollection services, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            (string applicationName, string applicationBasePath) = (System.Reflection.Assembly.GetEntryAssembly().GetName().Name, AppDomain.CurrentDomain.BaseDirectory);

            services.AddSwaggerGen(config =>
            {
                Contact contact = new Contact
                {
                    Email = configuration.GetValue("Swagger:Contact:Email", string.Empty),
                    Name = configuration.GetValue("Swagger:Contact:Name", string.Empty),
                    Url = configuration.GetValue("Swagger:Contact:Url", string.Empty)
                };
                config.SwaggerDoc("v1", new Info
                    {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "REST API for Agenda API",
                    Version = "v1",
                    Contact = contact
                    });
                config.SwaggerDoc("v2", new Info
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "Agenda REST API v2",
                    Version = "v2",
                    Contact = contact
                });

                config.IgnoreObsoleteActions();
                config.IgnoreObsoleteProperties();
                string documentationPath = Path.Combine(applicationBasePath, $"{applicationName}.xml");
                if (File.Exists(documentationPath))
                {
                    config.IncludeXmlComments(documentationPath);
                }
                config.DescribeStringEnumsInCamelCase();
                config.DescribeAllEnumsAsStrings();
            });

            return services;
        }

        /// <summary>
        /// Configures the authentication middleware
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Authentication:JwtBearer:Issuer"],
                        ValidAudience = configuration["Authentication:JwtBearer:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:JwtBearer:Key"])),
                    };
                });

            return services;
        }

        /// <summary>
        /// Adds version
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options => {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.UseApiBehavior = true;
                options.ReportApiVersions = true;
            });
            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });

            return services;
        }

        /// <summary>
        /// Adds custom healthcheck
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services)
        {
            services.AddHealthChecks();

            return services;
        }
    }
}
