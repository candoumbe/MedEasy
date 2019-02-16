using FluentValidation.AspNetCore;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Serialization;
using Patients.API.Context;
using Patients.API.Controllers;
using Patients.API.Routing;
using Patients.API.StartupRegistration;
using Patients.Mapping;
using Patients.Validators.Features.Patients.DTO;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using static Newtonsoft.Json.DateFormatHandling;
using static Newtonsoft.Json.DateTimeZoneHandling;

namespace Patients.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add<FormatFilterAttribute>();
                //options.Filters.Add<ValidateModelAttribute>();
                ////options.Filters.Add<EnvelopeFilterAttribute>();
                //options.Filters.Add<HandleErrorAttribute>();
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.DateFormatHandling = IsoDateFormat;
                options.SerializerSettings.DateTimeZoneHandling = Utc;
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            })
            .AddFluentValidation(options =>
            {
                options.LocalizationEnabled = true;
                options.RegisterValidatorsFromAssemblyContaining<CreatePatientInfoValidator>();
            });

            services.AddSingleton(x => AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder);
            services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<PatientsContext>>(item =>
            {
                DbContextOptionsBuilder<PatientsContext> builder = new DbContextOptionsBuilder<PatientsContext>();
                builder.UseSqlServer(Configuration.GetConnectionString("Patients"));

                return new EFUnitOfWorkFactory<PatientsContext>(builder.Options, (options) => new PatientsContext(options));
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
            services.Configure<PatientsApiOptions>((options) =>
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
                        Description = "REST API for Patients API",
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

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                    builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                );
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseResponseCaching();
                app.UseResponseCompression();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(opt =>
                {
                    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "MedEasy REST API V1");
                });
            }
            app.UseCors("AllowAnyOrigin");
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute(RouteNames.Default, $"{{controller=root}}/{{action={nameof(RootController.Index)}}}");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneByIdApi, "{controller}/{id}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllApi, "{controller}/");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "{controller}/{id}/{action}/");
                routeBuilder.MapRoute(RouteNames.DefaultSearchResourcesApi, "{controller}/search/");
            });
        }
    }
}
