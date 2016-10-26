using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Prescription resource
    /// </summary>
    public class PrescriptionInfo : PrescriptionHeaderInfo
    {
        /// <summary>
        /// Items of the prescription
        /// </summary>
        public IEnumerable<PrescriptionItemInfo> Items { get; set; }

    }
}
