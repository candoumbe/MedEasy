
using System;
using System.Collections.Generic;

namespace Measures.Models.v1
{
    public class GenericMeasureModel
    {
        public Guid PatientId { get; set; }

        /// <summary>
        /// Id of the measure
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// When was the measure made
        /// </summary>
        public DateTime DateOfMeasure { get; set; }

        public IDictionary<string, object> Values { get; set; }
    }
}
