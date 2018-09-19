using Identity.API.Routing;
using Identity.DataStores.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Data.SqlClient;

namespace Identity.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCustomMvc(_configuration);
            services.AddAuthorization();
            services.AddDependencyInjection();
            services.ConfigureSwagger(_hostingEnvironment, _configuration);
            services.ConfigureAuthentication(_configuration);
            services.AddDataStores();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {

            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                ILogger<Startup> logger = services.GetRequiredService<ILogger<Startup>>();
                IdentityContext context = services.GetRequiredService<IdentityContext>();
                logger?.LogInformation($"Starting Identity.API");

                try
                {
                    if (!context.Database.IsInMemory())
                    {
                        logger?.LogInformation($"Upgrading Identity store");
                        // Forces database migrations on startup
                        RetryPolicy policy = Policy
                            .Handle<SqlException>(sql => sql.Message.Like("*Login failed*", ignoreCase: true))
                            .WaitAndRetry(
                                retryCount: 5,
                                sleepDurationProvider: (retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
                                onRetry: (exception, timeSpan, attempt, pollyContext) =>
                                    logger?.LogError(exception, $"Error while upgrading database (Attempt {attempt}/{pollyContext.Count})")
                                );
                        logger?.LogInformation("Starting identity database migration");

                        // Forces datastore migration on startup
                        policy.Execute(async () => await context.Database.MigrateAsync().ConfigureAwait(false))
                            .ConfigureAwait(false);

                        logger?.LogInformation("Identity database migrated successfully");

                    }
                    

                    logger?.LogInformation($"Identity.API started");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "An error occurred on startup.");
                }
            }

            app.UseAuthentication();
            app.UseHttpMethodOverride();
            app.UseHsts();
            app.UseHttpsRedirection();
            applicationLifetime.ApplicationStopping.Register(() =>
            {

            });

            if (env.IsProduction() || env.IsStaging())
            {
                app.UseResponseCaching();
                app.UseResponseCompression();
            }
            else
            {
                loggerFactory.AddDebug();
                loggerFactory.AddConsole();
                
                if (env.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(opt =>
                    {
                        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API V1");
                    })
                    ;
                }
            }

            app.UseCors("AllowAnyOrigin");
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute(RouteNames.Default, "identity/{controller=root}/{action=index}");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneByIdApi, "identity/{controller}/{id}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllApi, "identity/{controller}/");
                routeBuilder.MapRoute(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, "identity/{controller}/{id}/{action}/{subResourceId}");
                routeBuilder.MapRoute(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, "identity/{controller}/{id}/{action}/");
                routeBuilder.MapRoute(RouteNames.DefaultSearchResourcesApi, "identity/{controller}/search/");
            });
        }
    }
}
