namespace Measures.CQRS.Events.BloodPressures
{
    using Measures.DTO;

    using System;

    using static Newtonsoft.Json.JsonConvert;

    /// <summary>
    /// Event that notifies the creation of a new <see cref="BloodPressureInfo"/> resource .
    /// </summary>
    public class BloodPressureCreated : MeasureCreated<Guid, BloodPressureInfo>
    {
        /// <summary>
        /// Builds a new <see cref="BloodPressureCreated"/> instance
        /// </summary>
        /// <param name="measureInfo"></param>
        public BloodPressureCreated(BloodPressureInfo measureInfo) : base(Guid.NewGuid(), measureInfo)
        { }

        public override string ToString() => this.Jsonify();
    }
}
