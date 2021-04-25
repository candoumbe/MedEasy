using Agenda.DataStores;

using MedEasy.Abstractions.ValueConverters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

using NodaTime;

using System.IO;

namespace Agenda.API.Context
{
    /// <summary>
    /// Factory class to create <see cref="AgendaContext"/> during design time.
    /// </summary>
    public class AgendaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AgendaContext>
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
                .AddJsonFile("appsettings.IntegrationTest.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
            DbContextOptionsBuilder<AgendaContext> builder = new();
            string connectionString = configuration.GetConnectionString("agenda");
            builder.UseSqlite(connectionString, b => b.MigrationsAssembly(typeof(AgendaContext).Assembly.FullName)
                                                      .UseNodaTime())
                   .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();

            return new AgendaContext(builder.Options, SystemClock.Instance);
        }
    }
}
