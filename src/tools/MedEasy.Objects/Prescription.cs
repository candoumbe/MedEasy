using System;
using System.Collections.Generic;

namespace MedEasy.Objects
{
    /// <summary>
    /// A medical prescription delivered to a <see cref="Patient"/> by a <see cref="Prescriptor"/>
    /// at a given date
    /// </summary>
    public class Prescription : AuditableEntity<int, Prescription>
    {
        /// <summary>
        /// Date on which the prescription was made
        /// </summary>
        public DateTimeOffset DeliveryDate { get; set; } = new DateTimeOffset();

        /// <summary>
        /// <see cref="Doctor"/> who made the prescription
        /// </summary>
        public virtual Doctor Prescriptor { get; set; }

        /// <summary>
        /// Id of the doctor who delivered the 
        /// </summary>
        public int PrescriptorId { get; set; }

        /// <summary>
        /// <see cref="Patient"/> the prescription was made for
        /// </summary>
        public virtual Patient Patient { get; set; }

        /// <summary>
        /// Id of the patient for which the prescription is done
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Content of the prescription
        /// </summary>
        public virtual ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();

    }
}
