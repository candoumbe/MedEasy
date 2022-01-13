namespace Agenda.API
{
    using Agenda.CQRS.Features.Appointments.Commands;
    using Agenda.DataStores;
    using Agenda.Ids;
    using Agenda.Mapping;
    using Agenda.Validators;

    using AutoMapper;

    using FluentValidation.AspNetCore;

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
        private const string IntegrationTestEnvironmentName = "IntegrationTest";

        /// <summary>
        /// Configure service for MVC
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        public static IServiceCollection AddCustomizedMvc(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            services
                .AddCors(options =>
                {
                    options.AddPolicy("AllowAnyOrigin", builder =>
                        builder.AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin()
                    //.AllowCredentials()
                    );
                })
                .AddControllers(options =>
                {
                    options.Filters.Add<FormatFilterAttribute>();
                    options.Filters.Add<HandleErrorAttribute>();
                    options.Filters.Add<AddCountHeadersFilterAttribute>();

                    AuthorizationPolicy policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                       .RequireAuthenticatedUser()
                       .Build();

                    options.Filters.Add(new AuthorizeFilter(policy));
                })
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
                    JsonSerializerOptions jsonSerializerOptions = options.JsonSerializerOptions;
                    jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    jsonSerializerOptions.WriteIndented = true;
                    jsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                });

            services.AddRouting(opts =>
            {
                opts.AppendTrailingSlash = false;
                opts.LowercaseUrls = true;
            });

            services.AddHsts(options =>
            {
                options.Preload = true;
                if (env.IsDevelopment() || env.IsEnvironment(IntegrationTestEnvironmentName))
                {
                    options.ExcludedHosts.Remove("localhost");
                    options.ExcludedHosts.Remove("127.0.0.1");
                    options.ExcludedHosts.Remove("[::1]");
                }
            });

            Option<Uri> optionalHttps = configuration.GetServiceUri("agenda-api", "https")
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
        /// Adds required dependencies to access API datastores
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddDataStores(this IServiceCollection services, IConfiguration configuration)
        {
            static DbContextOptionsBuilder<AgendaContext> BuildDbContextOptions(IServiceProvider serviceProvider, IConfiguration configuration)
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                IHostEnvironment hostingEnvironment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
                DbContextOptionsBuilder<AgendaContext> builder = new();
                builder.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();

                string connectionString = configuration.GetConnectionString("Agenda");

                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    builder.UseSqlite(connectionString, options =>
                    {
                        options.UseNodaTime()
                               .MigrationsAssembly("Agenda.DataStores.Sqlite");
                    });
                }
                else
                {
                    builder.UseNpgsql(connectionString,
                                    options => options.EnableRetryOnFailure(5).UseNodaTime()
                                                      .MigrationsAssembly("Agenda.DataStores.Postgres")
                    );
                }
                builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
                builder.ConfigureWarnings(options => options.Default(WarningBehavior.Log));
                return builder;
            }

            using IServiceScope scope = services.BuildServiceProvider().CreateScope();

            services.AddTransient(serviceProvider =>
            {
                DbContextOptionsBuilder<AgendaContext> optionsBuilder = BuildDbContextOptions(serviceProvider, configuration);
                IClock clock = serviceProvider.GetRequiredService<IClock>();
                return new AgendaContext(optionsBuilder.Options, clock);
            });

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<AgendaContext>>(serviceProvider =>
            {
                DbContextOptionsBuilder<AgendaContext> builder = BuildDbContextOptions(serviceProvider, configuration);

                IClock clock = serviceProvider.GetRequiredService<IClock>();
                return new EFUnitOfWorkFactory<AgendaContext>(builder.Options, options => new AgendaContext(options, clock));
            });

            services.AddAsyncInitializer<DataStoreMigrateInitializerAsync<AgendaContext>>();

            return services;
        }

        /// <summary>
        /// Adds supports for Options
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<AgendaApiOptions>((options) =>
            {
                options.DefaultPageSize = configuration.GetValue($"ApiOptions:{nameof(AgendaApiOptions.DefaultPageSize)}", 30);
                options.MaxPageSize = configuration.GetValue($"ApiOptions:{nameof(AgendaApiOptions.DefaultPageSize)}", 100);
            });

            services.Configure<JwtOptions>((options) =>
            {
                options.Issuer = configuration.GetValue<string>($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Issuer)}");
                options.Audience = configuration.GetValue<string>($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Audience)}");
                options.Key = configuration.GetValue<string>($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.Key)}");
            });

            return services;
        }

        /// <summary>
        /// Configure dependency injection container
        /// </summary>
        /// <param name="services"></param>
        /// <remarks>
        /// Adds the
        /// </remarks>
        public static IServiceCollection AddCustomizedDependencyInjection(this IServiceCollection services)
        {
            services.AddMediatR(typeof(CreateAppointmentInfoCommand).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));

            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton(_ => AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(provider => provider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);

            services.AddSingleton<IClock>(SystemClock.Instance);

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
                string url = configuration.GetValue("Swagger:Contact:Url", string.Empty);
                OpenApiContact contact = new()
                {
                    Email = configuration.GetValue("Swagger:Contact:Email", string.Empty),
                    Name = configuration.GetValue("Swagger:Contact:Name", string.Empty),
                    Url = string.IsNullOrWhiteSpace(url) ? null : new Uri(url)
                };
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "REST API for Agenda API",
                    Version = "v1",
                    Contact = contact
                });
                config.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "Agenda REST API v2",
                    Version = "v2",
                    Contact = contact
                });

                config.ConfigureForNodaTime();

                config.IgnoreObsoleteActions();
                config.IgnoreObsoleteProperties();
                string documentationPath = Path.Combine(applicationBasePath, $"{applicationName}.xml");
                if (File.Exists(documentationPath))
                {
                    config.IncludeXmlComments(documentationPath);
                }

                config.ConfigureForStronglyTypedIdsInAssembly<AppointmentId>();
            });

            return services;
        }

        /// <summary>
        /// Configures the authentication middleware
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddCustomAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
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
