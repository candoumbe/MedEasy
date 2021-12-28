namespace Measures.Objects
{
    using Measures.Ids;

    using NodaTime;

    using System;

    /// <summary>
    /// Defines a blood pressure measure.
    /// </summary>
    /// <remarks>
    /// All mesures are expressed in milimeters of mercury (mm Hg) 
    /// </remarks>
    public class BloodPressure : PhysiologicalMeasurement<BloodPressureId>
    {
        /// <summary>
        /// Gets/sets the blood pressure
        /// </summary>
        public float DiastolicPressure { get; private set; }

        /// <summary>
        /// Get/set the blood pressure
        /// </summary>
        public float SystolicPressure { get; private set; }

        /// <summary>
        /// Builds a new <see cref="BloodPressure"/> instance.
        /// </summary>
        /// <param name="subjectId">id of the <see cref="Subject"/> who the current measure will be attached to.</param>
        /// <param name="dateOfMeasure">dDate of the measure</param>
        /// <param name="diastolicPressure">The diastolic measure</param>
        /// <param name="systolicPressure">The systolic measure</param>
        public BloodPressure(SubjectId subjectId, BloodPressureId id, Instant dateOfMeasure, float diastolicPressure, float systolicPressure)
            : base(subjectId, id, dateOfMeasure)
        {
            DiastolicPressure = diastolicPressure;
            SystolicPressure = systolicPressure;
        }

        /// <summary>
        /// Updates the <see cref="SystolicPressure"/> measurement
        /// </summary>
        /// <param name="newValue"></param>
        public void ChangeSystolicTo(float newValue) => SystolicPressure = newValue;

        /// <summary>
        /// Updates <see cref="DiastolicPressure"/> measurement
        /// </summary>
        /// <param name="newValue"></param>
        public void ChangeDiastolicTo(float newValue) => throw new NotImplementedException();

        /// <summary>
        /// Updates
        /// </summary>
        /// <param name="newValue"></param>
        public void ChangeDateOfMeasure(DateTime newValue) => throw new NotImplementedException();
    }
}