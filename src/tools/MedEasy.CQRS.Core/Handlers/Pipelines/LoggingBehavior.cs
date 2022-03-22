namespace MedEasy.CQRS.Core.Handlers.Pipelines
{
    using MediatR;

    using Microsoft.Extensions.Logging;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A pipeline behavior that logs command handlers inputs and outputs
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        ///<inheritdoc/>
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            string requestTypeName = typeof(TRequest).FullName;
            _logger.LogInformation("Executing {@CommandType}", requestTypeName);

            TResponse response = await next.Invoke()
                                           .ConfigureAwait(false);

            _logger.LogInformation("Executed {@CommandType}", requestTypeName);

            return response;
        }
    }
}
