using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using MedEasy.Objects;
using MedEasy.Handlers.Exceptions;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// An instance of this class process <see cref="IAddNewTemperatureMeasureCommand"/> commands
    /// </summary>
    public class RunAddNewPhysiologicalMeasureCommand<TEntity, TData, TOutput> : GenericCreateCommandRunner<Guid, TEntity, TData, TOutput, IAddNewPhysiologicalMeasureCommand<Guid, TData>>, IRunAddNewPhysiologicalMeasureCommand<Guid, TData, TOutput>
        where TEntity : PhysiologicalMeasurement
        where TData : CreatePhysiologicalMeasureInfo
    {
        
        /// <summary>
        /// Builds a new <see cref="RunAddNewPhysiologicalMeasureCommand"/> instance
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <param name="validator">Validator that will be used to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        public RunAddNewPhysiologicalMeasureCommand(IValidate<IAddNewPhysiologicalMeasureCommand<Guid, TData>> validator, ILogger<RunAddNewPhysiologicalMeasureCommand<TEntity, TData, TOutput>> logger, IUnitOfWorkFactory factory,
            IExpressionBuilder expressionBuilder) : base(validator, logger, factory, expressionBuilder)
        {
            
        }


        public override async Task OnCreatingAsync(Guid id, TData input)
        {
            using (var uow = UowFactory.New())
            {
                if (! await uow.Repository<Objects.Patient>().AnyAsync(x => x.Id == input.Id))
                {
                    throw new NotFoundException($"No patient found");
                }
            }
        }


    }
}
