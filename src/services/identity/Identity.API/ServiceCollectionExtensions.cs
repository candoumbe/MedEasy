namespace Identity.API
{
    using AutoMapper;

    using FluentValidation;
    using FluentValidation.AspNetCore;

    using Identity.API.Features.Auth;
    using Identity.CQRS.Handlers;
    using Identity.CQRS.Handlers.EFCore.Commands.Accounts;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DataStores;
    using Identity.Ids;
    using Identity.Mapping;
    using Identity.Objects;
    using Identity.Validators;

    using MedEasy.Abstractions.ValueConverters;
    using MedEasy.Core.Filters;
    using MedEasy.Core.Infrastructure;
    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Handlers.Pipelines;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DataStores.Core;
    using MedEasy.Ids.Converters;

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
            services.AddControllers(config =>
            {
                config.Filters.Add<FormatFilterAttribute>();
                config.Filters.Add<ValidateModelActionFilterAttribute>();
                config.Filters.Add<AddCountHeadersFilterAttribute>();
                ////options.Filters.Add(typeof(EnvelopeFilterAttribute));
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
                jsonSerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory());
                jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonSerializerOptions.WriteIndented = true;
                jsonSerializerOptions.PropertyNameCaseInsensitive = true;
                jsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            })

            .AddFluentValidation(options =>
            {
                options.LocalizationEnabled = true;

                options.RegisterValidatorsFromAssemblyContaining<LoginInfoValidator>();
                options.RunDefaultMvcValidationAfterFluentValidationExecutes = true;
            });

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
            Option<Uri> optionalHttps = configuration.GetServiceUri("identity-api", "https")
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
            services.Configure<IdentityApiOptions>((options) =>
            {
                options.DefaultPageSize = configuration.GetValue($"APIOptions:{nameof(IdentityApiOptions.DefaultPageSize)}", 30);
                options.MaxPageSize = configuration.GetValue($"APIOptions:{nameof(IdentityApiOptions.DefaultPageSize)}", 100);
            });
            services.Configure<JwtOptions>((options) =>
            {
                options.Key = configuration.GetValue<string>($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Key)}");
                options.Issuer = configuration.GetValue<string>($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)}");
                options.Audiences = configuration.GetSection($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Audiences)}")
                                                 .GetChildren()
                                                 .Select(x => x.Value)
                                                 .Distinct();
                options.AccessTokenLifetime = configuration.GetValue($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.AccessTokenLifetime)}", 10d);
                options.RefreshTokenLifetime = configuration.GetValue($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.RefreshTokenLifetime)}", 20d);
            });

            return services;
        }

        /// <summary>
        /// Adds required dependencies to access APÏ datastores
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddDataStores(this IServiceCollection services)
        {
            services.AddTransient(serviceProvider =>
            {
                DbContextOptionsBuilder<IdentityContext> optionsBuilder = BuildDbContextOptions(serviceProvider);
                IClock clock = serviceProvider.GetRequiredService<IClock>();
                return new IdentityContext(optionsBuilder.Options, clock);
            });

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<IdentityContext>>(serviceProvider =>
            {
                DbContextOptionsBuilder<IdentityContext> builder = BuildDbContextOptions(serviceProvider);
                IClock clock = serviceProvider.GetRequiredService<IClock>();

                return new EFUnitOfWorkFactory<IdentityContext>(builder.Options,
                                                                options => new IdentityContext(options, clock));
            });

            services.AddAsyncInitializer<DataStoreMigrateInitializerAsync<IdentityContext>>();

            return services;

            static DbContextOptionsBuilder<IdentityContext> BuildDbContextOptions(IServiceProvider serviceProvider)
            {
                IHostEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
                DbContextOptionsBuilder<IdentityContext> builder = new();
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                string connectionString = configuration.GetConnectionString("Identity");

                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    builder.UseSqlite(connectionString,
                                      options => options.UseNodaTime()
                                                        .MigrationsAssembly("Identity.DataStores.Sqlite"));
                }
                else
                {
                    builder.UseNpgsql(connectionString,
                                      options => options.EnableRetryOnFailure(5)
                                                        .UseNodaTime()
                                                        .MigrationsAssembly("Identity.DataStores.Postgres"));
                }
                builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
                       .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>()
                       .ConfigureWarnings(options =>
                       {
                           options.Default(WarningBehavior.Log);
                       });
                return builder;
            }
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

            services.AddScoped(sp =>
            {
                IHttpContextAccessor contextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                IApiVersioningFeature apiVersioningFeature = contextAccessor.HttpContext.Features.Get<IApiVersioningFeature>();
                return apiVersioningFeature?.RequestedApiVersion;
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
                typeof(GetOneAccountByUsernameAndPasswordQuery).Assembly,
                typeof(HandleCreateAccountInfoCommand).Assembly
            );

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));

            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton<IHandleCreateSecurityTokenCommand, HandleCreateJwtSecurityTokenCommand>();
            services.AddSingleton(AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
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
                       ValidateLifetime = true,
                       LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
                        {
                            using IServiceScope scope = services.BuildServiceProvider().CreateScope();
                            IValidator<SecurityToken> securityTokenValidator = scope.ServiceProvider.GetRequiredService<IValidator<SecurityToken>>();

                            return securityTokenValidator.Validate(securityToken).IsValid;
                        },
                       RequireExpirationTime = false,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)}"],
                       ValidAudiences = configuration.GetSection($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Audiences)}")
                            .GetChildren()
                            .Select(x => x.Value)
                            .Distinct(),
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[$"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Key)}"])),
                   };
                   options.Validate();
               });

            return services;
        }

        /// <summary>
        /// Adds custom middleware for performing healthchecks
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddCustomHealthChecks(this IServiceCollection services)
        {
            IHealthChecksBuilder healthChecksBuilder = services.AddHealthChecks();
            return healthChecksBuilder
                                  .AddDbContextCheck(customTestQuery: (Func<IdentityContext, System.Threading.CancellationToken, System.Threading.Tasks.Task<bool>>)(async (context, ct) => await context.Set<Account>().AnyAsync(ct)));
        }

        /// <summary>
        /// Adds Swagger middlewares
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IHostEnvironment hostingEnvironment, IConfiguration configuration)
        {

            (string applicationName, string applicationBasePath) = (System.Reflection.Assembly.GetEntryAssembly().GetName().Name, AppDomain.CurrentDomain.BaseDirectory);

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "REST API for Identity management",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Email = configuration.GetValue("Swagger:Contact:Email", string.Empty),
                        Name = configuration.GetValue("Swagger:Contact:Name", string.Empty),
                        //Url = configuration.GetValue<Uri>("Swagger:Contact:Url", Uri.Empty)
                    }
                });

                config.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "REST API for Identity management",
                    Version = "v2",
                    Contact = new OpenApiContact
                    {
                        Email = configuration.GetValue("Swagger:Contact:Email", string.Empty),
                        Name = configuration.GetValue("Swagger:Contact:Name", string.Empty),
                        //Url = configuration.GetValue<Uri>("Swagger:Contact:Url", string.Empty)
                    }
                });

                config.IgnoreObsoleteActions();
                config.IgnoreObsoleteProperties();
                string documentationPath = Path.Combine(applicationBasePath, $"{applicationName}.xml");
                if (File.Exists(documentationPath))
                {
                    config.IncludeXmlComments(documentationPath);
                }

                OpenApiSecurityScheme securityScheme = new()
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Token to access the API",
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                };
                config.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

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

                config.ConfigureForStronglyTypedIdsInAssembly<AccountId>();
            });

            return services;
        }
    }
}
