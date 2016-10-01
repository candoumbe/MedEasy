using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Data to provide when querying any physiological measure information
    /// </summary>
    public class GetOnePhysiologicalMeasureInfo
    {
        /// <summary>
        /// Id of the patient to get one measure from
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Id of the measure to get
        /// </summary>
        public int MeasureId { get; set; }
    }
}
