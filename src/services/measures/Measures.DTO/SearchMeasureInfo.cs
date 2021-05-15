namespace Measures.DTO
{
    using Measures.Ids;

    using MedEasy.DTO.Search;
    using MedEasy.RestObjects;

    using NodaTime;

    /// <summary>
    /// Base class for writing Search classes for 
    /// </summary>
    /// <typeparam name="TMeasureInfo"></typeparam>
    public abstract class SearchMeasureInfo<TMeasureInfo> : AbstractSearchInfo<TMeasureInfo>
    {
        /// <summary>
        /// Defines the min <see cref="PhysiologicalMeasurementInfo.DateOfMeasure"/>
        /// </summary>
        [FormField(Description = "Minimum date of measure")]
        public ZonedDateTime? From { get; set; }
        /// <summary>
        /// Defines the max <see cref="PhysiologicalMeasurementInfo.DateOfMeasure"/>
        /// </summary>
        [FormField(Description = "Maximum date of measure")]
        public ZonedDateTime? To { get; set; }

        [FormField(Description = "Id of the patient")]
        public PatientId PatientId { get; set; }
    }
}
