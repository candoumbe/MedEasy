using MedEasy.Objects;

namespace Patients.Objects
{
    /// <summary>
    /// Contains informations about <see cref="Doctor"/>
    /// </summary>
    public class Doctor : AuditableEntity<int, Doctor>
    {
        /// <summary>
        /// Firstname
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Lastname
        /// </summary>
        public string Lastname { get; set; }
    }
}
