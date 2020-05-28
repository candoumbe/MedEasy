using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Measures.DTO
{
    /// <summary>
    /// Data class that holds  informations about a measure in a JSon fashion
    /// </summary>
    public class GenericMeasureInfo : PhysiologicalMeasurementInfo
    {
        public Guid FormId { get; set; }

        public JsonDocument Data { get; set; }
    }
}
