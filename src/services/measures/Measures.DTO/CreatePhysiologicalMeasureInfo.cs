using Measures.Ids;

namespace Measures.DTO
{
    /// <summary>
    /// Base class for data to provide when creating any physiological measure informations
    /// </summary>
    public abstract class CreatePhysiologicalMeasureInfo
    {
        /// <summary>
        /// Patient which the measure is created for
        /// </summary
        public PatientId PatientId { get; set; }
    }
}
