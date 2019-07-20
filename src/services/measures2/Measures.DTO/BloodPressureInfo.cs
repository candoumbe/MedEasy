using System;
using System.Collections.Generic;

namespace Measures.DTO
{
    public class BloodPressureInfo : PhysiologicalMeasurementInfo, IEquatable<BloodPressureInfo>
    {
        /// <summary>
        /// Value of the measure
        /// </summary>
        public float SystolicPressure { get; set; }

        /// <summary>
        /// Bloo
        /// </summary>
        public float DiastolicPressure { get; set; }

        public override bool Equals(object obj) => Equals(obj as BloodPressureInfo);

        public bool Equals(BloodPressureInfo other) => other != null
            && DateOfMeasure == other.DateOfMeasure
            && SystolicPressure == other.SystolicPressure
            && DiastolicPressure == other.DiastolicPressure;

        public override int GetHashCode() => (DateOfMeasure, SystolicPressure, DiastolicPressure).GetHashCode();

        public static bool operator ==(BloodPressureInfo info1, BloodPressureInfo info2)
        {
            return EqualityComparer<BloodPressureInfo>.Default.Equals(info1, info2);
        }

        public static bool operator !=(BloodPressureInfo info1, BloodPressureInfo info2)
        {
            return !(info1 == info2);
        }
    }
}
