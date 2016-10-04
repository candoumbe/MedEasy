using AutoMapper.QueryableExtensions;
using MedEasy.Commands;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using MedEasy.Handlers.Exceptions;
using MedEasy.Objects;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Handlers.Patient.Commands
{
    public class RunAddPhysiologicalMeasureCommand<TEntity, TData, TOutput, TCommand> : GenericCreateCommandRunner<Guid, TEntity, TData, TOutput, TCommand>
        where TEntity : PhysiologicalMeasurement
        where TData : CreatePhysiologicalMeasureInfo
        where TCommand : AddNewPhysiologicalMeasureCommand<Guid, TData, TOutput>
    {
        public RunAddPhysiologicalMeasureCommand(IValidate<TCommand> validator, ILogger<RunAddPhysiologicalMeasureCommand<TEntity, TData, TOutput, TCommand>> logger, IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder) : base(validator, logger, uowFactory, expressionBuilder)
        {
        }


        public override async Task OnCreatingAsync(Guid id, TData input)
        {
            using (var uow = UowFactory.New())
            {
                if (!await uow.Repository<Objects.Patient>().AnyAsync(x => x.Id == input.Id))
                {
                    throw new NotFoundException($"{nameof(Objects.Patient)} with {nameof(Objects.Patient.Id)} not found");
                }

                await base.OnCreatingAsync(id, input);
            }


        }
    }
}
