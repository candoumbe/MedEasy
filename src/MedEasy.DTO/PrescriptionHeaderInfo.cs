using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Prescription resource
    /// </summary>
    public class PrescriptionHeaderInfo : ResourceBase<int>
    {
        
        /// <summary>
        /// Date on which the prescription was made
        /// </summary>
        public DateTimeOffset DeliveryDate { get; set; } = new DateTimeOffset();

        /// <summary>
        /// <see cref="Doctor"/> who made the prescription
        /// </summary>
        public int PrescriptorId { get; set; }

        /// <summary>
        /// Id of the <see cref="PatientInfo"/> the prescription was made for
        /// </summary>
        public int PatientId { get; set; }

    }
}
