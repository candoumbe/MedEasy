namespace Documents.API
{
    using AutoMapper;

    using Documents.CQRS.Queries;
    using Documents.DataStore;
    using Documents.Mapping;

    using FluentValidation.AspNetCore;

    using MedEasy.Core.Filters;
    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;

    using MediatR;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;

    using NodaTime;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Text.Json;

    using static Microsoft.AspNetCore.Http.StatusCodes;
    using NodaTime.Serialization.SystemTextJson;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using MedEasy.Abstractions.ValueConverters;
    using Documents.Ids;
    using MedEasy.Core.Infrastructure;
    using MedEasy.CQRS.Core.Handlers.Pipelines;
    using Optional;
    using MedEasy.DataStores.Core;

    /// <summary>
    /// Provide extension method used to configure services collection
    /// </summary>
    public static partial class ServiceCollectionExtensions
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

                // The following policy forces every request to be authenticated
                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

                config.Filters.Add(new AuthorizeFilter(policy));
            })
            .AddFluentValidation(options =>
            {
                options.LocalizationEnabled = true;
            })
            .AddJsonOptions(options =>
            {
                JsonSerializerOptions jsonSerializerOptions = options.JsonSerializerOptions;
                jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonSerializerOptions.WriteIndented = true;
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
            Option<Uri> optionalHttps = configuration.GetServiceUri("documents-api", "https")
                                                     .SomeNotNull();
            optionalHttps.MatchSome(https =>
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = https.Port;
                    options.RedirectStatusCode = Status307TemporaryRedirect;
                });
            });

            //services.AddSt

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
            services.Configure<DocumentsApiOptions>((options) =>
            {
                options.DefaultPageSize = configuration.GetValue($"APIOptions:{nameof(DocumentsApiOptions.DefaultPageSize)}", 30);
                options.MaxPageSize = configuration.GetValue($"APIOptions:{nameof(DocumentsApiOptions.DefaultPageSize)}", 100);
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
            static DbContextOptionsBuilder<DocumentsStore> BuildDbContextOptions(IServiceProvider serviceProvider)
            {
                IHostEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
                DbContextOptionsBuilder<DocumentsStore> builder = new();
                builder.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                string connectionString = configuration.GetConnectionString("documents");

                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    builder.UseSqlite(connectionString, options =>
                    {
                        options.UseNodaTime()
                               .MigrationsAssembly("Documents.DataStores.Sqlite");
                    });
                }
                else
                {
                    builder.UseNpgsql(
                        connectionString,
                        options => options.EnableRetryOnFailure(5)
                                          .UseNodaTime()
                                          .MigrationsAssembly("Documents.DataStores.Postgres")
                    );
                }
                builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
                builder.ConfigureWarnings(options =>
                {
                    options.Default(WarningBehavior.Log);
                });
                return builder;
            }

            // The following line is only required to be able to perform migration on startup
            services.AddTransient(serviceProvider =>
            {
                DbContextOptionsBuilder<DocumentsStore> optionsBuilder = BuildDbContextOptions(serviceProvider);
                IClock clock = serviceProvider.GetRequiredService<IClock>();

                return new DocumentsStore(optionsBuilder.Options, clock);
            });

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<DocumentsStore>>(serviceProvider =>
           {
               DbContextOptionsBuilder<DocumentsStore> builder = BuildDbContextOptions(serviceProvider);
               IClock clock = serviceProvider.GetRequiredService<IClock>();

               return new EFUnitOfWorkFactory<DocumentsStore>(builder.Options, options => new DocumentsStore(options, clock));
           });

            services.AddAsyncInitializer<DataStoreMigrateInitializerAsync<DocumentsStore>>();

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

            services.AddScoped(provider =>
            {
                HttpContext httpContext = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                return httpContext.GetRequestedApiVersion();
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
                typeof(GetOneDocumentInfoByIdQuery).Assembly
            );

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));

            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton(AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);

            services.AddHttpContextAccessor();
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
            services.AddAuthorization()
               .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       RequireAudience = true,
                       ValidateLifetime = true,
                       RequireExpirationTime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)}"],
                       ValidAudience = configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Audience)}"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Key)}"])),
                   };
                   options.Validate();
               });

            return services;
        }

        /// <summary>
        /// Adds Swagger middlewares
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddSwagger(this IServiceCollection services, IHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            (string applicationName, string applicationBasePath) = (System.Reflection.Assembly.GetEntryAssembly().GetName().Name, AppDomain.CurrentDomain.BaseDirectory);

            services.AddSwaggerGen(config =>
            {
                string contactUrl = configuration.GetValue("Swagger:Contact:Url", string.Empty);
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = $"REST API for {hostingEnvironment.ApplicationName}",
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

                config.ConfigureForStronglyTypedIdsInAssembly<DocumentId>();
            });

            return services;
        }
    }
}
