using System;
using MedEasy.Commands.Patient;
using MedEasy.Handlers.Commands;
using MedEasy.DTO;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// An instance of this class process <see cref="IAddNewTemperatureMeasureCommand"/> commands
    /// </summary>
    public interface IRunAddNewTemperatureMeasureCommand : IRunCommandAsync<Guid, CreateTemperatureInfo, TemperatureInfo, IAddNewTemperatureMeasureCommand>
    {
        
    }
}
