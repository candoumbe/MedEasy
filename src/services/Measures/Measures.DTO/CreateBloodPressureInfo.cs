using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Measures.DTO
{
    /// <summary>
    /// data to provide when creating a new blood pressure info
    /// </summary>
    public class CreateBloodPressureInfo : CreatePhysiologicalMeasureInfo
    {

        [DataType(DataType.DateTime)]
        public DateTimeOffset DateOfMeasure{ get; set; }

        /// <summary>
        /// The new systolic blod pressure value
        /// </summary>
        public float SystolicPressure { get; set; }


        /// <summary>
        /// The new diastolic blod pressure value
        /// </summary>
        public float DiastolicPressure { get; set; }

        
    }
}
