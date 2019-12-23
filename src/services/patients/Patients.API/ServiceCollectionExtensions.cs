using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using MedEasy.Abstractions;
using MedEasy.Core.Filters;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
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
using Patients.Context;
using Patients.Mapping;
using Patients.Validators.Features.Patients.DTO;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Newtonsoft.Json.DateFormatHandling;
using static Newtonsoft.Json.DateTimeZoneHandling;

namespace Patients.API
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
                options.RegisterValidatorsFromAssemblyContaining<CreatePatientInfoValidator>();
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
            services.Configure<PatientsApiOptions>((options) =>
            {
                options.DefaultPageSize = configuration.GetValue($"APIOptions:{nameof(PatientsApiOptions.DefaultPageSize)}", 30);
                options.MaxPageSize = configuration.GetValue($"APIOptions:{nameof(PatientsApiOptions.DefaultPageSize)}", 100);
            });

            services.Configure<MvcOptions>(options => options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAnyOrigin")));
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

#if NETCOREAPP2_1
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
                options.HttpsPort = configuration.GetValue<int>("HttpsPort", 54003);
                options.RedirectStatusCode = Status307TemporaryRedirect;
            });
#endif
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
                DbContextOptionsBuilder<PatientsContext> optionsBuilder = BuildDbContextOptions(serviceProvider);

                return new PatientsContext(optionsBuilder.Options);
            });

            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<PatientsContext>>(serviceProvider =>
           {
               IHostingEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
               DbContextOptionsBuilder<PatientsContext> builder = BuildDbContextOptions(serviceProvider);

               return new EFUnitOfWorkFactory<PatientsContext>(builder.Options, options => new PatientsContext(options));
           });

            return services;
        }

        /// <summary>
        /// Configure dependency injections
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
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

        private static DbContextOptionsBuilder<PatientsContext> BuildDbContextOptions(IServiceProvider serviceProvider)
        {
            IHostingEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
            DbContextOptionsBuilder<PatientsContext> builder = new DbContextOptionsBuilder<PatientsContext>();
            if (hostingEnvironment.IsEnvironment("IntegrationTest"))
            {
                builder.UseInMemoryDatabase($"{Guid.NewGuid()}");
            }
            else
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                builder.UseSqlServer(
                    configuration.GetConnectionString("Patients"),
                    options => options.EnableRetryOnFailure(5)
                        .MigrationsAssembly(typeof(PatientsContext).Assembly.FullName)
                );
            }
            builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
            builder.ConfigureWarnings(options =>
            {
                options.Default(WarningBehavior.Log);
            });
            return builder;
        }

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration) =>
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                                //                       IValidator<SecurityToken> securityTokenValidator = scope.ServiceProvider.GetRequiredService<IValidator<SecurityToken>>();
                                //WARNING Validate the token
                                return true;
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

        /// <summary>
        /// Adds Swagger middlewares
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="configuration"></param>
        public static void ConfigureSwagger(this IServiceCollection services, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            (string applicationName, string applicationBasePath) = (System.Reflection.Assembly.GetEntryAssembly().GetName().Name, AppDomain.CurrentDomain.BaseDirectory);

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Info
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "REST API for Patients management",
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
        }
    }
}
