﻿using MedEasy.DTO.Search;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
        public DateTime? From { get; set; }
        /// <summary>
        /// Defines the max <see cref="PhysiologicalMeasurementInfo.DateOfMeasure"/>
        /// </summary>
        public DateTime? To { get; set; }
    }
}
