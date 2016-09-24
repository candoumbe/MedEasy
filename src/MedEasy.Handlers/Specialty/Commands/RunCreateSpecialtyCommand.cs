using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using MedEasy.Commands.Specialty;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Commands;
using MedEasy.Handlers.Exceptions;

namespace MedEasy.Handlers.Specialty.Commands
{


    /// <summary>
    /// An instance of this class can run <see cref="IRunCreateSpecialtyCommand"/> commands
    /// </summary>
    public class RunCreateSpecialtyCommand : GenericCreateCommandRunner<Guid, Objects.Specialty, CreateSpecialtyInfo, SpecialtyInfo, ICreateSpecialtyCommand>, IRunCreateSpecialtyCommand
    {


        public RunCreateSpecialtyCommand(IValidate<ICreateSpecialtyCommand> validator, ILogger<RunCreateSpecialtyCommand> logger, IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder) : base(validator, logger, factory, expressionBuilder)
        {
        }

        /// <inherithed />
        public override async Task OnCreatingAsync(Guid commandId, CreateSpecialtyInfo input)
        {
            using (var uow = UowFactory.New())
            {
                if (await uow.Repository<Objects.Specialty>().AnyAsync(x => x.Code == input.Code).ConfigureAwait(false))
                {
                    throw new CommandNotValidException<Guid>(commandId, new[] { new ErrorInfo("ErrDuplicate", "A specialty with this code already exists", ErrorLevel.Error) });
                }

                input.Code = input.Code?.ToUpper();
                input.Name = input.Name?.ToTitleCase();

            }
        }

    }
}
