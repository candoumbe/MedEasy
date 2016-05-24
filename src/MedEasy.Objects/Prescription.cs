using System;
using System.Collections.Generic;

namespace MedEasy.Objects
{
    /// <summary>
    /// An instance of this class represents a medical prescription delivered to a <see cref="Patient"/> by a <see cref="Prescriptor"/>
    /// at a given date
    /// </summary>
    public class Prescription : AuditableEntity<int, Prescription>
    {
        /// <summary>
        /// Date on which the prescription was made
        /// </summary>
        public DateTime DeliveryDate { get; set; } = new DateTime();

        /// <summary>
        /// <see cref="Doctor"/> who made the prescription
        /// </summary>
        public Doctor Prescriptor { get; set; }

        /// <summary>
        /// <see cref="Patient"/> the prescription was made for
        /// </summary>
        public Patient Patient { get; set; }

        public ICollection<PrescriptionItem> Items { get; set; }

    }
}
