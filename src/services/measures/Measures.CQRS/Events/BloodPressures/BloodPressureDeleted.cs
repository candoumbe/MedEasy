﻿namespace Measures.CQRS.Events.BloodPressures
{
    using Measures.Ids;

    using System;

    /// <summary>
    /// Event that notifies the deletion of a <see cref="BloodPressureInfo"/> resource .
    /// </summary>
    public class BloodPressureDeleted : MeasureDeleted<Guid, BloodPressureId>
    {
        /// <summary>
        /// Builds a new <see cref="BloodPressureDeleted"/> instance
        /// </summary>
        /// <param name="measureId">Unique identifier of the created resource</param>
        public BloodPressureDeleted(BloodPressureId measureId) : base(Guid.NewGuid(), measureId)
        {
        }
    }
}
