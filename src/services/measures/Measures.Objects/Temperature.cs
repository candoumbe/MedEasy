namespace Measures.Objects
{
    using Measures.Ids;

    using NodaTime;

    using System;

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
        /// <param name="id">Id of the record</param>
        /// <param name="subjectId">Id of the subject which the current measure belongs to</param>
        /// <param name="dateOfMeasure">Defines when the measure was made</param>
        /// <param name="value">Value of the measure</param>
        public Temperature(TemperatureId id, SubjectId subjectId, Instant dateOfMeasure, float value)
            : base(subjectId, id, dateOfMeasure)
        {
            Value = value;
        }

        public bool Equals(Temperature other) => ReferenceEquals(this, other)
            || (other != null && Value.Equals(other.Value));
    }
}