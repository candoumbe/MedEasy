using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Identity.API.Features.Auth;
using Identity.CQRS.Handlers;
using Identity.CQRS.Handlers.EFCore.Commands.Accounts;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores.SqlServer;
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
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment env)
        {
            services.AddMvc(config =>
            {
                config.Filters.Add<FormatFilterAttribute>();
                config.Filters.Add<ValidateModelActionFilter>();
                config.Filters.Add<AddCountHeadersFilterAttribute>();
                ////options.Filters.Add(typeof(EnvelopeFilterAttribute));
                config.Filters.Add<HandleErrorAttribute>();
                config.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .Build();

                config.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddFluentValidation(options =>
            {
                options.LocalizationEnabled = true;

                options.RegisterValidatorsFromAssemblyContaining<LoginInfoValidator>();
                options.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
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
                    .Select(x => x.Value);
                options.AccessTokenLifetime = configuration.GetValue($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.AccessTokenLifetime)}", 10d);
                options.RefreshTokenLifetime = configuration.GetValue($"Authentication:{nameof(JwtOptions)}:{nameof(JwtOptions.RefreshTokenLifetime)}", 20d);
            });
            services.Configure<MvcOptions>(options => options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAnyOrigin")));

            return services;

        }

        /// <summary>
        /// Adds required dependencies to access APÏ datastores
        /// </summary>
        /// <param name="services"></param>
        /// 
        /// 
        /// <summary>
        /// Adds required dependencies to access API datastores
        /// </summary>
        /// <param name="services"></param>
        /// 
        /// 
        public static IServiceCollection AddDataStores(this IServiceCollection services)
        {
            DbContextOptionsBuilder<IdentityContext> BuildDbContextOptions(IServiceProvider serviceProvider)
            {
                IHostingEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
                DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
                if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                {
                    builder.UseInMemoryDatabase($"{Guid.NewGuid()}");
                }
                else
                {
                    IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    builder.UseSqlServer(
                        configuration.GetConnectionString("Identity"),
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(builder =>
            {
                IUrlHelperFactory urlHelperFactory = builder.GetRequiredService<IUrlHelperFactory>();
                IActionContextAccessor actionContextAccessor = builder.GetRequiredService<IActionContextAccessor>();

                return urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            });

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
                            .Select(x => x.Value),
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
        public static IServiceCollection AddSwagger(this IServiceCollection services, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            (string applicationName, string applicationBasePath) = (System.Reflection.Assembly.GetEntryAssembly().GetName().Name, AppDomain.CurrentDomain.BaseDirectory);

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Info
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "REST API for Identity management",
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
                string documentationPath = Path.Combine(applicationBasePath, $"{applicationName}.xml");
                if (File.Exists(documentationPath))
                {
                    config.IncludeXmlComments(documentationPath);
                }
                config.DescribeStringEnumsInCamelCase();
                config.DescribeAllEnumsAsStrings();
                config.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Name = "Authorization",
                    In = "header",
                    Description = "Token to access the API",
                    Type = "apiKey"
                });
                config.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", Enumerable.Empty<string>() }
                });
            });

            return services;
        }
    }
}
