namespace Measures.DTO
{
    using Measures.Ids;

    using MedEasy.Ids;
    using MedEasy.RestObjects;

    using NodaTime;

    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Base class for physiological measure resources
    /// </summary>
    /// <typeparam name="TId">Type of the identifier</typeparam>
    public abstract class PhysiologicalMeasurementInfo<TId> : Resource<TId>
        where TId : StronglyTypedId<Guid>, IEquatable<TId>
    {
        /// <summary>
        /// Id of the <see cref="SubjectInfo"/> resource the measure was taken on
        /// </summary>
        public SubjectId SubjectId { get; set; }

        /// <summary>
        /// When the measure was made
        /// </summary>
        [DataType(DataType.DateTime)]
        public Instant DateOfMeasure { get; set; }
    }
}
