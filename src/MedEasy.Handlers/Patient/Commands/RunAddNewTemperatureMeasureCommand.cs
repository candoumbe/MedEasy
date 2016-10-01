using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using System.Collections.Generic;
using MedEasy.Objects;
using MedEasy.Handlers.Exceptions;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// An instance of this class process <see cref="IAddNewTemperatureMeasureCommand"/> commands
    /// </summary>
    public class RunAddNewTemperatureMeasureCommand : GenericCreateCommandRunner<Guid, Temperature, CreateTemperatureInfo, TemperatureInfo, IAddNewTemperatureMeasureCommand>, IRunAddNewTemperatureMeasureCommand
    {
        
        /// <summary>
        /// Builds a new <see cref="RunAddNewTemperatureMeasureCommand"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <param name="validator">Validator that will be used to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        public RunAddNewTemperatureMeasureCommand(IValidate<IAddNewTemperatureMeasureCommand> validator, ILogger<RunAddNewTemperatureMeasureCommand> logger, IUnitOfWorkFactory factory,
            IExpressionBuilder expressionBuilder) : base(validator, logger, factory, expressionBuilder)
        {
            
        }


        public override async Task OnCreatingAsync(Guid id, CreateTemperatureInfo input)
        {
            using (var uow = UowFactory.New())
            {
                if (!await uow.Repository<Objects.Patient>().AnyAsync(x => x.Id == input.PatientId))
                {
                    throw new NotFoundException($"{nameof(Objects.Patient)} with {nameof(Objects.Patient.Id)} not found");
                }

                await base.OnCreatingAsync(id, input);
            }

            
        }


    }
}
