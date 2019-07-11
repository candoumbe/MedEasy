using MedEasy.Objects;
using System;

namespace Patients.Objects
{
    /// <summary>
    /// Contains informations about <see cref="Doctor"/>
    /// </summary>
    public class Doctor : AuditableEntity<Guid, Doctor>
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
        public Doctor(Guid id, string firstname, string lastname)
            : base (id)
        {
            Firstname = firstname;
            Lastname = lastname;
        }
    }
}
