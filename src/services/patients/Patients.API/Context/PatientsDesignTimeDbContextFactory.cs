using MedEasy.Abstractions.ValueConverters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

using NodaTime;

using Patients.Context;

using System.IO;

namespace Patients.API.Context
{
    public class PatientsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PatientsContext>
    {
        /// <inheritdoc/>
        public PatientsContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .AddCommandLine(args)
                .Build();
            DbContextOptionsBuilder<PatientsContext> builder = new();
            string connectionString = configuration.GetConnectionString("Patients");
            builder.UseSqlite(connectionString, b => b.MigrationsAssembly(typeof(PatientsContext).Assembly.FullName)
                                                      .UseNodaTime())
                   .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();

            return new PatientsContext(builder.Options, SystemClock.Instance);
        }
    }
}
