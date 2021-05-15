namespace MedEasy.ContinuousIntegration
{
    using System.ComponentModel;

    using Nuke.Common.Tooling;

    /// <summary>
    /// List of services that can be served by MedEasy
    /// </summary>
    [TypeConverter(typeof(TypeConverter<MedEasyService>))]
    public class MedEasyService : Enumeration
    {
        /// <summary>
        /// Identity service
        /// </summary>
        public static MedEasyService Identity => new() { Value = nameof(Identity) };
        /// <summary>
        /// Measures service
        /// </summary>
        public static MedEasyService Measures => new() { Value = nameof(Measures) };
        /// <summary>
        /// Documents service
        /// </summary>
        public static MedEasyService Documents => new() { Value = nameof(Documents) };

        /// <summary>
        /// Agenda service
        /// </summary>
        public static MedEasyService Agenda => new() { Value = nameof(Agenda) };

        /// <summary>
        /// Patients service
        /// </summary>
        public static MedEasyService Patients => new() { Value = nameof(Patients) };

        public static implicit operator string(MedEasyService service) => service.Value;
    }
}