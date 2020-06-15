using FluentAssertions;

using Measures.CQRS.Commands.GenericMeasures;
using Measures.DTO;

using MedEasy.CQRS.Core.Commands;

using System;

using Xunit;

namespace Measures.CQRS.UnitTests.Commands.GenericMeasures
{
    public class CreateGenericMeasureFormInfoCommandTests
    {
        [Fact]
        public void Is_a_command()
        {
            Type createGenericMeasureFormCommandType = typeof(CreateGenericMeasureFormInfoCommand);

            createGenericMeasureFormCommandType.Should()
                                               .BeDerivedFrom<CommandBase<Guid, CreateGenericMeasureFormInfo, GenericMeasureFormInfo>>().And
                                               .NotHaveDefaultConstructor();
        }
    }
}
