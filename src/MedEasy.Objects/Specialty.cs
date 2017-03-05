using System.Collections.Generic;

namespace MedEasy.Objects
{

    public class Specialty : AuditableEntity<int, Specialty>
    {
        /// <summary>
        /// List of doctors that has the current speciality
        /// </summary>
        public IEnumerable<Doctor> Doctors { get; set; } = new List<Doctor>();

        /// <summary>
        /// Name of the specialty
        /// </summary>
        public string Name { get; set; }
    }
}
