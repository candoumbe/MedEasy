using Measures.DataStores;

using MedEasy.Abstractions.ValueConverters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

using NodaTime;

using System;
using System.IO;

namespace Measures.API.Context
{
    /// <summary>
    /// Factory class to create <see cref="MeasuresStore"/> during design time.
    /// </summary>
    public class MeasuresDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MeasuresStore>
    {
        /// <summary>
        /// Creates a new <see cref="MeasuresStore"/> instance.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public MeasuresStore CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .AddJsonFile("appsettings.IntegrationTest.json")
                .AddCommandLine(args)
                .Build();

            string provider = configuration.GetValue("provider", "sqlite").ToLowerInvariant();
            DbContextOptionsBuilder<MeasuresStore> builder = new();
            string connectionString = configuration.GetConnectionString("measures");

            switch (provider)
            {
                case "sqlite":
                    builder.UseSqlite(connectionString, b => b.MigrationsAssembly("Measures.DataStores.Sqlite")
                                                              .UseNodaTime())
                           .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                    break;
                case "postgres":
                    builder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Measures.DataStores.Postgres")
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
