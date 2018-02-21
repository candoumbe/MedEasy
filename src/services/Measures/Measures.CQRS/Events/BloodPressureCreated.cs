using MedEasy.CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measures.CQRS.Events
{

    /// <summary>
    /// Event that notifies the creation of a new <see cref="BloodPressureInfo"/> resource .
    /// </summary>
    public class BloodPressureCreated : MeasureCreated<Guid, Guid>
    {
        /// <summary>
        /// Systolic measure
        /// </summary>
        public float Systolic { get; }

        /// <summary>
        /// Diastolic measure
        /// </summary>
        public float Diastolic { get; }

        /// <summary>
        /// Date of measure
        /// </summary>
        public DateTimeOffset DateOfMeasure { get; }


        /// <summary>
        /// Builds a new <see cref="BloodPressureCreated"/> instance
        /// </summary>
        /// <param name="measureId">Unique identifier of the created resource</param>
        /// <param name="systolic">Systolic measure</param>
        /// <param name="diastolic">diastolic measuure</param>
        /// <param name="dateOfMeasure">Date the measure was made</param>
        public BloodPressureCreated(Guid measureId, float systolic, float diastolic, DateTimeOffset dateOfMeasure) : base(Guid.NewGuid(), measureId)
        {
            Systolic = systolic;
            Diastolic = diastolic;
            DateOfMeasure = dateOfMeasure;
        }

    }
}
