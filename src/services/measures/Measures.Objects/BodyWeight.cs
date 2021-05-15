namespace Measures.Objects
{
    using Measures.Ids;

    using NodaTime;

    using System;

    /// <summary>
    /// An instance of this class represents a body weight measurement
    /// </summary>
    public sealed class BodyWeight : PhysiologicalMeasurement<BodyWeightId>, IEquatable<BodyWeight>
    {
        /// <summary>
        /// Gets / sets the temperature
        /// </summary>
        public decimal Value { get; set; }

        public BodyWeight(BodyWeightId id, PatientId patientId, Instant dateOfMeasure, decimal value)
           : base(patientId, id, dateOfMeasure)
        {
            Value = value;
        }

        public bool Equals(BodyWeight other) => ReferenceEquals(this, other) || (other != null && Value.Equals(other.Value));
    }
}