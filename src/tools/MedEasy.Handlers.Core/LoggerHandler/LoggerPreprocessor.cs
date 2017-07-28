using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MedEasy.CQRS.Core;

namespace MedEasy.Handlers.Core.LoggerHandler
{
    /// <summary>
    /// Logger preprocessor that can be used in a request pipeline.
    /// </summary>
    /// <typeparam name="TRequest">Type of the input</typeparam>
    public class LoggerPreprocessor<TRequest, TResponse> : IPreprocess<TRequest>
        where TRequest : IRequest
    {

        private ILogger<LoggerPreprocessor<TRequest, TResponse>> Logger { get; }

        /// <summary>
        /// Builds a new <see cref="LoggerPreprocessor{TRequest, TResponse}"/> instance
        /// </summary>
        /// <param name="logger">logger</param>
        public LoggerPreprocessor(ILogger<LoggerPreprocessor<TRequest, TResponse>> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logged the request that's been processed
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task Preprocess(TRequest input)
        {
            Logger.LogInformation($"Request <{input.Id}>");

            return Task.CompletedTask;
        }
    }
}
