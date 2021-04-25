using Identity.DataStores;

using MedEasy.Abstractions.ValueConverters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

using NodaTime;

using System;
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
                .AddCommandLine(args)
                .Build();
            DbContextOptionsBuilder<IdentityContext> builder = new();
            string connectionString = configuration.GetConnectionString("Identity");

            string provider = configuration.GetValue("provider", "sqlite")
                                           ?.ToLowerInvariant();
            IdentityContext context;
            switch(provider)
            {
                case "sqlite":
                builder.UseSqlite(connectionString,
                                  b => b.UseNodaTime()
                                        .MigrationsAssembly(typeof(IdentityContext).Assembly.FullName))
                       .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                context = new IdentityContext(builder.Options, SystemClock.Instance);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported provider '{provider}'");

            }

            return context;
        }
    }
}
