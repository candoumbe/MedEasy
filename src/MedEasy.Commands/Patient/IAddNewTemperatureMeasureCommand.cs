using MedEasy.DTO;
using System;

namespace MedEasy.Commands.Patient
{
    /// <summary>
    /// The contract for classes that must be treated as commands to add new measure of temperature
    /// </summary>
    /// <see cref="CreateTemperatureInfo"/>
    public interface IAddNewTemperatureMeasureCommand : ICommand<Guid, CreateTemperatureInfo>
    {

    }
}