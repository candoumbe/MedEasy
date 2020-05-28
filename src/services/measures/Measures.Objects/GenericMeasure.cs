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
        public Guid FormId { get; }

        /// <summary>
        /// Measure data
        /// </summary>
        public JsonDocument Data { get; }

        /// <summary>
        /// Buidls a new <see cref="GenericMeasure"/> instance
        /// </summary>
        /// <param name="id">id of the measure</param>
        /// <param name="patientId">id of the <see cref="Patient"/> to attach the current measure to</param>
        /// <param name="dateOfMeasure">when the measure was made</param>
        /// <param name="formId">id of the <see cref="MeasureForm"/> <see cref="Data"/> are modeled against</param>
        /// <param name="data">data of the measure.</param>
        public GenericMeasure(Guid id, Guid patientId, DateTime dateOfMeasure, Guid formId, JsonDocument data) : base(id, patientId, dateOfMeasure)
        {
            FormId = formId;
            Data = data;
        }
    }
}
