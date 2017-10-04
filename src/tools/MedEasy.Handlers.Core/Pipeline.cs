using MedEasy.Commands;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Optional;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Handlers.Core
{
    /// <summary>
    /// Composes a list of components that the command will flow through before hitting the command handler.
    /// </summary>
    /// <remarks>
    /// Each command has a change to look the command an perform action(s) before or after
    /// </remarks>
    /// <typeparam name="TCommandId">Type of the command identifier</typeparam>
    /// <typeparam name="TCommandData">Type fot </typeparam>
    /// <typeparam name="TCommandResult">Type of the result of the command</typeparam>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    public class Pipeline<TCommandId, TCommandData, TCommandResult, TCommand> : IRunCommandAsync<TCommandId, TCommandData, TCommandResult, TCommand>
        where TCommand : ICommand<TCommandId, TCommandData, TCommandResult>
        where TCommandId : IEquatable<TCommandId>
    {
        private readonly IEnumerable<IPreprocess<TCommandData>> _preprocessors;
        private readonly IRunCommandAsync<TCommandId, TCommandData, TCommandResult, TCommand> _inner;
        private readonly IEnumerable<IPostProcess<TCommand, Option<TCommandResult, CommandException>>> _postprocessors;

        /// <summary>
        /// Builds a new <see cref="Pipeline{TCommandId, TCommandData, TCommandResult, TCommand}"/> instance.
        /// </summary>
        /// <param name="inner">The inner handler</param>
        /// <param name="preprocessors">Decorator that will </param>
        /// <param name="postProcessors"></param>
        public Pipeline(IRunCommandAsync<TCommandId, TCommandData, TCommandResult, TCommand> inner, IEnumerable<IPreprocess<TCommandData>> preprocessors, IEnumerable<IPostProcess<TCommand, Option<TCommandResult, CommandException>>> postProcessors)
        {
            _preprocessors = preprocessors;
            _inner = inner;
            _postprocessors = postProcessors;
        }

        /// <summary>
        /// Runs the command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Option<TCommandResult, CommandException>> RunAsync(TCommand command, CancellationToken cancellationToken = default) {

            await _preprocessors.ForEachAsync(pp => pp.Preprocess(command.Data))
                .ConfigureAwait(false);

            Option<TCommandResult, CommandException> result = await _inner.RunAsync(command, cancellationToken)
                .ConfigureAwait(false);

            await _postprocessors.ForEachAsync(pp => pp.PostProcess(command, result))
                .ConfigureAwait(false);

            return result;
        }
    }
}
