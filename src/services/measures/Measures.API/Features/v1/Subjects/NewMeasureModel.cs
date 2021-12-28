namespace Measures.API.Features.Subjects
{
    using NodaTime;

    /// <summary>
    /// Base class for model to create new measure
    /// </summary>
    public abstract class NewMeasureModel
    {
        /// <summary>
        /// Indicates when the measure was made
        /// </summary>
        public Instant DateOfMeasure { get; set; }
    }
}
