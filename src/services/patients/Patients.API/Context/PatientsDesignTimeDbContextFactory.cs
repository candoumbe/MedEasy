﻿namespace Patients.API.Context
{
    using MedEasy.Abstractions.ValueConverters;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.Extensions.Configuration;

    using NodaTime;

    using Patients.Context;

    using System;
    using System.IO;

    /// <summary>
    /// <see cref="IDesignTimeDbContextFactory{TContext}"/> implementation for <see cref="PatientsDataStore"/>.
    /// </summary>
    public class PatientsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PatientsDataStore>
    {
        /// <inheritdoc/>
        public PatientsDataStore CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .AddJsonFile("appsettings.IntegrationTest.json")
                .AddCommandLine(args)
                .Build();

            string provider = configuration.GetValue("provider", "sqlite").ToLowerInvariant();
            DbContextOptionsBuilder<PatientsDataStore> builder = new();
            string connectionString = configuration.GetConnectionString("Patients");

            switch (provider)
            {
                case "sqlite":
                    builder.UseSqlite(connectionString, b => b.MigrationsAssembly("Patients.DataStores.Sqlite")
                                                              .UseNodaTime())
                           .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                    break;
                case "postgres":
                    builder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Patients.DataStores.Postgres")
                                                              .UseNodaTime())
                           .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                    break;
                default:
                    throw new NotSupportedException($"'{provider}' database engine is not currently supported");
            }

            return new PatientsDataStore(builder.Options, SystemClock.Instance);
        }
    }
}
