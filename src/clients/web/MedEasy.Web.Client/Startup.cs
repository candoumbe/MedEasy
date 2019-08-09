using Blazor.Extensions.Storage;
using MedEasy.Web.Client.Services;
using MedEasy.Web.Client.Services.Authentication;
using MedEasy.Web.Client.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Refit;
namespace MedEasy.Web.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStorage();
            services.AddSingleton<TokenService>();
            services.AddScoped<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            services.AddScoped<AuthenticationStateProvider, MedEasyAuthenticationStateProvider>();
            services.AddScoped<IIdentityApi>((serviceProvider) => RestService.For<IIdentityApi>("https://localhost:51800/"));
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
