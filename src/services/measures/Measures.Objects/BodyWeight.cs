using System;

namespace Measures.Objects
{
    /// <summary>
    /// An instance of this class represents a body weight measurement.
    /// 
    /// <para>
    /// <see cref="Value"/> is expressed in the international unit system : the Kilogram (see https://en.wikipedia.org/wiki/Kilogram)
    /// </para>
    /// </summary>
    public class BodyWeight : PhysiologicalMeasurement, IEquatable<BodyWeight>
    {
        /// <summary>
        /// Gets the current value of the messure (expressed in Kilogram)
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        /// Builds a new <see cref="BodyWeight"/> instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patientId"></param>
        /// <param name="dateOfMeasure"></param>
        /// <param name="value"></param>
        public BodyWeight(Guid id, Guid patientId, DateTime dateOfMeasure, float value)
           : base(id, patientId, dateOfMeasure)
        {
            if (float.IsNaN(value) || float.IsPositiveInfinity(value) || value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, ValueIsOutsideRangeOfValidValuesMessage);
            }

            Value = value;
        }

        /// <summary>
        /// Updates <see cref="Value"/>.
        /// </summary>
        /// <param name="newValue"></param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     if either :
        /// <list type="bullet">
        ///         
        /// </list>
        /// </exception>
        public void ChangeValueTo(float newValue)
        {
            if (float.IsNaN(newValue) || float.IsPositiveInfinity(newValue) || newValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, ValueIsOutsideRangeOfValidValuesMessage);
            }
            Value = newValue;
        }

        public bool Equals(BodyWeight other) => ReferenceEquals(this, other) || (other != null && Value.Equals(other.Value));
    }
}