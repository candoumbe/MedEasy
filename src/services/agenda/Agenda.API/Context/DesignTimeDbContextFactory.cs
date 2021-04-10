using Agenda.DataStores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using NodaTime;

using System.IO;

namespace Agenda.API.Context
{
    /// <summary>
    /// Factory class to create <see cref="AgendaContext"/> during design time.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AgendaContext>
    {
        /// <summary>
        /// Creates a new <see cref="AgendaContext"/> instance.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public AgendaContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();
            DbContextOptionsBuilder<AgendaContext> builder = new();
            string connectionString = configuration.GetConnectionString("Agenda");
            builder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(AgendaContext).Assembly.FullName));

            return new AgendaContext(builder.Options, SystemClock.Instance);
        }
    }
}
