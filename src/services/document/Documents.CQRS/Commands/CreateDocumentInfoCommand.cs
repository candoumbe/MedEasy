using Documents.DTO;
using Documents.DTO.v1;

using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;

using Optional;

using System;

namespace Documents.CQRS.Commands
{
    /// <summary>
    ///  A <see cref="CommandBase{Guid, NewDocumentInfo, Option}"/> implementation to create a new document
    /// </summary>
    public class CreateDocumentInfoCommand : CommandBase<Guid, NewDocumentInfo, Option<DocumentInfo, CreateCommandResult>>
    {
        /// <summary>
        /// Builds a new <see cref="CreateDocumentInfoCommand"/> instance
        /// </summary>
        /// <param name="data">data to create a new <see cref="DocumentInfo"/> resource</param>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c>.</exception>
        public CreateDocumentInfoCommand(NewDocumentInfo data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
