namespace Measures.API
{
    using AutoMapper;

    using FluentValidation.AspNetCore;

    using MassTransit;

    using Measures.API.Features.Auth;
    using Measures.CQRS.Handlers.BloodPressures;
    using Measures.CQRS.Queries.Subjects;
    using Measures.DataStores;
    using Measures.Ids;
    using Measures.Mapping;
    using Measures.Validators.Commands.BloodPressures;

    using MedEasy.Abstractions.ValueConverters;
    using MedEasy.Core.Filters;
    using MedEasy.Core.Infrastructure;
    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Handlers.Pipelines;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DataStores.Core;
    using MedEasy.Validators;

    using MediatR;

    using MicroElements.Swashbuckle.NodaTime;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Authorization;
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

    using System;
    using System.Collections.Generic;
    using System.IO;
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
            services.AddControllers(config =>
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
            .AddFluentValidation(options =>
            {
                options.LocalizationEnabled = true;

                options
                    .RegisterValidatorsFromAssemblyContaining<PatchBloodPressureInfoValidator>()
                    .RegisterValidatorsFromAssemblyContaining<PaginationConfigurationValidator>()
                    ;
            })
            .AddJsonOptions(options =>
            {
                JsonSerializerOptions jsonSerializerOptions = options.JsonSerializerOptions;
                jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonSerializerOptions.WriteIndented = true;
                jsonSerializerOptions.AllowTrailingCommas = true;
                jsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            })
            .AddXmlSerializerFormatters();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                    builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                );
            });

            services.AddRouting(options =>
            {
                options.AppendTrailingSlash = false;
                options.LowercaseUrls = true;
            });

            Option<Uri> optionalHttps = configuration.GetServiceUri("measures-api", "https")
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
        /// Adds custom options
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<MeasuresApiOptions>((options) =>
            {
                options.DefaultPageSize = configuration.GetValue($"ApiOptions:{nameof(MeasuresApiOptions.DefaultPageSize)}", 30);
                options.MaxPageSize = configuration.GetValue($"ApiOptions:{nameof(MeasuresApiOptions.DefaultPageSize)}", 100);
            });
            services.Configure<JwtOptions>((options) =>
            {
                options.Key = configuration.GetValue<string>($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Key)}");
                options.Issuer = configuration.GetValue<string>($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)}");
                options.Audience = configuration.GetValue<string>($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Audience)}");
            });

            return services;
        }

        /// <summary>
        /// Adds required dependencies to access APÏ datastores
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddDataStores(this IServiceCollection services)
        {
            static DbContextOptionsBuilder<MeasuresStore> BuildDbContextOptions(IServiceProvider serviceProvider)
            {
                IHostEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
                DbContextOptionsBuilder<MeasuresStore> builder = new();
                builder.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();

                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                string connectionString = configuration.GetConnectionString("measures");

                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    builder.UseSqlite(connectionString,
                                      options => options.UseNodaTime()
                                                        .MigrationsAssembly("Measures.DataStores.Sqlite"));
                }
                else
                {
                    builder.UseNpgsql(connectionString,
                                      options => options.EnableRetryOnFailure(5)
                                                        .UseNodaTime()
                                                        .MigrationsAssembly("Measures.DataStores.Postgres"));
                }
                builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
                builder.ConfigureWarnings(options =>
                {
                    options.Default(WarningBehavior.Log);
                });

                return builder;
            }

            services.AddTransient(serviceProvider =>
            {
                DbContextOptionsBuilder<MeasuresStore> optionsBuilder = BuildDbContextOptions(serviceProvider);
                IClock clock = serviceProvider.GetRequiredService<IClock>();

                return new MeasuresStore(optionsBuilder.Options, clock);
            });

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<MeasuresStore>>(serviceProvider =>
            {
                DbContextOptionsBuilder<MeasuresStore> builder = BuildDbContextOptions(serviceProvider);
                IClock clock = serviceProvider.GetRequiredService<IClock>();

                return new EFUnitOfWorkFactory<MeasuresStore>(builder.Options, options => new MeasuresStore(options, clock));
            });

            services.AddAsyncInitializer<DataStoreMigrateInitializerAsync<MeasuresStore>>();

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
                options.DefaultApiVersion = new ApiVersion(2, 0);
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

        /// <summary>
        /// Configure dependency injections
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddMediatR(
                typeof(GetSubjectInfoByIdQuery).Assembly,
                typeof(HandleGetPageOfBloodPressureInfoQuery).Assembly
            );

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));

            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton(AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);
            services.AddHttpContextAccessor();

            services.AddScoped<ApiVersion>(sp =>
            {
                HttpContext http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
                return http.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
            });


            services.AddSingleton<IClock>(SystemClock.Instance);

            return services;
        }

        /// <summary>
        /// Adds Authorization and authentication
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services
                   .AddAuthorization()
                   .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(options =>
                   {
                       options.TokenValidationParameters = new TokenValidationParameters
                       {
                           ValidateIssuer = true,
                           ValidateAudience = true,
                           ValidateLifetime = true,
                           ValidateIssuerSigningKey = true,
                           ValidIssuer = configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)}"],
                           ValidAudience = configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Audience)}"],
                           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Key)}"])),
                       };

                       options.Validate();
                   })
                   ;

            return services;
        }

        /// <summary>
        /// Adds Swagger middlewares
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddCustomizedSwagger(this IServiceCollection services, IHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            (string applicationName, string applicationBasePath) = (System.Reflection.Assembly.GetEntryAssembly().GetName().Name, AppDomain.CurrentDomain.BaseDirectory);

            services.AddSwaggerGen(config =>
            {
                string contactUrl = configuration.GetValue("Swagger:Contact:Url", string.Empty);
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = $"REST API for {hostingEnvironment.ApplicationName}",
                    Version = "1",
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

                config.CustomSchemaIds(type => type.FullName);
                config.ConfigureForNodaTimeWithSystemTextJson();
                config.ConfigureForStronglyTypedIdsInAssembly<SubjectId>();
            });

            return services;
        }


        /// <summary>
        /// Adds a customized MassTransit to the dependency injection container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<PatientCaseCreatedConsumer>();
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
    }
}
