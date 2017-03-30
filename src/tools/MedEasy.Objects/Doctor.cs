using System.Collections.Generic;

namespace MedEasy.Objects
{
    public class Doctor : AuditableEntity<int, Doctor>
    {
        private string _firstname;

        public string Firstname
        {
            get => _firstname;
            set => _firstname = value ?? string.Empty;
        }

        private string _lastname;

        public string Lastname
        {
            get => _lastname;
            set => _lastname = value ?? string.Empty;
        }

        public string Fullname => $"{Firstname}{(!string.IsNullOrWhiteSpace(Firstname) && !string.IsNullOrWhiteSpace(Lastname) ? " " : string.Empty)}{Lastname}";

        public int? SpecialtyId { get; set; }

        public virtual Specialty Specialty { get; set; }

        public virtual IEnumerable<Patient> Patients { get; set; } = new List<Patient>();

        public virtual IEnumerable<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}