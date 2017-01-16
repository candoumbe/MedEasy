using MedEasy.Commands;
using MedEasy.Handlers.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Pipeline
{
    /// <summary>
    /// A pipeline to build
    /// </summary>
    /// <typeparam name="TCommandId"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    public class Pipeline<TCommandId, TData, TOutput, TCommand> : IRunCommandAsync<TCommandId, TData, TOutput, TCommand>
        where TCommandId : IEquatable<TCommandId>
        where TCommand : ICommand<TCommandId, TData>
    {
        private readonly IRunCommandAsync<TCommandId, TData, TOutput, TCommand> _handler;
        private readonly IEnumerable<IPostCommandHandler<TCommandId, TData, TOutput>> _postHandlers;

        /// <summary>
        /// Builds a new <see cref="Pipeline{TCommandId, TData, TOutput, TCommand}"/> instance.
        /// </summary>
        /// <param name="handler">The handler that the <see cref="Pipeline{TCommandId, TData, TOutput, TCommand}"/> instance will be build around.</param>
        /// <param name="postHandlers">handlers to run after the execution of the <see cref="handler"/>.</param>
        /// <exception cref="ArgumentNullException">if <see cref="handler"/> or <see cref="_postHandlers"/> is <c>null</c>.</exception>
        public Pipeline(IRunCommandAsync<TCommandId, TData, TOutput, TCommand> handler, IEnumerable<IPostCommandHandler<TCommandId, TData, TOutput>> postHandlers)
        {
            _handler = handler;
            _postHandlers = postHandlers;
        }

        public async Task<TOutput> RunAsync(TCommand command)
        {
            TOutput output = await _handler.RunAsync(command);

            foreach (var postHandler in _postHandlers)
            {
                await postHandler.HandleAsync(command.Data, output);
            }

            return output;
        }
    }
}
