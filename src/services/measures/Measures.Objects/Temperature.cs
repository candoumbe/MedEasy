using NodaTime;

using System;

namespace Measures.Objects
{
    /// <summary>
    /// An instance of this class represents a temperature measurement
    /// </summary>
    public class Temperature : PhysiologicalMeasurement, IEquatable<Temperature>
    {
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
        public Temperature(Guid id, Guid patientId, Instant dateOfMeasure, float value)
            : base(id, patientId, dateOfMeasure)
        {
            Value = value;
        }

        public bool Equals(Temperature other) => ReferenceEquals(this, other)
            || (other != null && Value.Equals(other.Value));
    }
}