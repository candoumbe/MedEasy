using System;

namespace Measures.Objects
{
    /// <summary>
    /// Defines a blood pressure measure.
    /// </summary>
    /// <remarks>
    /// All mesures are expressed in milimeters of mercury (mm Hg) 
    /// </remarks>
    public class BloodPressure : PhysiologicalMeasurement
    {
        /// <summary>
        /// Gets/sets the blood pressure
        /// </summary>
        public float DiastolicPressure { get; private set; }

        /// <summary>
        /// Get/set the blood pressure
        /// </summary>
        public float SystolicPressure { get; private set; }


        public BloodPressure(Guid id, Guid patientId, DateTimeOffset dateOfMeasure, float diastolicPressure, float systolicPressure)
            : base (id, patientId, dateOfMeasure)
        {
            DiastolicPressure = diastolicPressure;
            SystolicPressure = systolicPressure;
        }

        public void ChangeSystolicTo(float newValue) => SystolicPressure = newValue;
        public void ChangeDiastolicTo(float newValue) => throw new NotImplementedException();
        public void ChangeDateOfMeasure(DateTime newValue) => throw new NotImplementedException();
    }
}