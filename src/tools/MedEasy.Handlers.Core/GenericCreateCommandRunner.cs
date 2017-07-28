using AutoMapper.QueryableExtensions;
using MedEasy.Commands;
using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MedEasy.Validators.ErrorLevel;

namespace MedEasy.Handlers.Core.Commands
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
        where TEntity : class
        where TCommandId : IEquatable<TCommandId>
        where TCommand : ICommand<TCommandId, TData, TOutput>
    {

        /// <summary>
        /// Builds a new <see cref="GenericCreateCommandRunner{TKey, TEntity, TData, TOutput, TCommand}"/> instance
        /// </summary>
        /// <param name="expressionBuilder">Buiulder expression to map <see cref="TEntity"/> instance to <see cref="TOutput"/> </param>
        /// <param name="uowFactory">Factory to build instances of <see cref="IUnitOfWork"/></param>
        public GenericCreateCommandRunner(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            ExpressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        protected IUnitOfWorkFactory UowFactory { get; }

        protected IExpressionBuilder ExpressionBuilder { get; }


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
        public override async Task<Option<TOutput, CommandException>> RunAsync(TCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            using (IUnitOfWork uow = UowFactory.New())
            {
                Option<TOutput, CommandException> result = default(Option<TOutput, CommandException>);
                try
                {
                    TData data = command.Data;
                    TEntity entity = ExpressionBuilder.GetMapExpression<TData, TEntity>().Compile().Invoke(data);

                    uow.Repository<TEntity>().Create(entity);
                    await uow.SaveChangesAsync();

                    TOutput output = ExpressionBuilder.GetMapExpression<TEntity, TOutput>().Compile().Invoke(entity);
                    result = output.Some<TOutput, CommandException>();
                }
                catch (CommandException ex)
                {

                    result = Option.None<TOutput, CommandException>(ex);
                }

                return result;
            }

        }
    }
}
