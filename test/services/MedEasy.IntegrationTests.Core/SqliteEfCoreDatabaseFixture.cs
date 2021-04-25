using MedEasy.Abstractions.ValueConverters;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MedEasy.IntegrationTests.Core
{
    public class SqliteEfCoreDatabaseFixture<TContext> where TContext : DbContext
    {
        public DbContextOptionsBuilder<TContext> OptionsBuilder { get; }

        private readonly SqliteConnection _connection;

        public SqliteEfCoreDatabaseFixture()
        {
            _connection = new ("Datasource=:memory:");
            _connection?.Open();
            OptionsBuilder = new DbContextOptionsBuilder<TContext>()
                    .UseSqlite(_connection, x => x.UseNodaTime()
                                                  .MigrationsAssembly(typeof(TContext).Assembly.FullName)
                    )
                    .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
        }

    }
}
