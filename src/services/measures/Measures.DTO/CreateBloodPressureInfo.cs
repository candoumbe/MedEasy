using MedEasy.RestObjects;

using NodaTime;

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
        public Instant DateOfMeasure { get; set; }

        /// <summary>
        /// The new systolic blod pressure value
        /// </summary>
        [FormField(Min = 0)]
        public float SystolicPressure { get; set; }

        /// <summary>
        /// The new diastolic blod pressure value
        /// </summary>
        [FormField(Min = 0)]
        public float DiastolicPressure { get; set; }

        public string ToString(Func<CreateBloodPressureInfo, string> format) => format?.Invoke(this);
    }
}
