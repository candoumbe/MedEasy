using NodaTime;

using Patients.Ids;

namespace Patients.Events
{
    public record PatientCaseCreated(PatientId Id, string Name, LocalDate? BirthDate);
}
