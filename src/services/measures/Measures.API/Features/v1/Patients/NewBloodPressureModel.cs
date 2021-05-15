namespace Measures.API.Features.Patients
{
    using MedEasy.Attributes;
    using MedEasy.RestObjects;

    /// <summary>
    /// Model to create a new blood pressure
    /// </summary>
    public class NewBloodPressureModel : NewMeasureModel
    {
        /// <summary>
        /// Systolic pressure
        /// </summary>
        [Minimum(0)]
        [FormField(Min = 0)]
        public float SystolicPressure { get; set; }

        /// <summary>
        /// Diastolic pressure
        /// </summary>
        [Minimum(0)]
        [FormField(Min = 0)]
        public float DiastolicPressure { get; set; }
    }
}
