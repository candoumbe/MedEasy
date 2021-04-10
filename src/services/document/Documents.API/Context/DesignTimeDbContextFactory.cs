using Documents.DataStore;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using NodaTime;

using System.IO;

namespace Documents.API.Context
{
    /// <summary>
    /// Factory class to create <see cref="DocumentsStore"/> during design time.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DocumentsStore>
    {
        /// <summary>
        /// Creates a new <see cref="DocumentsStore"/> instance.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public DocumentsStore CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .AddUserSecrets(typeof(Startup).Assembly)
                .Build();
            DbContextOptionsBuilder<DocumentsStore> builder = new DbContextOptionsBuilder<DocumentsStore>();
            string connectionString = configuration.GetConnectionString("Documents");
            builder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(DocumentsStore).Assembly.FullName));
            
            return new DocumentsStore(builder.Options, SystemClock.Instance);
        }
    }
}
