namespace MedEasy.CQRS.Core.Handlers.Pipelines
{
    using MediatR;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A pipeline behavior that logs command handlers inputs and outputs
    /// </summary>
    public class TimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<TimingBehavior<TRequest, TResponse>> _logger;
        private readonly Stopwatch _watch;

        public TimingBehavior(ILogger<TimingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
            _watch = new Stopwatch();
        }

        ///<inheritdoc/>
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            string requestTypeName = typeof(TRequest).FullName;
            _watch.Restart();
            TResponse response = await next.Invoke()
                                           .ConfigureAwait(false);
            _watch.Stop();
            _logger.LogDebug("Running {@CommandType} took {ExecutionTime}", requestTypeName, _watch.Elapsed);

            return response;
        }
    }
}
