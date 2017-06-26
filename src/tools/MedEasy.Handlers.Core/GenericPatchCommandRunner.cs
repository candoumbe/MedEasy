using System;
using System.Collections.Generic;
using System.Text;
using MedEasy.Validators;
using MedEasy.DAL.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Optional;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.JsonPatch;
using MedEasy.Commands;
using MedEasy.DTO;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch.Operations;
using MedEasy.Objects;
using MedEasy.Handlers.Core.Exceptions;

namespace MedEasy.Handlers.Core.Commands
{
    /// <summary>
    /// Extends this class to create patch commands' runner.
    /// </summary>
    /// <typeparam name="TCommandId">Type of the command identifier</typeparam>
    /// <typeparam name="TEntityId">Type of data command will carry</typeparam>
    /// <typeparam name="TEntity">Type of result the execution of the command will output</typeparam>
    /// <typeparam name="TCommand">Type of commands to run</typeparam>
    public abstract class GenericPatchCommandRunner<TCommandId, TResourceId, TEntityId,  TEntity, TCommand> : CommandRunnerBase<TCommandId, IPatchInfo<TResourceId, TEntity>, Nothing, TCommand>
        where TCommandId : IEquatable<TCommandId>
        where TCommand : IPatchCommand<TCommandId, TResourceId, TEntity, IPatchInfo<TResourceId, TEntity>>
        where TEntity : class, IEntity<TEntityId>
    {
        protected IUnitOfWorkFactory UowFactory { get; }

        protected IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Builds a new <see cref="GenericPatchCommandRunner{TCommandId, TInput, TOutput, TCommand}"/> instance.
        /// </summary>
        /// <param name="validator">Validate the command</param>
        /// <param name="uowFactory">Factory to build <see cref="IUnitOfWork"/> instances.</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="uowFactory"/> or <paramref name="validator"/> is <c>null</c>.</exception>
        protected GenericPatchCommandRunner(IValidate<TCommand> validator, IUnitOfWorkFactory uowFactory) : base(validator)
        {
            UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
        }

        public override async Task<Option<Nothing, CommandException>> RunAsync(TCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (IUnitOfWork uow = UowFactory.New())
            {
                TResourceId patientId = command.Data.Id;
                Option<TEntity> source = await uow.Repository<TEntity>()
                    .SingleOrDefaultAsync(x => Equals(x.UUID, command.Data.Id))
                    .ConfigureAwait(false);

                Option<Nothing, CommandException> patchResult = source.Match(
                    some: x =>
                    {
                        JsonPatchDocument<TEntity> changes = command.Data.PatchDocument;
                        changes.ApplyTo(x);

                        return Option.Some<Nothing, CommandException>(Nothing.Value);
                    },
                    none: () => Option.None<Nothing, CommandException>(new CommandEntityNotFoundException($"Element <{command.Data.Id}> not found."))
                );

                if (patchResult.HasValue)
                {
                    await uow.SaveChangesAsync()
                        .ConfigureAwait(false);
                }

                return patchResult;
            }
        }

    }

}

