using Microsoft.Data.Sqlite;
using System;
using System.Data.Common;

namespace MedEasy.IntegrationTests.Core
{
    /// <summary>
    /// A fixture that use a in-memory Sqlite database
    /// </summary>
    public class SqliteDatabaseFixture : IDisposable
    {
        /// <summary>
        /// The current connection
        /// </summary>
        public SqliteConnection Connection { get; private set; }

        /// <summary>
        /// Builds a new <see cref="SqliteDatabaseFixture"/>
        /// </summary>
        public SqliteDatabaseFixture()
        {
            Connection = new SqliteConnection("Datasource=:memory:");
            Connection.Open();
        }

        public void Dispose()
        {
            Connection?.Close();
            Connection = null;
        }
    }
}
