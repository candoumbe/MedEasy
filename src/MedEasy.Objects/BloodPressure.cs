namespace MedEasy.Objects
{
    /// <summary>
    /// An instance of this class represents a blood pressure measure.
    /// All mesures are expressed in milimeters of mercury (mm Hg) 
    /// </summary>
    public class BloodPressure : PhysiologicalMeasurement
    {
        /// <summary>
        /// Gets/sets the blood pressure
        /// </summary>
        public float DiastolicPressure { get; set; }

        public float SystolicPressure { get; set; }
    }
}