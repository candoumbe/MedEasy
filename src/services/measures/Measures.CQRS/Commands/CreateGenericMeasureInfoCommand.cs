using Measures.DTO;

using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;

using Optional;

using System;

namespace Measures.CQRS.Commands
{
    /// <summary>
    /// A command to create a new generic measure for a "patient"
    /// </summary>
    public class CreateGenericMeasureInfoCommand : CommandBase<Guid, CreateGenericMeasureInfo, Option<GenericMeasureInfo, CreateCommandResult>>
    {
        public CreateGenericMeasureInfoCommand(CreateGenericMeasureInfo data) : base(Guid.NewGuid(), data)
        { 
        }
    }
}
