using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using NodaTime;

using Patients.Context;
using System.IO;

namespace Patients.API.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PatientsContext>
    {
        public PatientsContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();
            DbContextOptionsBuilder<PatientsContext> builder = new DbContextOptionsBuilder<PatientsContext>();
            string connectionString = configuration.GetConnectionString("Patients");
            builder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(PatientsContext).Assembly.FullName));
            
            return new PatientsContext(builder.Options, SystemClock.Instance);
        }
    }
}
