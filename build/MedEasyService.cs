namespace MedEasy.ContinuousIntegration
{
    using System.ComponentModel;

    using Nuke.Common.Tooling;

    /// <summary>
    /// List of services that can be served by MedEasy
    /// </summary>
    [TypeConverter(typeof(TypeConverter<MedEasyServices>))]
    public class MedEasyServices : Enumeration
    {
        /// <summary>
        /// Identity service
        /// </summary>
        public static readonly MedEasyServices Identity = new() { Value = nameof(Identity) };
        /// <summary>
        /// Measures service
        /// </summary>
        public static readonly MedEasyServices Measures = new() { Value = nameof(Measures) };
        /// <summary>
        /// Documents service
        /// </summary>
        public static readonly MedEasyServices Documents = new() { Value = nameof(Documents) };

        /// <summary>
        /// Agenda service
        /// </summary>
        public static readonly MedEasyServices Agenda = new() { Value = nameof(Agenda) };

        /// <summary>
        /// Patients service
        /// </summary>
        public static readonly MedEasyServices Patients = new() { Value = nameof(Patients) };

        /// <summary>
        /// All backend services
        /// </summary>
        public static readonly MedEasyServices Backends = new() { Value = nameof(Backends) };

        /// <summary>
        /// All database services
        /// </summary>
        public static readonly MedEasyServices Datastores = new() { Value = nameof(Datastores) };

        /// <summary>
        /// Entreprise Service Bus
        /// </summary>
        public static readonly MedEasyServices Esb = new() { Value = nameof(Esb) };
        public static readonly MedEasyServices Proxy = new() { Value = nameof(Proxy) };



        /// <summary>
        /// Frontend
        /// </summary>
        public static readonly MedEasyServices Web = new() { Value = nameof(Web) };


        ///<inheritdoc/>
        public static implicit operator string(MedEasyServices service) => service.Value;
    }
}