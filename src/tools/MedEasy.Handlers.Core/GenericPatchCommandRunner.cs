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
using MedEasy.CQRS.Core;

namespace MedEasy.Handlers.Core.Commands
{
    /// <summary>
    /// Extends this class to create patch commands' runner.
    /// </summary>
    /// <typeparam name="TEntityId">Type of data command will carry</typeparam>
    /// <typeparam name="TEntity">Type of result the execution of the command will output</typeparam>
    public abstract class GenericPatchCommandRunner<TResourceId, TEntityId, TEntity> : CommandRunnerBase<Guid, PatchInfo<TResourceId, TEntity>, Nothing, IPatchCommand<TResourceId, TEntity>>
        where TEntity : class, IEntity<TEntityId>
    {
        protected IUnitOfWorkFactory UowFactory { get; }

        protected IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Builds a new <see cref="GenericPatchCommandRunner{TCommandId, TInput, TOutput, TCommand}"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory to build <see cref="IUnitOfWork"/> instances.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/> is <c>null</c>.</exception>
        protected GenericPatchCommandRunner(IUnitOfWorkFactory uowFactory) : base()
        {
            UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
        }

        public override async Task<Option<Nothing, CommandException>> RunAsync(IPatchCommand<TResourceId, TEntity> command, CancellationToken cancellationToken = default)
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

