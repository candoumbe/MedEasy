namespace Measures.CQRS.Events.BloodPressures
{
    using Measures.Ids;

    using System;

    /// <summary>
    /// Notifies that a <see cref="BloodPressureInfo"/> was updated.
    /// </summary>
    public class BloodPressureUpdated : MeasureDeleted<Guid, BloodPressureId>
    {
        /// <summary>
        /// Builds a new <see cref="BloodPressureUpdated"/> instance
        /// </summary>
        /// <param name="measureId">Unique identifier of the updated <see cref="Objects.BloodPressure"/></param>
        public BloodPressureUpdated(BloodPressureId measureId) : base(Guid.NewGuid(), measureId)
        {
        }
    }
}
