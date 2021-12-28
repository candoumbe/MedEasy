namespace Measures.CQRS.Commands.Patients
{
    using Measures.DTO;

    using MedEasy.CQRS.Core.Commands;

    using System;

    /// <summary>
    /// Command to create <see cref="SubjectInfo"/> resources.
    /// </summary>
    public class CreateSubjectInfoCommand : CommandBase<Guid, NewSubjectInfo, SubjectInfo>
    {
        /// <summary>
        /// Creates a new <see cref="CreateSubjectInfoCommand"/> resource 
        /// </summary>
        /// <param name="data">informations on the resource to create</param>
        public CreateSubjectInfoCommand(NewSubjectInfo data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
