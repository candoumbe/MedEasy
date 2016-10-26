using FluentAssertions;
using MedEasy.API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Collections.Generic;
using Xunit;
using static Moq.MockBehavior;

namespace MedEasy.API.Tests.Filters
{
    public class ValidateModelAttributeTests
    {
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

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new Mock<Controller>());


            
            // Act
            ValidateModelAttribute attribute = new ValidateModelAttribute();
            attribute.OnActionExecuting(actionExecutingContext);

            // Assert
            actionExecutingContext.Result.Should()
                .NotBeNull().And
                .BeAssignableTo<BadRequestObjectResult>();
        }
    }
}
