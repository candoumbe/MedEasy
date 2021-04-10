using MedEasy.DTO.Search;
using MedEasy.RestObjects;
using Newtonsoft.Json;

using NodaTime;

using System;

namespace Measures.DTO
{
    /// <summary>
    /// Base class for writing Search classes for 
    /// </summary>
    /// <typeparam name="TMeasureInfo"></typeparam>
    [JsonObject]
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
        public Guid? PatientId { get; set; }
    }
}
