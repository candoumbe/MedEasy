namespace Identity.API.Context
{
    using Identity.DataStores;

    using MedEasy.Abstractions.ValueConverters;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.Extensions.Configuration;

    using NodaTime;

    using System;
    using System.IO;

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
                .AddJsonFile("appsettings.IntegrationTest.json")
                .AddCommandLine(args)
                .Build();

            string provider = configuration.GetValue("provider", "sqlite").ToLowerInvariant();
            DbContextOptionsBuilder<IdentityContext> builder = new();
            string connectionString = configuration.GetConnectionString("Identity");

            switch (provider)
            {
                case "sqlite":
                    builder.UseSqlite(connectionString, b => b.MigrationsAssembly("Identity.DataStores.Sqlite")
                                                              .UseNodaTime())
                           .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                    break;
                case "postgres":
                    builder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Identity.DataStores.Postgres")
                                                              .UseNodaTime())
                           .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                    break;
                default:
                    throw new NotSupportedException($"'{provider}' database engine is not currently supported");
            }

            return new (builder.Options, SystemClock.Instance);
        }
    }
}
