using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;
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
