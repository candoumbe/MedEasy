using AutoMapper;

using FluentValidation.AspNetCore;

using Measures.API.Features.Auth;
using Measures.Context;
using Measures.CQRS.Handlers.BloodPressures;
using Measures.CQRS.Queries.BloodPressures;
using Measures.CQRS.Queries.Patients;
using Measures.Mapping;
using Measures.Validators.Commands.BloodPressures;

using MedEasy.Core.Filters;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Swashbuckle.AspNetCore.Swagger;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static Microsoft.AspNetCore.Http.StatusCodes;
using static Newtonsoft.Json.DateFormatHandling;
using static Newtonsoft.Json.DateTimeZoneHandling;

namespace Measures.API
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
        /// <param name="env"></param>
        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            services.AddControllers(config =>
            {
                config.Filters.Add<FormatFilterAttribute>();
                config.Filters.Add<ValidateModelActionFilter>();
                config.Filters.Add<AddCountHeadersFilterAttribute>();
                ////options.Filters.Add(typeof(EnvelopeFilterAttribute));
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
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.DateFormatHandling = IsoDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = Utc;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
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

            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = configuration.GetValue<int>("HttpsPort", 63796);
                options.RedirectStatusCode = Status307TemporaryRedirect;
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
            static DbContextOptionsBuilder<MeasuresContext> BuildDbContextOptions(IServiceProvider serviceProvider)
            {
                IHostEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
                DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    builder.UseInMemoryDatabase($"{Guid.NewGuid()}");
                }
                else
                {
                    IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    builder.UseNpgsql(
                        configuration.GetConnectionString("Measures"),
                        options => options.EnableRetryOnFailure(5)
                            .MigrationsAssembly(typeof(MeasuresContext).Assembly.FullName)
                    );
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
                DbContextOptionsBuilder<MeasuresContext> optionsBuilder = BuildDbContextOptions(serviceProvider);

                return new MeasuresContext(optionsBuilder.Options);
            });

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<MeasuresContext>>(serviceProvider =>
            {
                DbContextOptionsBuilder<MeasuresContext> builder = BuildDbContextOptions(serviceProvider);

                return new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, options => new MeasuresContext(options));
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
                options.DefaultApiVersion = new ApiVersion(2, 0);
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
                typeof(GetPatientInfoByIdQuery).Assembly,
                typeof(HandleGetPageOfBloodPressureInfoQuery).Assembly
            );
            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton(AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);
            services.AddHttpContextAccessor();

            services.AddScoped<ApiVersion>(sp =>
            {
                HttpContext http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
                return http.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
            });

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

                OpenApiSecurityScheme bearerSecurityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Token to access the API",
                    Type = SecuritySchemeType.Http
                };

                config.AddSecurityDefinition("bearerAuth", bearerSecurityScheme);
                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [bearerSecurityScheme] = new List<string>()
                });

                config.CustomSchemaIds(type => type.FullName);
            });

            return services;
        }
    }
}
