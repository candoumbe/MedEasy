using System.Collections.Generic;

namespace MedEasy.Objects
{

    public class Specialty : AuditableEntity<int, Specialty>
    {
        public string Code { get; set; }
        /// <summary>
        /// List of doctors that has the current speciality
        /// </summary>
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

        public string Name { get; set; }
    }
}
