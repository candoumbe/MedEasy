using Measures.Ids;

using System;
using System.ComponentModel.DataAnnotations;

namespace Measures.DTO
{
    /// <summary>
    /// Data to provide when querying many physiological measure information
    /// </summary>
    public class GetMostRecentPhysiologicalMeasuresInfo
    {
        /// <summary>
        /// Id of the patient to get one measure from
        /// </summary>
        public PatientId PatientId { get; set; }

        /// <summary>
        /// Number of measures to return
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? Count { get; set; }
    }
}
