﻿using System;

namespace MedEasy.Objects
{
    /// <summary>
    /// An instance of this class represents a body weight measurement
    /// </summary>
    public class BodyWeight : PhysiologicalMeasurement, IEquatable<BodyWeight>
    {
        /// <summary>
        /// Gets / sets the temperature
        /// </summary>
        public decimal Value { get; set; }

        public bool Equals(BodyWeight other) => ReferenceEquals(this, other) || (other != null && Value.Equals(other.Value));


    }
}