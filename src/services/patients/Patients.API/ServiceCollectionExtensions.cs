namespace Patients.API
{
    using AutoMapper;

    using FluentValidation;
    using FluentValidation.AspNetCore;

    using MassTransit;

    using MedEasy.Abstractions.ValueConverters;
    using MedEasy.Core.Filters;
    using MedEasy.Core.Infrastructure;
    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Handlers.Pipelines;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DataStores.Core;
    using MedEasy.Ids;

    using MediatR;

    using MicroElements.Swashbuckle.NodaTime;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;

    using NodaTime;
    using NodaTime.Serialization.SystemTextJson;

    using Optional;

    using Patients.Context;
    using Patients.CQRS.Commands;
    using Patients.CQRS.Handlers.Patients;
    using Patients.Ids;
    using Patients.Mapping;
    using Patients.Validators.Features.Patients.DTO;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using static Microsoft.AspNetCore.Http.StatusCodes;

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
        /// <param name="env"></param>
        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            services
                .AddControllers(config =>
                {
                    config.Filters.Add<FormatFilterAttribute>();
                    config.Filters.Add<ValidateModelActionFilterAttribute>();
                    config.Filters.Add<AddCountHeadersFilterAttribute>();

                    config.Filters.Add<HandleErrorAttribute>();

                    AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .Build();

                    config.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddJsonOptions(options =>
                {
                    JsonSerializerOptions jsonSerializerOptions = options.JsonSerializerOptions;
                    jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    jsonSerializerOptions.WriteIndented = true;
                    jsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                });
            services.AddFluentValidationAutoValidation(options => options.DisableDataAnnotationsValidation = false)
                    .AddFluentValidationClientsideAdapters()
                    .AddValidatorsFromAssemblyContaining<CreatePatientInfoValidator>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                    builder.AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowAnyOrigin()
                );
            });
            services.AddOptions();
            services.Configure<PatientsApiOptions>((options) =>
            {
                options.DefaultPageSize = configuration.GetValue($"APIOptions:{nameof(PatientsApiOptions.DefaultPageSize)}", 30);
                options.MaxPageSize = configuration.GetValue($"APIOptions:{nameof(PatientsApiOptions.DefaultPageSize)}", 100);
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    IDictionary<string, IEnumerable<string>> errors = context.ModelState
                        .Where(element => !string.IsNullOrWhiteSpace(element.Key))
                        .ToDictionary(item => item.Key, item => item.Value.Errors.Select(x => x.ErrorMessage).Distinct());
                    ValidationProblemDetails validationProblem = new()
                    {
                        Title = "Validation failed",
                        Detail = $"{errors.Count} validation errors",
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
            services.AddRouting(options =>
            {
                options.AppendTrailingSlash = false;
                options.LowercaseUrls = true;
            });

            Option<Uri> optionalHttps = configuration.GetServiceUri("patients-api", "https")
                                                     .SomeNotNull();
            optionalHttps.MatchSome(https =>
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = https.Port;
                    options.RedirectStatusCode = Status307TemporaryRedirect;
                });
            });

            return services;
        }

        /// <summary>
        /// Adds required dependencies to access APÏ datastores
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddDataStores(this IServiceCollection services, IConfiguration configuration)
        {
            static DbContextOptionsBuilder<PatientsDataStore> BuildDbContextOptions(IServiceProvider serviceProvider, IConfiguration configuration)
            {
                IHostEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
                DbContextOptionsBuilder<PatientsDataStore> builder = new();
                builder.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();

                string connectionString = configuration.GetConnectionString("Patients");

                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    builder.UseSqlite(connectionString, options =>
                    {
                        options.UseNodaTime()
                               .MigrationsAssembly("Patients.DataStores.Sqlite");
                    });
                }
                else
                {
                    builder.UseNpgsql(
                        connectionString,
                        options => options.EnableRetryOnFailure(5)
                                          .UseNodaTime()
                                          .MigrationsAssembly("Patients.DataStores.Postgres")
                                          .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    );
                }
                builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
                builder.ConfigureWarnings(options => options.Default(WarningBehavior.Log));
                return builder;
            }

            services.AddTransient(serviceProvider =>
            {
                DbContextOptionsBuilder<PatientsDataStore> optionsBuilder = BuildDbContextOptions(serviceProvider, configuration);
                IClock clock = serviceProvider.GetRequiredService<IClock>();

                return new PatientsDataStore(optionsBuilder.Options, clock);
            });

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<PatientsDataStore>>(serviceProvider =>
            {
                DbContextOptionsBuilder<PatientsDataStore> builder = BuildDbContextOptions(serviceProvider, configuration);

                IClock clock = serviceProvider.GetRequiredService<IClock>();

                return new EFUnitOfWorkFactory<PatientsDataStore>(builder.Options, options => new PatientsDataStore(options, clock));
            });

            services.AddAsyncInitializer<DataStoreMigrateInitializerAsync<PatientsDataStore>>();

            return services;
        }

        /// <summary>
        /// Configure dependency injections
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddMediatR(typeof(CreatePatientInfoCommand).Assembly,
                                typeof(HandleCreatePatientInfoCommand).Assembly);

            services.AddSingleton(AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);

            services.AddHttpContextAccessor();
            services.AddScoped(serviceProvider =>
            {
                HttpContext http = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                return http.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
            });

            services.AddSingleton<IClock>(SystemClock.Instance);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));

            services.AddScoped<IHandleCreatePatientInfoCommand, HandleCreatePatientInfoCommand>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();

            return services;
        }

        /// <summary>
        /// Adds custom authentication
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddCustomAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthorization()
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            RequireExpirationTime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)}"],
                            ValidAudience = configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Audience)}"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Key)}"]))
                        };
                        options.Validate();
                    });

            return services;
        }

        /// <summary>
        /// Adds Swagger middlewares
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environment"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddCustomizedSwagger(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
        {
            (string applicationName, string applicationBasePath) = (System.Reflection.Assembly.GetEntryAssembly().GetName().Name, AppDomain.CurrentDomain.BaseDirectory);

            services.AddSwaggerGen(config =>
            {
                string contactUrl = configuration.GetValue("Swagger:Contact:Url", string.Empty);
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = environment.ApplicationName,
                    Description = "REST API for Patients management",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Email = configuration.GetValue("Swagger:Contact:Email", string.Empty),
                        Name = configuration.GetValue("Swagger:Contact:Name", string.Empty),
                        Url = string.IsNullOrWhiteSpace(contactUrl)
                            ? null
                            : new Uri(contactUrl)
                    }
                });

                config.IgnoreObsoleteActions();
                config.IgnoreObsoleteProperties();
                string documentationPath = Path.Combine(applicationBasePath, $"{applicationName}.xml");
                if (File.Exists(documentationPath))
                {
                    config.IncludeXmlComments(documentationPath);
                }

                OpenApiSecurityScheme bearerSecurityScheme = new()
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Token to access the API",
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                };
                config.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerSecurityScheme);

                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new()
                            {
                                Id = JwtBearerDefaults.AuthenticationScheme,
                                Type = ReferenceType.SecurityScheme
                            },
                        },
                        new List<string>()
                    }
                });
                config.ConfigureForNodaTimeWithSystemTextJson();
                config.ConfigureForStronglyTypedIdsInAssembly<DoctorId>();
                config.ConfigureForStronglyTypedIdsInAssembly<TenantId>();
            });

            return services;
        }

        /// <summary>
        /// Adds a customized MassTransit to the dependency injection container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environment"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                if (environment.IsEnvironment("IntegrationTest"))
                {
                    x.UsingInMemory();
                }
                else
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(configuration.GetServiceUri(name: "message-bus", binding: "internal"));
                        cfg.ConfigureEndpoints(context);
                    });
                }
            });

            services.AddMassTransitHostedService();

            return services;
        }

        /// <summary>
        /// Adds version
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.UseApiBehavior = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("api-version", "version"),
                    new QueryStringApiVersionReader("version", "v", "api-version")
                );
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
    }
}
