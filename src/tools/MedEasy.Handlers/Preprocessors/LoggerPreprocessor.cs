using MedEasy.CQRS.Core;
using MedEasy.Handlers.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.Handlers.Preprocessors
{
    public class LoggerPreprocessor<TRequest, TResponse> : IPreprocess<TRequest>
        where TRequest : IRequest<Guid, TResponse>
    {

        private ILogger<LoggerPreprocessor<TRequest, TResponse>> Logger { get; }

        /// <summary>
        /// Builds a new <see cref="LoggerPreprocessor{TRequest, TResponse}"/> instance.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <exception cref="ArgumentNullException">if <paramref name="logger"/> is <c>null</c>.</exception>
        public LoggerPreprocessor(ILogger<LoggerPreprocessor<TRequest, TResponse>> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Preprocess(TRequest input)
        {
            Logger.LogInformation($"Request <{input.Id}> started");
            Logger.LogTrace($"Content : {input}");
            return Task.CompletedTask;
        }
    }
}
