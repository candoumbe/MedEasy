namespace MedEasy.ContinuousIntegration
{
    using System.ComponentModel;

    using Nuke.Common.Tooling;

    [TypeConverter(typeof(TypeConverter<Configuration>))]
    public class Configuration : Enumeration
    {
        internal static Configuration Debug = new() { Value = nameof(Debug) };
        internal static Configuration Release = new() { Value = nameof(Release) };

        public static implicit operator string(Configuration configuration)
        {
            return configuration.Value;
        }
    }
}