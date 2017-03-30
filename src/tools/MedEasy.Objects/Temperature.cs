using System;

namespace MedEasy.Objects
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

        public bool Equals(Temperature other) => ReferenceEquals(this, other) || (other != null && Value.Equals(other.Value));


    }
}