namespace MedEasy.Core.UnitTests.Attributes
{
    using FluentAssertions;

    using MedEasy.Core.Filters;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Routing;

    using Moq;

    using System;
    using System.Collections.Generic;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static Moq.MockBehavior;

    [UnitTest]
    [Category("Filters")]
    public class ValidateModelFilterTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private ValidateModelActionFilterAttribute _sut;
        private Mock<HttpContext> _httpContextMock;
        private Mock<ControllerActionDescriptor> _controllerActionDescriptorMock;

        public ValidateModelFilterTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _sut = new ValidateModelActionFilterAttribute();
            _httpContextMock = new Mock<HttpContext>(Strict);
            _controllerActionDescriptorMock = new Mock<ControllerActionDescriptor>(Strict);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _sut = null;
            _httpContextMock = null;
            _controllerActionDescriptorMock = null;
        }

        [Fact]
        public void ShouldReturnBadRequesWhenModelStateIsNotValid()
        {
            // Arrange
            ModelStateDictionary modelState = new();
            modelState.AddModelError("name", "invalid");

            _httpContextMock.SetupGet(mock => mock.Request.Method).Returns(HttpMethods.Get);

            ActionContext actionContext = new(
               _httpContextMock.Object,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               modelState);

            ActionExecutingContext actionExecutingContext = new(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new Mock<Controller>());

            // Act
            _sut.OnActionExecuting(actionExecutingContext);

            // Assert
            BadRequestObjectResult badRequest = actionExecutingContext.Result.Should()
                .NotBeNull().And
                .BeAssignableTo<BadRequestObjectResult>().Which;

            badRequest.StatusCode.Should()
                .Be(Status400BadRequest);

            ValidationProblemDetails errorOBject = badRequest.Value.Should()
                .BeOfType<ValidationProblemDetails>().Which;

            errorOBject.Title.Should()
                .Be("Validation failed");
            errorOBject.Errors.Should()
                .NotBeNull().And
                .ContainKey("name").WhichValue.Should()
                    .HaveCount(1).And
                    .HaveElementAt(0, "invalid");
        }

        //public static IEnumerable<object[]> EvaluateActionParametersCases
        //{
        //    get
        //    {
        //        yield return new object[]
        //        {
        //            Enumerable.Empty<(object value, ParameterInfo)>(),
        //            ((Expression<Func<IActionResult, bool>>)(actionResult => actionResult == null)),
        //            "Action with no parameter is always valid",
        //        };

        //        {
        //            Mock<ParameterInfo> param1 = new Mock<ParameterInfo>(Strict);
        //            param1.SetupGet(mock => mock.Name).Returns("id");
        //            param1.SetupGet(mock => mock.ParameterType).Returns(typeof(Guid));
        //            param1.Setup(mock => mock.GetCustomAttributes(true)).Returns(new Attribute[] { new RequireNonDefaultAttribute() });

        //            yield return new object[]
        //            {
        //                new (object value, ParameterInfo param)[]
        //                {
        //                    (Guid.Empty, param1.Object)
        //                },
        //                ((Expression<Func<IActionResult, bool>>)(actionResult => actionResult != null 
        //                    && actionResult is BadRequestObjectResult
        //                    && ((BadRequestObjectResult)actionResult).StatusCode == Status400BadRequest 
        //                    && ((BadRequestObjectResult)actionResult).Value is ErrorObject
        //                    && "BAD_REQUEST".Equals(((ErrorObject)((BadRequestObjectResult)actionResult).Value).Code)
        //                    && "Validation failed".Equals(((ErrorObject)((BadRequestObjectResult)actionResult).Value).Description)
        //                    && ((ErrorObject)((BadRequestObjectResult)actionResult).Value).Errors.Once()
        //                    && ((ErrorObject)((BadRequestObjectResult)actionResult).Value).Errors.Once(x => x.Key == param1.Object.Name)
        //                )),

        //            };
        //        }
        //    }
        //}

        //[Theory]
        //[Trait("Category", "Unit test")]
        //[MemberData(nameof(EvaluateActionParametersCases))]
        //public void EvaluateActionParameters(IEnumerable<(object value, ParameterInfo parameterInfo)> parameters, Expression<Func<IActionResult, bool>> actionResultExpectation, string reason)
        //{
        //    // Arrange
        //    ModelStateDictionary modelState = new ModelStateDictionary();

        //    _httpContextMock.SetupGet(mock => mock.Request.Method).Returns(HttpMethods.Get);

        //    _controllerActionDescriptorMock.Setup(mock => mock.MethodInfo.GetParameters())
        //        .Returns(parameters.Select(x => x.parameterInfo).ToArray());

        //    ActionContext actionContext = new ActionContext(
        //       _httpContextMock.Object,
        //       new Mock<RouteData>().Object,
        //       _controllerActionDescriptorMock.Object,
        //       modelState);

        //    ActionExecutingContext actionExecutingContext = new ActionExecutingContext(
        //        actionContext,
        //        new List<IFilterMetadata>(),
        //        new Dictionary<string, object>(),
        //        new Mock<Controller>());

        //    // Act
        //    _sut.OnActionExecuting(actionExecutingContext);

        //    // Assert
        //    actionExecutingContext.Result.Should()
        //        .Match(actionResultExpectation, reason);
        //}
    }
}
