using System.Threading.Tasks;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using MedEasy.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using static MedEasy.Validators.ErrorLevel;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Commands;
using System;
using MedEasy.Objects;
using System.Threading;

namespace MedEasy.Handlers.Core.Commands
{
    /// <summary>
    /// Generic handler for running delete commands to delete resource by their identifier
    /// </summary>
    /// <typeparam name="TKey">Type of the identifier</typeparam>
    /// <typeparam name="TEntity">Type of the entity to create</typeparam>
    /// <typeparam name="TData">Type of the data the command this instance handles will carry</typeparam>
    /// <typeparam name="TOutput">Type of the TCommand outputs</typeparam>
    /// <typeparam name="TCommand">Type of the commmand</typeparam>
    public abstract class GenericDeleteByIdCommandRunner<TKey, TEntity, TCommand> : CommandRunnerBase<TKey, Guid, TCommand>
        where TCommand : ICommand<TKey, Guid>
        where TEntity : class, IEntity<int>
        where TKey : IEquatable<TKey>
    {

        /// <summary>
        /// Builds a new <see cref="GenericDeleteByIdCommandRunner{TKey, TEntity, TData, TCommand}"/> instance
        /// </summary>
        /// <param name="validator">Validator to use to validate commands in <see cref="HandleAsync(TCommand)"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="uowFactory">Factory to build instances of <see cref="IUnitOfWork"/></param>
        /// <param name="dataToEntityMapper">Function to convert commands data to entity</param>
        /// <param name="entityToOutputMapper">Function to convert entity to command</param>
        public GenericDeleteByIdCommandRunner(IValidate<TCommand> validator,
            ILogger<GenericDeleteByIdCommandRunner<TKey, TEntity, TCommand>> logger,
            IUnitOfWorkFactory uowFactory) : base (validator)
        {
            UowFactory = uowFactory;
            Logger = logger;
        }

        public IUnitOfWorkFactory UowFactory { get; }

        public ILogger<GenericDeleteByIdCommandRunner<TKey, TEntity, TCommand>> Logger { get; }


        /// <summary>
        /// Process the command.
        /// The command is validates prior to being processed
        /// </summary>
        /// <param name="command">the command to run</param>
        /// <returns>The result of command's execution</returns>
        /// <exception cref="CommandNotValidException{TCommandId}">if  <paramref name="command"/> validation fails</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="command"/> is <c>null</c></exception>
        public override async Task<Nothing> RunAsync(TCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            Logger.LogInformation($"Start processing command : {command.Id}");
            Logger.LogTrace("Validating command");
            IEnumerable<Task<ErrorInfo>> errorsTasks = Validator.Validate(command);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(errorsTasks).ConfigureAwait(false);
            if (errors.Any(item => item.Severity == Error))
            {
                Logger.LogTrace("validation failed", errors);
#if DEBUG || TRACE
                foreach (ErrorInfo error in errors)
                {
                    Logger.LogDebug($"{error.Key} - {error.Severity} : {error.Description}");
                }
#endif
                throw new CommandNotValidException<TKey>(command.Id, errors);

            }
            Logger.LogTrace("Command validation succeeded");

            using (IUnitOfWork uow = UowFactory.New())
            {
                Guid data = command.Data;
                
                uow.Repository<TEntity>().Delete(item => data.Equals(item.UUID));
                await uow.SaveChangesAsync();

                Logger.LogInformation($"Command {command.Id} processed successfully");

                return Nothing.Value;

            }
        }
    }
}
