using NodaTime;

using Patients.Ids;

namespace Patients.Events
{
    /// <summary>
    /// Event raised when a new patient case is created in the system
    /// </summary>
    public record PatientCaseCreated(PatientId Id, string Name, LocalDate? BirthDate);
}
