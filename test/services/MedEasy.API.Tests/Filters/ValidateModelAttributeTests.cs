using FluentAssertions;
using MedEasy.API.Filters;
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
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.API.Tests.Filters
{
    public class ValidateModelAttributeTests : IDisposable
    {
        private Mock<ILogger<ValidateModelAttribute>> _loggerMock;
        private ITestOutputHelper _outputHelper;
        private ValidateModelAttribute _validateModelAttribute;

        public ValidateModelAttributeTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<ValidateModelAttribute>>(MockBehavior.Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));


            _validateModelAttribute = new ValidateModelAttribute(_loggerMock.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new ValidateModelAttribute(null);

            // Assert
            act.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        public void Dispose()
        {
            _outputHelper = null;
            _loggerMock = null;
            _validateModelAttribute = null;
        }

        [Fact]
        public void ShouldReturnBadRequesWhenModelStateIsNotValid()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");

            ActionContext actionContext = new ActionContext(
               new Mock<HttpContext>().Object,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               modelState);

            ActionExecutingContext actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new Mock<Controller>());



            // Act

            _validateModelAttribute.OnActionExecuting(actionExecutingContext);

            // Assert
            actionExecutingContext.Result.Should()
                .NotBeNull().And
                .BeAssignableTo<BadRequestObjectResult>().Which
                    .Value.Should()
                    .NotBeNull();
        }
    }
}
