using System.Collections.Generic;

namespace MedEasy.Objects
{
    public class Doctor : AuditableEntity<int, Doctor>
    {
        private string _firstname;

        public string Firstname
        {
            get
            {
                return _firstname;
            }
            set
            {
                _firstname = value ?? string.Empty;
            }
        }

        private string _lastname;

        public string Lastname
        {
            get
            {
                return _lastname;
            }
            set
            {
                _lastname = value ?? string.Empty;
            }
        }

        public string Fullname => $"{Firstname}{(!string.IsNullOrWhiteSpace(Firstname) && !string.IsNullOrWhiteSpace(Lastname) ? " " : string.Empty)}{Lastname}";

        public int? SpecialtyId { get; set; }

        public Specialty Specialty { get; set; }

        public ICollection<Patient> Patients { get; set; }
    }
}