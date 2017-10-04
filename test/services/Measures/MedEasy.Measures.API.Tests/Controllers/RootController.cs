using MedEasy.Measures.API.Controllers;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using static Moq.MockBehavior;

namespace MedEasy.Measures.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="RootController"/>
    /// </summary>
    public class RootControllerTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IHostingEnvironment> _hostingEnvironmentMock;
        private Mock<IUrlHelperFactory> _urlHelperFactoryMock;
        private ActionContextAccessor _actionContextAccessor;
        private Mock<IOptions<PrescriptionApiOptions>> _optionsMock;
        private RootController _controller;

        public RootControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _hostingEnvironmentMock = new Mock<IHostingEnvironment>(Strict);
            _urlHelperFactoryMock = new Mock<IUrlHelperFactory>(Strict);
            _urlHelperFactoryMock.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>()).Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string routeName, object routeValues) => $"api/?{(routeValues == null ? string.Empty : $"{routeValues?.ToQueryString()}")}");

            _actionContextAccessor = new ActionContextAccessor()
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            _optionsMock = new Mock<IOptions<PrescriptionApiOptions>>(Strict);
            _optionsMock.Setup(mock => mock.Value).Returns(new PrescriptionApiOptions { DefaultPageSize = PaginationConfiguration.DefaultPageSize, MaxPageSize = PaginationConfiguration.MaxPageSize });

            _controller = new RootController(_hostingEnvironmentMock.Object, _urlHelperFactoryMock.Object, _actionContextAccessor, _optionsMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _hostingEnvironmentMock = null;
            _urlHelperFactoryMock = null;
            _actionContextAccessor = null;
            _controller = null;
            _optionsMock = null;
        }


        [Theory]
        [InlineData("Development")]
        public void Endpoints(string environmentName)
        {
            // Arrange
            _hostingEnvironmentMock.Setup(mock => mock.EnvironmentName)
                .Returns(environmentName);


            // Act
            IEnumerable<Endpoint> endpoints = _controller.Index();


            // Assert
            endpoints.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Name)).And
                .BeInAscendingOrder(x => x.Name).And
                .Contain(x => x.Name == BloodPressuresController.EndpointName).And
                .Contain(x => x.Name == BodyWeightsControllers.EndpointName);
                
            // Patients endpoint
            Endpoint patients = endpoints.Single(x => x.Name == PatientsController.EndpointName);
            patients.Link.Should()
                .NotBeNull();
            patients.Link.Relation.Should().Be("collection");
            patients.Link.Href.Should().NotBeNullOrWhiteSpace();

            patients.Forms.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .NotContain(x => x.Meta == null).And
                .Contain(x => "create-form" == x.Meta.Relation).And
                .Contain(x => "search" == x.Meta.Relation);

            Form patientCreateForm = patients.Forms.Single(x => "create-form" == x.Meta.Relation);
            patientCreateForm.Meta.Method.Should().Be("POST");
            patientCreateForm.Meta.Href.Should().NotBeNullOrWhiteSpace();
            patientCreateForm.Items.Should()
                .NotContainNulls().And
                .Contain(x => x.Name == nameof(CreatePatientInfo.Firstname)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.Lastname)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.BirthDate)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.BirthPlace)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.MainDoctorId));

            Form patientSearchForm = patients.Forms.Single(x => "search" == x.Meta.Relation);
            patientSearchForm.Meta.Method.Should().BeNull();
            patientSearchForm.Meta.Href.Should().NotBeNullOrWhiteSpace();
            patientSearchForm.Items.Should()
                .NotContainNulls().And
                .Contain(x => x.Name == nameof(SearchPatientInfo.Firstname)).And
                .Contain(x => x.Name == nameof(SearchPatientInfo.Lastname)).And
                .Contain(x => x.Name == nameof(SearchPatientInfo.Page)).And
                .Contain(x => x.Name == nameof(SearchPatientInfo.PageSize));


            if (environmentName != "Production")
            {
                endpoints.Should()
                    .Contain(x => x.Name == "documentation");
            }


        }
    }
}
