using FluentAssertions;
using MedEasy.API.Filters;
using MedEasy.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using static MedEasy.Validators.ErrorLevel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static Moq.MockBehavior;
using Xunit;
using MedEasy.Handlers.Core.Exceptions;

namespace MedEasy.API.Tests.Filters
{
    public class HandleErrorAttributeTests : IDisposable
    {
        private HandleErrorAttribute _handleErrorAttribute;
        private Mock<ILogger<HandleErrorAttribute>> _loggerMock;

        public HandleErrorAttributeTests()
        {
            _loggerMock = new Mock<ILogger<HandleErrorAttribute>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

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
            ActionContext actionContext = new ActionContext(
               new Mock<HttpContext>().Object,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            // Act
            IEnumerable<ErrorInfo> exceptionErrors = new[] {
                new ErrorInfo("prop1", "error 1", Error),
                new ErrorInfo("prop2", "warning 2", Warning)
            };
            ExceptionContext exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new CommandNotValidException<Guid>(Guid.NewGuid(), exceptionErrors)
            };
            await _handleErrorAttribute.OnExceptionAsync(exceptionContext);


            // Assert
            exceptionContext.ExceptionHandled.Should().BeTrue();
            exceptionContext.Result.Should()
                .BeAssignableTo<BadRequestObjectResult>();

           
            exceptionContext.ModelState?.Should()
                .NotBeNull().And
                .Contain(x => x.Key == "prop1").And
                .Contain(x => x.Key == "prop2");
        }

        [Fact]
        public async Task ShouldReturnsNotFoundResultWhenHandlingNotFoundException()
        {
            // Arrange
            ActionContext actionContext = new ActionContext(
               new Mock<HttpContext>().Object,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            // Act
            IEnumerable<ErrorInfo> exceptionErrors = new[] {
                new ErrorInfo("prop1", "error 1", Error),
                new ErrorInfo("prop2", "warning 2", Warning)
            };
            ExceptionContext exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>());
            exceptionContext.Exception = new NotFoundException("exception message");
            await _handleErrorAttribute.OnExceptionAsync(exceptionContext);


            // Assert
            exceptionContext.ExceptionHandled.Should().BeTrue();
            exceptionContext.Result.Should()
                .BeOfType<NotFoundObjectResult>().Which
                    .Value.Should()
                        .NotBeNull();
        }



        [Fact]
        public async Task ShouldReturnsBadRequestWhenHandlingQueryNotValidException()
        {
            // Arrange
            ActionContext actionContext = new ActionContext(
               new Mock<HttpContext>().Object,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            // Act
            IEnumerable<ErrorInfo> exceptionErrors = new[] {
                new ErrorInfo("prop1", "error 1", Error),
                new ErrorInfo("prop2", "warning 2", Warning)
            };
            ExceptionContext exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>());
            exceptionContext.Exception = new QueryNotValidException<Guid>(Guid.NewGuid(), exceptionErrors);
            await _handleErrorAttribute.OnExceptionAsync(exceptionContext);


            // Assert
            exceptionContext.ExceptionHandled.Should().BeTrue();
            exceptionContext.Result.Should()
                .BeAssignableTo<BadRequestObjectResult>();


            exceptionContext.ModelState.Should()
                .NotBeNull().And
                .Contain(x => x.Key == "prop1").And
                .Contain(x => x.Key == "prop2");
        }

        [Fact]
        public async Task ShouldReturnsConflictWhenHandlingCommandConflictException()
        {
            // Arrange
            ActionContext actionContext = new ActionContext(
               new Mock<HttpContext>().Object,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            // Act

            ExceptionContext exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new CommandConflictException<Guid>(Guid.NewGuid())
            };
            await _handleErrorAttribute.OnExceptionAsync(exceptionContext);


            // Assert
            exceptionContext.ExceptionHandled.Should().BeTrue();
            exceptionContext.Result.Should()
                .BeAssignableTo<StatusCodeResult>().Which
                .StatusCode.Should()
                    .Be(StatusCodes.Status409Conflict);


            exceptionContext.ModelState.Should().BeNullOrEmpty();
        }




    }
}
