using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Commands;
using System;
using MedEasy.Objects;
using System.Threading;
using Optional;
using MedEasy.CQRS.Core;

namespace MedEasy.Handlers.Core.Commands
{
    /// <summary>
    /// Generic handler for running delete commands to delete resource by their identifier
    /// </summary>
    /// <typeparam name="TCommandId">Type of the command identifier</typeparam>
    /// <typeparam name="TEntity">Type of the entity to create</typeparam>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    public abstract class GenericDeleteByIdCommandRunner<TCommandId, TEntity, TCommand> : GenericDeleteByIdCommandRunner<TCommandId, int, TEntity, TCommand>
        where TEntity : class, IEntity<int>
        where TCommandId : IEquatable<TCommandId>
        where TCommand : ICommand<TCommandId, Guid, Nothing>
    {

        /// <summary>
        /// Builds a new <see cref="GenericDeleteByIdCommandRunner{TCommandId, TEntity, TCommand}"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory to build instances of <see cref="IUnitOfWork"/></param>
        /// 
        /// 
        public GenericDeleteByIdCommandRunner(IUnitOfWorkFactory uowFactory) : base(uowFactory)
        {
        }
    }

    /// <summary>
    /// Generic handler for running delete commands to delete resource by their identifier
    /// </summary>
    /// <typeparam name="TCommandId">Type of the identifier</typeparam>
    /// <typeparam name="TEntity">Type of the entity to delete</typeparam>
    /// <typeparam name="TEntityId">Type of the <see cref="TEntity"/> identifier</typeparam>
    /// <typeparam name="TCommand">Type of the data the command this instance handles will carry</typeparam>
    public abstract class GenericDeleteByIdCommandRunner<TCommandId, TEntityId, TEntity, TCommand> : CommandRunnerBase<TCommandId, Guid, Nothing, TCommand>
        where TEntity : class, IEntity<TEntityId>
        where TCommandId : IEquatable<TCommandId>
        where TCommand : ICommand<TCommandId, Guid, Nothing>
    {

        /// <summary>
        /// Builds a new <see cref="GenericDeleteByIdCommandRunner{TKey, TEntity, TData, TCommand}"/> instance
        /// </summary>
        /// <param name="uowFactory">Factory to build instances of <see cref="IUnitOfWork"/></param>
        /// 
        /// 
        public GenericDeleteByIdCommandRunner(IUnitOfWorkFactory uowFactory)
        {
            UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
        }

        private IUnitOfWorkFactory UowFactory { get; }
        
        /// <summary>
        /// Process the command.
        /// The command is validates prior to being processed
        /// </summary>
        /// <param name="command">the command to run</param>
        /// <returns>The result of command's execution</returns>
        /// <exception cref="CommandNotValidException{TCommandId}">if  <paramref name="command"/> validation fails</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="command"/> is <c>null</c></exception>
        public override async Task<Option<Nothing, CommandException>> RunAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            using (IUnitOfWork uow = UowFactory.New())
            {
                Guid data = command.Data;

                uow.Repository<TEntity>().Delete(item => data.Equals(item.UUID));
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                return Option.Some<Nothing, CommandException>(Nothing.Value);

            }
        }
    }
}
