using Measures.Ids;

using NodaTime;

using System;

namespace Measures.Objects
{
    /// <summary>
    /// An instance of this class represents a temperature measurement
    /// </summary>
    public sealed class Temperature : PhysiologicalMeasurement<TemperatureId>, IEquatable<Temperature>
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
        public Temperature(TemperatureId id, PatientId patientId, Instant dateOfMeasure, float value)
            : base(patientId, id, dateOfMeasure)
        {
            Value = value;
        }

        public bool Equals(Temperature other) => ReferenceEquals(this, other)
            || (other != null && Value.Equals(other.Value));
    }
}