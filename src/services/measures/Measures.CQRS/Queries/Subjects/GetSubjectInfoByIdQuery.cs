namespace Measures.CQRS.Queries.Subjects
{
    using Measures.DTO;
    using Measures.Ids;

    using MedEasy.CQRS.Core.Queries;

    using Optional;

    using System;

    /// <summary>
    /// Query to read a <see cref="SubjectInfo"/> resource by its id.
    /// </summary>
    /// <remarks>
    /// Execution of a <see cref="GetSubjectInfoByIdQuery"/> returns an <see cref="Option{PatientInfo}"/>
    /// </remarks>
    public class GetSubjectInfoByIdQuery : GetOneResourceQuery<Guid, SubjectId, Option<SubjectInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetSubjectInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="id">id of the resource to read</param>
        public GetSubjectInfoByIdQuery(SubjectId id) : base(Guid.NewGuid(), id)
        {
        }
    }
}
