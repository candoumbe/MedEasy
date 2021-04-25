using Measures.Context;

using MedEasy.Abstractions.ValueConverters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

using NodaTime;

using System.IO;

namespace Measures.API.Context
{
    /// <summary>
    /// Factory class to create <see cref="MeasuresContext"/> during design time.
    /// </summary>
    public class MeasuresDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MeasuresContext>
    {
        /// <summary>
        /// Creates a new <see cref="MeasuresContext"/> instance.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public MeasuresContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            DbContextOptionsBuilder<MeasuresContext> builder = new();
            string connectionString = configuration.GetValue<string>(nameof(connectionString));
            builder.UseSqlite(connectionString, b => b.UseNodaTime()
                                                       .MigrationsAssembly(typeof(MeasuresContext).Assembly.FullName))
                   .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();

            return new (builder.Options, SystemClock.Instance);
        }
    }
}
