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
    [Feature("Timing behavior")]
    public class TimingBehaviorTests
    {
        [Fact]
        public void Should_be_a_PipelineBehavior()
        {
            Type sut = typeof(TimingBehavior<,>);

            // Assert
            sut.Should()
               .NotHaveDefaultConstructor().And
               .NotBeAbstract();
        }

        [Fact]
        public async Task Given_a_handler_that_does_not_throw_TimingBehavior_should_log_debug()
        {
            // Arrange
            IRequest request = Mock.Of<IRequest>();
            IRequestHandler<IRequest> handler = Mock.Of<IRequestHandler<IRequest>>();
            Mock<ILogger<TimingBehavior<IRequest, Unit >>> loggerMock = new(Strict);

            loggerMock.Setup(mock => mock.Log(IsAny<LogLevel>(),
                                              IsAny<EventId>(),
                                              IsAny<IsAnyType>(),
                                              IsAny<Exception>(),
                                              (Func<IsAnyType, Exception, string>)IsAny<object>()));

            TimingBehavior<IRequest, Unit> behavior = new(loggerMock.Object);

            // Act
            await behavior.Handle(request, default, () => handler.Handle(request, default))
                          .ConfigureAwait(false);

            // Assert
            loggerMock.Verify(mock => mock.Log(Is<LogLevel>(level => level == LogLevel.Debug),
                                              IsAny<EventId>(),
                                              IsAny<IsAnyType>(),
                                              Is<Exception>(ex => ex == null),
                                              (Func<IsAnyType, Exception, string>)IsAny<object>()), Times.Exactly(1));
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Given_a_handler_that_throw_an_exception_TimingBehavior_should__not()
        {
            // Arrange
            IRequest request = Mock.Of<IRequest>();
            IRequestHandler<IRequest> handler = Mock.Of<IRequestHandler<IRequest>>();
            Mock<ILogger<TimingBehavior<IRequest, Unit>>> loggerMock = new(Strict);

            loggerMock.Setup(mock => mock.Log(IsAny<LogLevel>(),
                                              IsAny<EventId>(),
                                              IsAny<IsAnyType>(),
                                              IsAny<Exception>(),
                                              (Func<IsAnyType, Exception, string>)IsAny<object>()));

            TimingBehavior<IRequest, Unit> behavior = new(loggerMock.Object);

            // Act
            Func<Task> execution =async () =>  await behavior.Handle(request, default, () => throw new NotSupportedException())
                                                             .ConfigureAwait(false);

            // Assert
            execution.Should()
                     .Throw<Exception>();
            loggerMock.Verify(mock => mock.Log(IsAny<LogLevel>(),
                                               IsAny<EventId>(),
                                               IsAny<IsAnyType>(),
                                               IsAny<Exception>(),
                                               (Func<IsAnyType, Exception, string>)IsAny<object>()), Times.Never);
            loggerMock.VerifyNoOtherCalls();
        }
    }
}
