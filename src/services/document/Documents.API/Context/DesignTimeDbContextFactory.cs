using Documents.DataStore;

using MedEasy.Abstractions.ValueConverters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

using NodaTime;

using System.IO;

namespace Documents.API.Context
{
    /// <summary>
    /// Factory class to create <see cref="DocumentsStore"/> during design time.
    /// </summary>
    public class DocumentsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DocumentsStore>
    {
        /// <inheritdoc/>
        public DocumentsStore CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .AddUserSecrets(typeof(Startup).Assembly)
                .AddCommandLine(args)
                .Build();
            DbContextOptionsBuilder<DocumentsStore> builder = new();
            string connectionString = configuration.GetConnectionString("Documents");
            builder.UseSqlite(connectionString,
                              b => b.UseNodaTime()
                                    .MigrationsAssembly(typeof(DocumentsStore).Assembly.FullName))
                   .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();

            return new (builder.Options, SystemClock.Instance);
        }
    }
}
