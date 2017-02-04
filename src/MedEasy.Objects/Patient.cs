using System;
using System.Collections.Generic;

namespace MedEasy.Objects
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class Patient : AuditableEntity<int, Patient>
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

        /// <summary>
        /// Patient's fullname
        /// </summary>
        public string Fullname => $"{Firstname}{(!string.IsNullOrWhiteSpace(Firstname) && !string.IsNullOrWhiteSpace(Lastname)? " ": string.Empty)}{Lastname}";

        /// <summary>
        /// BirthDate
        /// </summary>
        public DateTimeOffset? BirthDate { get; set; } = null;

        public string BirthPlace { get; set; } = string.Empty;

        public Doctor MainDoctor { get; set; } = null;

        /// <summary>
        /// Additional notes on the patient
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Id of the current patient's main doctor
        /// </summary>
        public int? MainDoctorId { get; set; }

        /// <summary>
        /// <see cref="BloodPressure"/>s measure taken for this patient
        /// </summary>
        public IEnumerable<BloodPressure> BloodPressures { get; set; }

        /// <summary>
        /// <see cref="DocumentMetadata"/>
        /// </summary>
        public IEnumerable<DocumentMetadata> Documents { get; set; }

        /// <summary>
        /// <see cref="Patient"/>'s <see cref="Appointment"/>s
        /// </summary>
        public IEnumerable<Appointment> Appointments { get; set; }

        /// <summary>
        /// Builds a new <see cref="Patient"/> instance.
        /// </summary>
        public Patient()
        {
            Firstname = string.Empty;
            Lastname = string.Empty;
        }

    }
}
