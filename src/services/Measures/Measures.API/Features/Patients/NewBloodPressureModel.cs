using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measures.API.Features.Patients
{
    /// <summary>
    /// Model to create a new blood pressure
    /// </summary>
    public class NewBloodPressureModel : NewMeasureModel
    {
        /// <summary>
        /// Systolic pressure
        /// </summary>
        [FormField(Min = 0)]
        public float SystolicPressure { get; set; }

        /// <summary>
        /// Diastolic pressure
        /// </summary>
        [FormField(Min = 0)]
        public float DiastolicPressure { get; set; }
    }
}
