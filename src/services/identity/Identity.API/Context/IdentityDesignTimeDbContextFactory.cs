using Identity.DataStores;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using NodaTime;

using System.IO;

namespace Identity.API.Context
{
    /// <summary>
    /// Factory class to create <see cref="IdentityContext"/> during design time.
    /// </summary>
    public class IdentityDesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        /// <summary>
        /// Creates a new <see cref="IdentityContext"/> instance.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public IdentityContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();
            DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
            string connectionString = configuration.GetConnectionString("Identity");
            builder.UseSqlite(connectionString,
                              b => b.UseNodaTime()
                                    .MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
            return new IdentityContext(builder.Options, SystemClock.Instance);
        }
    }
}
