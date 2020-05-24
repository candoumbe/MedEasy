using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Measures.Objects
{
    /// <summary>
    /// A generic measure value
    /// </summary>
    public class GenericMeasure : PhysiologicalMeasurement
    {
        /// <summary>
        /// Schema associated with the current measure
        /// </summary>
        public MeasureForm Form { get; }

        /// <summary>
        /// Measure data
        /// </summary>
        public JsonDocument Data { get; }

        public GenericMeasure(Guid id, Guid patientId, DateTime dateOfMeasure, JsonDocument data) : base(id, patientId, dateOfMeasure)
        {

        }
    }
}
