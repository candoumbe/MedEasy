using Measures.DTO;
using MedEasy.CQRS.Core.Commands;
using System;

namespace Measures.CQRS.Commands.BloodPressures
{
    /// <summary>
    /// Command to create a new <see cref="BloodPressureInfo"/>.
    /// </summary>
    public class CreateBloodPressureInfoCommand : CommandBase<Guid, CreateBloodPressureInfo, BloodPressureInfo>
    {

        public CreateBloodPressureInfoCommand(CreateBloodPressureInfo data) : base(Guid.NewGuid(), data)
        {
        }
    }
}