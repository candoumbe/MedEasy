namespace Patients.Objects
{
    using MedEasy.Objects;

    using Patients.Ids;

    /// <summary>
    /// Contains informations about <see cref="Doctor"/>
    /// </summary>
    public class Doctor : AuditableEntity<DoctorId, Doctor>
    {
        /// <summary>
        /// Firstname
        /// </summary>
        public string Firstname { get; }

        /// <summary>
        /// Lastname
        /// </summary>
        public string Lastname { get; }

        /// <summary>
        /// Builds a new <see cref="Doctor"/> instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        public Doctor(DoctorId id, string firstname, string lastname)
            : base(id)
        {
            Firstname = firstname;
            Lastname = lastname;
        }
    }
}
