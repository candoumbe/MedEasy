using Measures.DTO;

using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;

using Optional;

using System;

namespace Measures.CQRS.Commands
{
    /// <summary>
    /// A command to create a new generic measure form.
    /// </summary>
    public class CreateGenericMeasureFormInfoCommand : CommandBase<Guid, CreateGenericMeasureFormInfo, GenericMeasureFormInfo>
    {
        public CreateGenericMeasureFormInfoCommand(CreateGenericMeasureFormInfo data) : base(Guid.NewGuid(), data)
        { 
        }
    }
}
