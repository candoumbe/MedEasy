using Forms;

using MedEasy.Attributes;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using static Newtonsoft.Json.Formatting;
using static Newtonsoft.Json.JsonConvert;

namespace Measures.DTO
{
    /// <summary>
    /// data to provide when creating a new blood pressure info
    /// </summary>
    public class CreateGenericMeasureInfo : CreatePhysiologicalMeasureInfo
    {
        [DataType(DataType.DateTime)]
        public DateTime DateOfMeasure{ get; set; }

        /// <summary>
        /// Id of the form where data should be validated against
        /// </summary>
        [FormField]
        [RequireNonDefault]
        public Guid FormId { get; set; }

        /// <summary>
        /// Data associated with the measure to create
        /// </summary>
        [FormField]
        public IDictionary<string, object> Values { get; set; }

        public override string ToString() => this.Jsonify();
    }
}
