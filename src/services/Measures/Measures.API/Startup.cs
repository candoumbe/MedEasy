using FluentValidation.AspNetCore;
using Measures.API.Context;
using Measures.API.Routing;
using Measures.API.StartupRegistration;
using Measures.Mapping;
using Measures.Validators;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Newtonsoft.Json;
using static Newtonsoft.Json.DateFormatHandling;
using static Newtonsoft.Json.DateTimeZoneHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;

namespace Measures.API
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The root configuration 
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Provides information about 
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Builds a new <see cref="Startup"/> instance.
        /// </summary>
        /// <param name="env">Accessor to the current ost's environment </param>
        public Startup(IHostingEnvironment env)
        {

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(FormatFilter));
                //options.Filters.Add(typeof(ValidateModelAttribute));
                ////options.Filters.Add(typeof(EnvelopeFilterAttribute));
                //options.Filters.Add(typeof(HandleErrorAttribute));
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.DateFormatHandling = IsoDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = Utc;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            })
            .AddFluentValidation(options =>
            {
                options.LocalizationEnabled = true;
                options.RegisterValidatorsFromAssemblyContaining<CreateBloodPressureInfoValidator>();
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder => 
                    builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                );
            });
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAnyOrigin"));
            });

            services.AddSingleton(x => AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder);
            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<MeasuresContext>>(item =>
            {
                DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
                builder.UseSqlServer(Configuration.GetConnectionString("Measures"));

                return new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, (options) => new MeasuresContext(options));

            });
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(builder =>
            {
                IUrlHelperFactory urlHelperFactory = builder.GetRequiredService<IUrlHelperFactory>();
                IActionContextAccessor actionContextAccessor = builder.GetRequiredService<IActionContextAccessor>();

                return urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            });


            services.AddValidators();

            services.AddOptions();
            services.Configure<MeasuresApiOptions>((options) =>
            {
                options.DefaultPageSize = Configuration.GetValue("APIOptions:DefaultPageSize", 30);
                options.MaxPageSize = Configuration.GetValue("APIOptions:MaxPageSize", 100);
            });

            if (HostingEnvironment.IsDevelopment())
            {
                ApplicationEnvironment app = PlatformServices.Default.Application;
                services.AddSwaggerGen(config =>
                {
                    config.SwaggerDoc("v1", new Info
                    {
                        Title = app.ApplicationName,
                        Description = "REST API for Measures API",
                        Version = "v1",
                        Contact = new Contact
                        {
                            Email = Configuration.GetValue("Swagger:Contact:Email", string.Empty),
                            Name = Configuration.GetValue("Swagger:Contact:Name", string.Empty),
                            Url = Configuration.GetValue("Swagger:Contact:Url", string.Empty)
                        }
                    });
                    
                    config.IgnoreObsoleteActions();
                    config.IgnoreObsoleteProperties();
                    config.IncludeXmlComments(Path.Combine(app.ApplicationBasePath, $"{app.ApplicationName}.xml"));
                    config.DescribeStringEnumsInCamelCase();
                    config.DescribeAllEnumsAsStrings();
                });
            }
            services.AddRouting(options =>
            {
                options.AppendTrailingSlash = true;
                options.LowercaseUrls = true;
            });

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (!env.IsDevelopment())
            {
                app.UseResponseCaching();
                app.UseResponseCompression();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();

                app.UseSwagger();
                app.UseSwaggerUI(opt =>
                {
                    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "MedEasy REST API V1");
                });

            }

            app.UseCors("AllowAnyOrigin");
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute("default", "measures/{controller=root}/{action=index}");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneByIdApi, "measures/{controller}/{id}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllApi, "measures/{controller}/");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "measures/{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "measures/{controller}/{id}/{action}/");
                routeBuilder.MapRoute(RouteNames.DefaultSearchResourcesApi, "measures/{controller}/search/");
            });
        }
    }
}
