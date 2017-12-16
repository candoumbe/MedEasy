using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Patients.API.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PatientsContext>
    {
        public PatientsContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            DbContextOptionsBuilder<PatientsContext> builder = new DbContextOptionsBuilder<PatientsContext>();
            string connectionString = configuration.GetConnectionString("Default");
            builder.UseSqlServer(connectionString);
            return new PatientsContext(builder.Options);
        }
    }
}
