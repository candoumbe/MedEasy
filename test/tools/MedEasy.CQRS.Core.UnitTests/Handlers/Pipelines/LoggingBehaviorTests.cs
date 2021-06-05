namespace MedEasy.CQRS.Core.UnitTests.Handlers.Pipelines
{
    using FluentAssertions;

    using MedEasy.CQRS.Core.Handlers.Pipelines;

    using MediatR;

    using Microsoft.Extensions.Logging;

    using Moq;

    using System;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Categories;

    using static Moq.MockBehavior;
    using static Moq.It;

    [UnitTest]
    [Feature("Logging behavior")]
    public class LoggingBehaviorTests
    {
        [Fact]
        public void Should_be_a_PipelineBehavior()
        {
            Type sut = typeof(LoggingBehavior<,>);

            // Assert
            sut.Should()
               .NotHaveDefaultConstructor().And
               .NotBeAbstract();
        }

        [Fact]
        public async Task Given_a_handler_that_does_not_throw_LoggingBehavior_should_log_information_before_and_after_the_handler_completes()
        {
            // Arrange
            IRequest request = Mock.Of<IRequest>();
            IRequestHandler<IRequest> handler = Mock.Of<IRequestHandler<IRequest>>();
            Mock<ILogger<LoggingBehavior<IRequest, Unit >>> loggerMock = new(Strict);

            loggerMock.Setup(mock => mock.Log(IsAny<LogLevel>(),
                                              IsAny<EventId>(),
                                              IsAny<IsAnyType>(),
                                              IsAny<Exception>(),
                                              (Func<IsAnyType, Exception, string>)IsAny<object>()));

            LoggingBehavior<IRequest, Unit> behavior = new(loggerMock.Object);

            // Act
            await behavior.Handle(request, default, () => handler.Handle(request, default))
                          .ConfigureAwait(false);

            // Assert
            loggerMock.Verify(mock => mock.Log(Is<LogLevel>(level => level == LogLevel.Information),
                                              IsAny<EventId>(),
                                              IsAny<IsAnyType>(),
                                              Is<Exception>(ex => ex == null),
                                              (Func<IsAnyType, Exception, string>)IsAny<object>()), Times.Exactly(2));
            loggerMock.VerifyNoOtherCalls();
        }
    }
}
