using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{

    public class BloodPressureMeasureInfo
    {
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
