namespace Agenda.API.Context
{
    using Agenda.DataStores;

    using MedEasy.Abstractions.ValueConverters;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.Extensions.Configuration;

    using NodaTime;

    using System.IO;

    /// <summary>
    /// <see cref="IDesignTimeDbContextFactory{TContext}"/> implementation for <see cref="AgendaContext"/>.
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
                .AddCommandLine(args)
                .Build();

            string provider = configuration.GetValue("provider", "sqlite").ToLowerInvariant();
            DbContextOptionsBuilder<AgendaContext> builder = new();
            string connectionString = configuration.GetConnectionString("agenda");

            switch (provider)
            {
                case "sqlite":
                    builder.UseSqlite(connectionString, b => b.MigrationsAssembly("Agenda.DataStores.Sqlite")
                                                              .UseNodaTime())
                           .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                    break;
                case "postgres":
                    builder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Agenda.DataStores.Postgres")
                                                              .UseNodaTime())
                           .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                    break;
                default:
                    throw new NotSupportedException($"'{provider}' database engine is not currently supported");
            }

            return new(builder.Options, SystemClock.Instance);
        }
    }
}
