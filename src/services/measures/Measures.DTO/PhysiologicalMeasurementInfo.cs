using MedEasy.RestObjects;
using System;
using System.ComponentModel.DataAnnotations;

namespace Measures.DTO
{
    /// <summary>
    /// Base class for physiological measure resources
    /// </summary>
    public abstract class PhysiologicalMeasurementInfo : Resource<Guid>
    {
        /// <summary>
        /// Id of the <see cref="PatientInfo"/> resource the measure was taken on
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// When the measure was made
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime DateOfMeasure { get; set; }
    }
}
