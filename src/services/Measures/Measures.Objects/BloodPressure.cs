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
        public float DiastolicPressure { get; set; }

        /// <summary>
        /// Get/set the blood pressure
        /// </summary>
        public float SystolicPressure { get; set; }
    }
}