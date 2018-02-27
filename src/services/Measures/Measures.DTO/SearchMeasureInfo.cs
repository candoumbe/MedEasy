using MedEasy.DTO.Search;
using MedEasy.RestObjects;
using Newtonsoft.Json;
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
        public DateTime? From { get; set; }
        /// <summary>
        /// Defines the max <see cref="PhysiologicalMeasurementInfo.DateOfMeasure"/>
        /// </summary>
        [FormField(Description = "Maximum date of measure")]
        public DateTime? To { get; set; }
    }
}
