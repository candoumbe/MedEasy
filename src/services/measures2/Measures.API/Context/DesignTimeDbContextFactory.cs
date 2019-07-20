using Measures.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Measures.API.Context
{
    /// <summary>
    /// Factory class to create <see cref="MeasuresContext"/> during design time.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MeasuresContext>
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
                .Build();
            DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
            string connectionString = configuration.GetConnectionString("Measures");
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("Measures.Context"));
            return new MeasuresContext(builder.Options);
        }
    }
}
