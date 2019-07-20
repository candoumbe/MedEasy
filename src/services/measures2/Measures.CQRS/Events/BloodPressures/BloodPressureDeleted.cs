using System;

namespace Measures.CQRS.Events.BloodPressures
{
    /// <summary>
    /// Event that notifies the deletion of a <see cref="BloodPressureInfo"/> resource .
    /// </summary>
    public class BloodPressureDeleted : MeasureDeleted<Guid, Guid>
    {
        /// <summary>
        /// Builds a new <see cref="BloodPressureDeleted"/> instance
        /// </summary>
        /// <param name="measureId">Unique identifier of the created resource</param>
        public BloodPressureDeleted(Guid measureId) : base(Guid.NewGuid(), measureId)
        {
        }
    }
}
