using Measures.DTO;

using MedEasy.CQRS.Core.Commands;

using System;

namespace Measures.CQRS.Commands.GenericMeasures
{
    public class CreateGenericMeasureFormInfoCommand : CommandBase<Guid, CreateGenericMeasureFormInfo, GenericMeasureFormInfo>
    {
        public CreateGenericMeasureFormInfoCommand(CreateGenericMeasureFormInfo data) : base(Guid.NewGuid(), data)
        {

        }
    }
}
