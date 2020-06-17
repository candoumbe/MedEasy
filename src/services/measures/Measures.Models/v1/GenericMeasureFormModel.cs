
using Forms;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Measures.Models.v1
{
    public class GenericMeasureFormModel
    {
        
        /// <summary>
        /// Id of the measure
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the form.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Fields of the form
        /// </summary>
        public IEnumerable<FormField> Fields { get; set; }
    }
}
