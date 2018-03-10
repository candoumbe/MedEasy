using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.IntegrationTests.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class DatabaseFixture : IDisposable
    {
        /// <summary>
        /// The current connection
        /// </summary>
        public DbConnection Connection { get; private set; }
         
        /// <summary>
        /// Builds a new <see cref="DatabaseFixture"/>
        /// </summary>
        public DatabaseFixture()
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
