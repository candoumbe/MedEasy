namespace MedEasy.Core.Filters
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    using static Moq.MockBehavior;
    using static MedEasy.CQRS.Core.Exceptions.ErrorLevel;
    using MedEasy.CQRS.Core.Exceptions;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Filters")]
    public class HandleErrorAttributeTests : IDisposable
    {
        private HandleErrorAttribute _handleErrorAttribute;
        private Mock<ILogger<HandleErrorAttribute>> _loggerMock;

        public HandleErrorAttributeTests()
        {
            _loggerMock = new Mock<ILogger<HandleErrorAttribute>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(),
                                               It.IsAny<EventId>(),
                                               It.Is<It.IsAnyType>((_, __) => true),
                                               It.IsAny<Exception>(),
                                               (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

            _handleErrorAttribute = new HandleErrorAttribute(_loggerMock.Object);
        }

        public void Dispose()
        {
            _loggerMock = null;
            _handleErrorAttribute = null;
        }

        [Fact]
        public async Task ShouldReturnsBadRequestWhenHandlingCommandNotValidException()
        {
            // Arrange
            ActionContext actionContext = new(
               new Mock<HttpContext>().Object,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            // Act
            IEnumerable<ErrorInfo> exceptionErrors = new[] {
                new ErrorInfo("prop1", "error 1", Error),
                new ErrorInfo("prop2", "warning 2", Warning)
            };
            ExceptionContext exceptionContext = new(actionContext, new List<IFilterMetadata>())
            {
                Exception = new CommandNotValidException<string>("zero", exceptionErrors)
            };
            await _handleErrorAttribute.OnExceptionAsync(exceptionContext)
                .ConfigureAwait(false);

            // Assert
            exceptionContext.ExceptionHandled.Should().BeTrue();
            exceptionContext.Result.Should()
                .BeAssignableTo<BadRequestResult>();

            exceptionContext.ModelState.Should()
                .NotBeNull().And
                .Contain(x => x.Key == "prop1").And
                .Contain(x => x.Key == "prop2");
        }

        [Fact]
        public async Task ShouldReturnsBadRequestWhenHandlingQueryNotValidException()
        {
            // Arrange
            ActionContext actionContext = new(
               new Mock<HttpContext>().Object,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            // Act
            IEnumerable<ErrorInfo> exceptionErrors = new[] {
                new ErrorInfo("prop1", "error 1", Error),
                new ErrorInfo("prop2", "warning 2", Warning)
            };
            ExceptionContext exceptionContext = new(actionContext, new List<IFilterMetadata>())
            {
                Exception = new QueryNotValidException<Guid>(Guid.NewGuid(), exceptionErrors)
            };
            await _handleErrorAttribute.OnExceptionAsync(exceptionContext)
                .ConfigureAwait(false);

            // Assert
            exceptionContext.ExceptionHandled.Should().BeTrue();
            exceptionContext.Result.Should()
                .BeAssignableTo<BadRequestResult>();

            exceptionContext.ModelState.Should()
                .NotBeNull().And
                .Contain(x => x.Key == "prop1").And
                .Contain(x => x.Key == "prop2");
        }
    }
}
