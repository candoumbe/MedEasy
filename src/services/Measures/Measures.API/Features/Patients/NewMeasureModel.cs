using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measures.API.Features.Patients
{
    /// <summary>
    /// Base class for model to create new measure
    /// </summary>
    public abstract class NewMeasureModel
    {
        /// <summary>
        /// Indicates when the measure was made 
        /// </summary>
        public DateTimeOffset DateOfMeasure { get; set; }
    }
}
