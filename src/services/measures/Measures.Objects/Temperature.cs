using System;

namespace Measures.Objects
{
    /// <summary>
    /// An instance of this class represents a temperature measurement
    /// </summary>
    public class Temperature : PhysiologicalMeasurement, IEquatable<Temperature>
    {
        public const float LowestKnownTemperature = -273.15f;

        /// <summary>
        /// Gets / sets the temperature
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Builds a new <see cref="Temperature"/> instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patientId"></param>
        /// <param name="dateOfMeasure"></param>
        /// <param name="value"></param>
        public Temperature(Guid id, Guid patientId, DateTime dateOfMeasure, float value)
            : base(patientId, id, dateOfMeasure)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < LowestKnownTemperature)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"The value is outside the range [{LowestKnownTemperature} ; {float.MaxValue}] of valid values");
            }
            Value = value;
        }

        public bool Equals(Temperature other) => ReferenceEquals(this, other)
            || (other != null && Value.Equals(other.Value));

        /// <summary>
        /// Updates <see cref="Value"/>
        /// </summary>
        /// <param name="newValue"></param>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="newValue"/> is outside the range <see cref="float.MinValue"/> to <see cref="float.MaxValue"/>.</exception>
        public void ChangeValueTo(float newValue)
        {
            if (float.IsNaN(newValue) || float.IsInfinity(newValue) || newValue < LowestKnownTemperature)
            {
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, $"The new value is outside the range [{LowestKnownTemperature} ; {float.MaxValue}] of valid values");
            }

            Value = newValue;
        }
    }
}