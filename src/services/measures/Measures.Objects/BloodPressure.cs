using System;

namespace Measures.Objects
{
    /// <summary>
    /// Defines a blood pressure measure.
    /// <para>
    /// The <see cref="SystolicBloodPressure"/> is always equals or greather than  <see cref="DiastolicPressure"/> 
    /// </para>
    /// <para>
    /// <remarks>
    /// All <see cref="BloodPressure"/> are expressed in milimeters of mercury (mm Hg) 
    /// </remarks>
    /// </para>
    /// </summary>
    public class BloodPressure : PhysiologicalMeasurement
    {
        /// <summary>
        /// Gets/sets the diastolic blood pressure
        /// </summary>
        public float DiastolicPressure { get; private set; }

        /// <summary>
        /// Get/set the systolic blood pressure
        /// 
        /// <para>
        /// </para>
        /// </summary>
        public float SystolicPressure { get; private set; }

        /// <summary>
        /// Builds a new <see cref="BloodPressure"/> instance.
        /// </summary>
        /// <param name="patientId">id of the <see cref="Patient"/> who the current measure will be attached to.</param>
        /// <param name="dateOfMeasure">dDate of the measure</param>
        /// <param name="diastolicPressure">The diastolic measure</param>
        /// <param name="systolicPressure">The systolic measure</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 
        /// <list type="bullet">
        ///     <item><paramref name="patientId"/> is <see cref="Guid.Empty"/></item>
        ///     <item><paramref name="id"/> is <see cref="Guid.Empty"/></item>
        ///     <item><paramref name="dateOfMeasure"/> is <see cref="DateTime.MinValue"/></item>
        ///     <item><paramref name="systolicPressure"/> is either negative, <see cref="float.NaN"/>, <see cref="float.PositiveInfinity"/></item>
        ///     <item><paramref name="diastolicPressure"/> is either negative, <see cref="float.NaN"/> or greater than <paramref name="systolicPressure"/>.</item>
        /// </list>
        /// 
        /// </exception>
        public BloodPressure(Guid patientId, Guid id, DateTime dateOfMeasure, float systolicPressure, float diastolicPressure)
            : base(patientId, id, dateOfMeasure)
        {
            if (float.IsNaN(systolicPressure))
            {
                throw new ArgumentOutOfRangeException(nameof(systolicPressure), $"{nameof(systolicPressure)} cannot be NaN");
            }

            if (float.IsNaN(diastolicPressure))
            {
                throw new ArgumentOutOfRangeException(nameof(diastolicPressure), $"{nameof(diastolicPressure)} cannot be NaN");
            }

            if (float.IsPositiveInfinity(systolicPressure))
            {
                throw new ArgumentOutOfRangeException(nameof(systolicPressure), $"{nameof(systolicPressure)} cannot be PositiveInfinity");
            }

            if (systolicPressure < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(systolicPressure), systolicPressure, $"{nameof(systolicPressure)} cannot be less than 0");
            }

            if (diastolicPressure < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(diastolicPressure), diastolicPressure, $"{nameof(diastolicPressure)} cannot be less than 0");
            }

            if (systolicPressure < diastolicPressure)
            {
                throw new ArgumentOutOfRangeException(nameof(systolicPressure), systolicPressure, $"{nameof(systolicPressure)} must be greater than or equal to {nameof(diastolicPressure)}");
            }

            SystolicPressure = systolicPressure;
            DiastolicPressure = diastolicPressure;
        }

        /// <summary>
        /// Updates the <see cref="SystolicPressure"/> measurement
        /// </summary>
        /// <param name="newValue"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newValue"/> is either :
        /// <list type="bullet">
        /// </list>
        /// <item>less than current <see cref="DiastolicPressure"/> value</item>
        /// <item>negative</item> 
        /// <item><see cref="float.NaN"/></item> 
        /// <item><see cref="float.PositiveInfinity"/></item> 
        /// </exception>
        public void ChangeSystolicTo(float newValue)
        {
#if NETSTANDARD2_0
            if (float.IsNaN(newValue))
            {
                throw new ArgumentOutOfRangeException(nameof(newValue));
            }

#else
            if (!float.IsNormal(newValue))
            {
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, $"{nameof(newValue)} is outside the range  [0; {float.MaxValue}] of valid values");
            }

#endif
            if (newValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, $"{nameof(SystolicPressure)} cannot be set to a value lower than 0");
            }

            if (newValue < DiastolicPressure)
            {
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, $"{nameof(newValue)} is lower than currrent {nameof(DiastolicPressure)} value ({DiastolicPressure})");
            }

            SystolicPressure = newValue;
        }

        /// <summary>
        /// Updates <see cref="DiastolicPressure"/> measurement
        /// </summary>
        /// <param name="newValue"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ChangeDiastolicTo(float newValue)
        {
            if (float.IsNaN(newValue))
            {
                throw new ArgumentOutOfRangeException(nameof(newValue),
                                                      newValue,
                                                      $"{nameof(DiastolicPressure)} cannot be set to {float.NaN}");
            }

            if (float.IsPositiveInfinity(newValue))
            {
                throw new ArgumentOutOfRangeException(nameof(newValue),
                                                          newValue,
                                                          $"{nameof(DiastolicPressure)} cannot be set to {float.PositiveInfinity}");
            }

            if (newValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newValue),
                                                      newValue,
                                                      $"{nameof(DiastolicPressure)} cannot be set to a value lower than 0");
            }

            if (newValue > SystolicPressure)
            {
                throw new ArgumentOutOfRangeException(nameof(newValue),
                                                      newValue,
                                                      $"{nameof(DiastolicPressure)} cannot be set to a value greater than the current value of {nameof(SystolicPressure)}");
            }

            DiastolicPressure = newValue;
        }

        /// <summary>
        /// Updates
        /// </summary>
        /// <param name="newValue"></param>
        public void ChangeDateOfMeasure(DateTime newValue) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override string ToString() => this.Jsonify();
    }
}