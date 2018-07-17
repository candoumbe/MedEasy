using AutoMapper;
using FluentValidation.AspNetCore;
using Identity.DTO;
using Measures.Context;
using Measures.CQRS.Commands.BloodPressures;
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
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Security.Claims;
using System.Text;
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
        public static void ConfigureMvc(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(FormatFilter));
                options.Filters.Add(typeof(ValidateModelActionFilter));
                options.Filters.Add(typeof(AddCountHeadersFilterAttribute));
                ////options.Filters.Add(typeof(EnvelopeFilterAttribute));
                options.Filters.Add(typeof(HandleErrorAttribute));
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                     .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                     .RequireAuthenticatedUser()
                     .Build();
                options.Filters.Add(new AuthorizeFilter(policy));


            })
            .AddFluentValidation(options =>
            {
                options.LocalizationEnabled = true;
                options
                    .RegisterValidatorsFromAssemblyContaining<PaginationConfigurationValidator>()
                    .RegisterValidatorsFromAssemblyContaining<CreateBloodPressureInfoValidator>()
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

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                    builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                );
            });
            services.AddOptions();
            services.Configure<MeasuresApiOptions>((options) =>
            {
                options.DefaultPageSize = configuration.GetValue("APIOptions:DefaultPageSize", 30);
                options.MaxPageSize = configuration.GetValue("APIOptions:MaxPageSize", 100);
            });
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAnyOrigin"));
            });

            services.AddRouting(options =>
            {
                options.AppendTrailingSlash = false;
                options.LowercaseUrls = true;
            });
        }

        /// <summary>
        /// Adds required dependencies to access APÏ datastores
        /// </summary>
        /// <param name="services"></param>
        /// 
        /// 
        public static void ConfigureDataStores(this IServiceCollection services) => 
            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<MeasuresContext>>(serviceProvider =>
                {
                    IHostingEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
                    DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
                    if (hostingEnvironment.IsEnvironment("IntegrationTest"))
                    {
                        string dbName = $"InMemoryDb_{Guid.NewGuid()}";
                        builder.UseInMemoryDatabase(dbName);
                    }
                    else
                    {
                        IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                        builder.UseSqlServer(configuration.GetConnectionString("Measures"), b => b.MigrationsAssembly("Measures.API"));
                    }
                    builder.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
                    builder.ConfigureWarnings(options =>
                    {
                        options.Default(WarningBehavior.Log);
                    });

                    return new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, (options) => new MeasuresContext(options));

                });

        /// <summary>
        /// Configure dependency injections
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureDependencyInjection(this IServiceCollection services)
        {
            services.AddMediatR(typeof(CreateBloodPressureInfoForPatientIdCommand).Assembly);
            services.AddSingleton<IHandleSearchQuery, HandleSearchQuery>();
            services.AddSingleton(provider => AutoMapperConfig.Build().CreateMapper());
            services.AddSingleton(provider => provider.GetRequiredService<IMapper>().ConfigurationProvider.ExpressionBuilder);

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(builder =>
            {
                IUrlHelperFactory urlHelperFactory = builder.GetRequiredService<IUrlHelperFactory>();
                IActionContextAccessor actionContextAccessor = builder.GetRequiredService<IActionContextAccessor>();

                return urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            });

            services.AddScoped(serviceProvider =>
            {
                IHttpContextAccessor httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                ClaimsPrincipal principal = httpContextAccessor.HttpContext.User;

                ConnectedAccountInfo connectedAccount = new ConnectedAccountInfo
                {
                    Id = Guid.TryParse(principal.FindFirstValue(CustomClaimTypes.AccountId), out Guid accountId) 
                        ? accountId 
                        : Guid.Empty,
                    Username = principal.FindFirstValue(ClaimTypes.Name),
                    Email = principal.FindFirstValue(ClaimTypes.Email)
                };

                return principal;
            });


        }

        /// <summary>
        /// Adds Swagger middlewares
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="configuration"></param>
        public static void ConfigureSwagger(this IServiceCollection services, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            ApplicationEnvironment app = PlatformServices.Default.Application;

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Info
                {
                    Title = hostingEnvironment.ApplicationName,
                    Description = "REST API for Measures API",
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
                string documentationPath = Path.Combine(app.ApplicationBasePath, $"{app.ApplicationName}.xml");
                if (File.Exists(documentationPath))
                {
                    config.IncludeXmlComments(documentationPath);
                }
                config.DescribeStringEnumsInCamelCase();
                config.DescribeAllEnumsAsStrings();
                config.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    In = "header",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    Type = "apiKey"
                    
                });
            });
        }

        /// <summary>
        /// Configures the authentication middleware
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration) => 
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
    }
}
