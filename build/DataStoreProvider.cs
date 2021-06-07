namespace MedEasy.ContinuousIntegration
{
    using System.ComponentModel;

    using Nuke.Common.Tooling;

    [TypeConverter(typeof(TypeConverter<DataStoreProvider>))]
    public class DataStoreProvider : Enumeration
    {
        /// <summary>
        /// Sqlite database engine
        /// </summary>
        public static readonly DataStoreProvider Sqlite = new() { Value = nameof(Sqlite) };

        /// <summary>
        /// Postgres database engine
        /// </summary>
        public static readonly DataStoreProvider Postgres = new() { Value = nameof(Postgres) };

        ///<inheritdoc/>
        public static implicit operator string(DataStoreProvider provider) => provider.Value;
    }
}