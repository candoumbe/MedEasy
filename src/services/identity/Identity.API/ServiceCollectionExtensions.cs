using AutoMapper;

using FluentValidation;
using FluentValidation.AspNetCore;

using Identity.API.Features.Auth;
using Identity.CQRS.Handlers;
using Identity.CQRS.Handlers.EFCore.Commands.Accounts;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores;
using Identity.Mapping;
using Identity.Validators;

using MedEasy.Abstractions;
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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static Microsoft.AspNetCore.Http.StatusCodes;
using static Newtonsoft.Json.DateFormatHandling;
using static Newtonsoft.Json.DateTimeZoneHandling;

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
        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatHandling = IsoDateFormat,
                DateTimeZoneHandling = Utc,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
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

                options.RegisterValidatorsFromAssemblyContaining<LoginInfoValidator>();
                options.RunDefaultMvcValidationAfterFluentValidationExecutes = true;
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = jsonSerializerSettings.ReferenceLoopHandling;
                options.SerializerSettings.DateFormatHandling = jsonSerializerSettings.DateFormatHandling;
                options.SerializerSettings.DateTimeZoneHandling = jsonSerializerSettings.DateTimeZoneHandling;
                options.SerializerSettings.NullValueHandling = jsonSerializerSettings.NullValueHandling;
                options.SerializerSettings.Formatting = jsonSerializerSettings.Formatting;
                options.SerializerSettings.ContractResolver = jsonSerializerSettings.ContractResolver;

                options.AllowInputFormatterExceptionMessages = env.IsDevelopment();
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
            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = configuration.GetValue<int>("HttpsPort", 51800);
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
            static DbContextOptionsBuilder<IdentityContext> BuildDbContextOptions(IServiceProvider serviceProvider)
            {
                IHostEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
                DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    builder.UseInMemoryDatabase($"{Guid.NewGuid()}");
                }
                else
                {
                    IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    builder.UseNpgsql(
                        configuration.GetConnectionString("identity-db"),
                        options => options.EnableRetryOnFailure(5)
                            .MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)
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
                DbContextOptionsBuilder<IdentityContext> optionsBuilder = BuildDbContextOptions(serviceProvider);

                return new IdentityContext(optionsBuilder.Options);
            });

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<IdentityContext>>(serviceProvider =>
           {
               DbContextOptionsBuilder<IdentityContext> builder = BuildDbContextOptions(serviceProvider);

               return new EFUnitOfWorkFactory<IdentityContext>(builder.Options, options => new IdentityContext(options));
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
            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton<IHandleCreateSecurityTokenCommand, HandleCreateJwtSecurityTokenCommand>();
            services.AddSingleton(AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddHttpContextAccessor();

            services.AddSingleton<IDateTimeService, DateTimeService>();

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
                            using (IServiceScope scope = services.BuildServiceProvider().CreateScope())
                            {
                                IValidator<SecurityToken> securityTokenValidator = scope.ServiceProvider.GetRequiredService<IValidator<SecurityToken>>();

                                return securityTokenValidator.Validate(securityToken).IsValid;
                            }
                        },
                       RequireExpirationTime = true,
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
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "REST API for Identity management",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Email = configuration.GetValue("Swagger:Contact:Email", string.Empty),
                        Name = configuration.GetValue("Swagger:Contact:Name", string.Empty),
                        //Url = configuration.GetValue<Uri>("Swagger:Contact:Url", string.Empty)
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

                OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Token to access the API",
                    Type = SecuritySchemeType.ApiKey
                };
                config.AddSecurityDefinition("Bearer", securityScheme);

                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [securityScheme] = new List<string>()
                });

                config.CustomSchemaIds(type => type.FullName);
            });

            return services;
        }
    }
}
