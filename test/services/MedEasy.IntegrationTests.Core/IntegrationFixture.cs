#if !NETCOREAPP2_0
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MedEasy.IntegrationTests.Core
{
    public class IntegrationFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder.UseEnvironment("IntegrationTest")
                    .UseStartup<TEntryPoint>();
    }
}

#endif