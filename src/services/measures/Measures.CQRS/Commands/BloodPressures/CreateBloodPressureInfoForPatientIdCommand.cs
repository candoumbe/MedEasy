using Measures.DTO;

using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;

using Optional;

using System;

namespace Measures.CQRS.Commands.BloodPressures
{
    /// <summary>
    /// Command to create a new <see cref="BloodPressureInfo"/> and attach it to 
    /// its <see cref="PatientInfo"/> resource.
    /// </summary>
    /// <see cref="CommandBase{TKey, TData, TResult}"/>
    public class CreateBloodPressureInfoForPatientIdCommand : CommandBase<Guid, CreateBloodPressureInfo, Option<BloodPressureInfo, CreateCommandResult>>
    {
        /// <summary>
        /// Creates a new <see cref="CreateBloodPressureInfoForPatientIdCommand"/> instance.
        /// </summary>
        /// <param name="data">data associated with the command</param>
        public CreateBloodPressureInfoForPatientIdCommand(CreateBloodPressureInfo data) : base(Guid.NewGuid(), data)
        {
        }
    }
}