using System;
using Xunit;
using MedEasy.CQRS.Core;
using MedEasy.Handlers.Core.LoggerHandler;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using FluentAssertions;
using static Moq.MockBehavior;

namespace MedEasy.Handlers.Preprocessors.Tests
{
    /// <summary>
    /// unit tests for <see cref="LoggerPreprocesor{TRequest, TResponse}"/> class.
    /// </summary>
    public class LoggerPreprocesorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        /// <summary>
        /// For unit tests only
        /// </summary>
        public class DummyRequest : IRequest
        {

            public DummyRequest()
            {
                Id = Guid.NewGuid();
            }

            public Guid Id { get; }
        }

        /// <summary>
        /// Builds a new <see cref="LoggerPreprocesorTests"/> instance
        /// </summary>
        /// <param name="outputHelper"></param>
        public LoggerPreprocesorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }


        public void Dispose()
        {
            _outputHelper = null;
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new LoggerPreprocessor<DummyRequest, Nothing>(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrEmpty();
        }


        [Fact]
        public async Task Preprocess_Calls_Logger()
        {
            // Arrange
            Mock<ILogger<LoggerPreprocessor<DummyRequest, Nothing>>> loggerMock = new Mock<ILogger<LoggerPreprocessor<DummyRequest, Nothing>>>(Loose);
            loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();

            LoggerPreprocessor<DummyRequest, Nothing> loggerPreprocessor = new LoggerPreprocessor<DummyRequest, Nothing>(loggerMock.Object);

            // Act
            DummyRequest request = new DummyRequest();
            await loggerPreprocessor.Preprocess(request);

            // Assert
            loggerMock.Verify();
        }
    }
}
