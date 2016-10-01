using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Commands.Patient
{

    /// <summary>
    /// Command to create a new measure of temperature
    /// </summary>
    public class AddNewTemperatureCommand : CommandBase<Guid, CreateTemperatureInfo>, IAddNewTemperatureMeasureCommand
    {
        /// <summary>
        /// Builds a new <see cref="AddNewTemperatureCommand"/> instance
        /// </summary>
        /// <param name="data">data of the command</param>
        public AddNewTemperatureCommand(CreateTemperatureInfo data) : base(Guid.NewGuid(), data)
        {

        }

        
    }
}
