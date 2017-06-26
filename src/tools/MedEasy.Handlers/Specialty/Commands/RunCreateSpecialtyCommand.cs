using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using MedEasy.Commands.Specialty;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Specialty.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Optional;

namespace MedEasy.Handlers.Specialty.Commands
{


    /// <summary>
    /// An instance of this class can run <see cref="IRunCreateSpecialtyCommand"/> commands
    /// </summary>
    public class RunCreateSpecialtyCommand : GenericCreateCommandRunner<Guid, Objects.Specialty, CreateSpecialtyInfo, SpecialtyInfo, ICreateSpecialtyCommand>, IRunCreateSpecialtyCommand
    {

        /// <summary>
        /// Builds a new <see cref="RunCreateSpecialtyCommand"/> instance.
        /// </summary>
        /// <param name="validator">Validator of <see cref="ICreateSpecialtyCommand"/> instances.</param>
        /// <param name="logger"></param>
        /// <param name="factory"></param>
        /// <param name="expressionBuilder"></param>
        public RunCreateSpecialtyCommand(IValidate<ICreateSpecialtyCommand> validator, ILogger<RunCreateSpecialtyCommand> logger, IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder) : base(validator, logger, factory, expressionBuilder)
        {
        }

        /// <inherithed />
        public override async Task OnCreatingAsync(Guid commandId, CreateSpecialtyInfo input)
        {
            input.Name = input.Name.ToTitleCase();

            using (IUnitOfWork uow = UowFactory.New())
            {
                if (await uow.Repository<Objects.Specialty>()
                    .AnyAsync(x => x.Name.ToLower() == input.Name.ToLower())
                    .ConfigureAwait(false))
                {
                    throw new CommandNotValidException<Guid>(commandId, new[] { new ErrorInfo("ErrDuplicate", "A specialty with this code already exists", ErrorLevel.Error) });
                }

            }
        }

    }
}
