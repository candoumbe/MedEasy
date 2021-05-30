namespace Patients.Events
{
    using NodaTime;

    using Patients.Ids;

    /// <summary>
    /// Event raise when a patient case is closed
    /// </summary>
    public record PatientCaseClosed
    {
        public PatientId Id { get; init; }

        public LocalDate Date { get; init; }

        public string Comment { get; init; }
    }
}
