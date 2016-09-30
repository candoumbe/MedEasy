using System.Threading.Tasks;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using MedEasy.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using static MedEasy.Validators.ErrorLevel;
using MedEasy.Handlers.Exceptions;
using MedEasy.Commands;
using AutoMapper.QueryableExtensions;
using System;

namespace MedEasy.Handlers.Commands
{
    /// <summary>
    /// Generic handler for create commands
    /// </summary>
    /// <typeparam name="TCommandId">Type of the command identifier</typeparam>
    /// <typeparam name="TEntity">Type of the entity to create</typeparam>
    /// <typeparam name="TData">Type of the data the command this instance handles will carry</typeparam>
    /// <typeparam name="TOutput">Type of the TCommand outputs</typeparam>
    /// <typeparam name="TCommand">Type of the commmand</typeparam>
    public abstract class GenericCreateCommandRunner<TCommandId, TEntity, TData, TOutput, TCommand> : CommandRunnerBase<TCommandId, TData, TOutput, TCommand>
        where TCommand : ICommand<TCommandId, TData>
        where TEntity : class
        where TCommandId : IEquatable<TCommandId>
    {

        /// <summary>
        /// Builds a new <see cref="GenericCreateCommandRunner{TKey, TEntity, TData, TOutput, TCommand}"/> instance
        /// </summary>
        /// <param name="validator">Validator to use to validate commands in <see cref="HandleAsync(TCommand)"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="uowFactory">Factory to build instances of <see cref="IUnitOfWork"/></param>
        /// <param name="dataToEntityMapper">Function to convert commands data to entity</param>
        /// <param name="entityToOutputMapper">Function to convert entity to command</param>
        public GenericCreateCommandRunner(IValidate<TCommand> validator, 
            ILogger<GenericCreateCommandRunner<TCommandId, TEntity, TData, TOutput, TCommand>> logger, 
            IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder
            ) : base (validator)
        {
            UowFactory = uowFactory;
            Logger = logger;
            ExpressionBuilder = expressionBuilder;
        }

        public IUnitOfWorkFactory UowFactory { get; }

        public ILogger<GenericCreateCommandRunner<TCommandId, TEntity, TData, TOutput, TCommand>> Logger { get; }

        public IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Hook to customize the input before 
        /// </summary>
        /// <param name="input">The input that contains</param>
        /// <returns></returns>
        public virtual Task OnCreatingAsync(TCommandId id, TData input) => Task.CompletedTask;

        /// <summary>
        /// Hook to customize the output of the command
        /// </summary>
        /// <param name="input"></param>
        public virtual Task OnCreatedAsync(TOutput output) => Task.CompletedTask;

        /// <summary>
        /// Process the command.
        /// The command is validates prior to being processed
        /// </summary>
        /// <param name="command">command to run</param>
        /// <returns>The result of the command execution</returns>
        /// <exception cref="CommandNotValidException{TCommandId}"> : 
        ///     - if <paramref name="command"/> validation fails,
        /// </exception>
        /// <exception cref="ArgumentNullException">if <paramref name="command"/> is <c>null</c></exception>
        public override async Task<TOutput> RunAsync(TCommand command)
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
                foreach (var error in errors)
                {
                    Logger.LogDebug($"{error.Key} - {error.Severity} : {error.Description}");
                }
#endif
                throw new CommandNotValidException<TCommandId>(command.Id, errors);

            }
            Logger.LogTrace("Command validation succeeded");

            using (var uow = UowFactory.New())
            {
                TData data = command.Data;
                await OnCreatingAsync(command.Id, data);
                TEntity entity = ExpressionBuilder.CreateMapExpression<TData, TEntity>().Compile().Invoke(data);

                uow.Repository<TEntity>().Create(entity);
                await uow.SaveChangesAsync();

                TOutput output = ExpressionBuilder.CreateMapExpression<TEntity, TOutput>().Compile().Invoke(entity);
                await OnCreatedAsync(output);
                Logger.LogInformation($"Command {command.Id} processed successfully");


                return output;
            }
        }
    }
}
