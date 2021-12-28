namespace Measures.DTO
{
    using Measures.Ids;

    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Data to provide when querying many physiological measure information
    /// </summary>
    public class GetMostRecentPhysiologicalMeasuresInfo
    {
        /// <summary>
        /// Id of the patient to get one measure from
        /// </summary>
        public SubjectId PatientId { get; set; }

        /// <summary>
        /// Number of measures to return
        /// </summary>
        [Range(1, int.MaxValue)]
        public int? Count { get; set; }
    }
}
