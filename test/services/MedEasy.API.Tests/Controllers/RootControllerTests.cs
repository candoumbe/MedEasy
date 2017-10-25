using MedEasy.API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Moq;
using static Moq.MockBehavior;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using MedEasy.RestObjects;
using MedEasy.DTO;
using MedEasy.DTO.Search;
using Microsoft.Extensions.Options;

namespace MedEasy.API.Tests.Controllers
{
    public class RootControllerTests : IDisposable
    {
        private ActionContextAccessor _actionContextAccessor;
        private RootController _controller;
        private Mock<IHostingEnvironment> _hostingEnvironmentMock;
        private Mock<IOptions<MedEasyApiOptions>> _optionsMock;
        private Mock<IUrlHelper> _urlHelperMock;
        private const string _baseUrl = "http://host/api";

        public RootControllerTests()
        {
            _hostingEnvironmentMock = new Mock<IHostingEnvironment>(Strict);
            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string routename, object routeValues) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            _actionContextAccessor = new ActionContextAccessor()
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            _optionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);
            _optionsMock.Setup(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = PaginationConfiguration.DefaultPageSize, MaxPageSize = PaginationConfiguration.MaxPageSize });

            _controller = new RootController(_hostingEnvironmentMock.Object, _urlHelperMock.Object, _actionContextAccessor, _optionsMock.Object);
        }

        public void Dispose()
        {
            _hostingEnvironmentMock = null;
            _urlHelperMock = null;
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
                .Contain(x => x.Name == PatientsController.EndpointName).And
                .Contain(x => x.Name == DoctorsController.EndpointName).And
                .Contain(x => x.Name == SpecialtiesController.EndpointName).And
                .Contain(x => x.Name == PrescriptionsController.EndpointName).And
                .Contain(x => x.Name == DocumentsController.EndpointName).And
                .Contain(x => x.Name == AppointmentsController.EndpointName);

            // Patients endpoint
            Endpoint patientsEndpoint = endpoints.Single(x => x.Name == PatientsController.EndpointName);
            patientsEndpoint.Link.Should()
                .NotBeNull();
            patientsEndpoint.Link.Relation.Should().Be(LinkRelation.Collection);
            patientsEndpoint.Link.Href.Should().NotBeNullOrWhiteSpace();

            patientsEndpoint.Forms.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .NotContain(x => x.Meta == null).And
                .Contain(x => LinkRelation.CreateForm == x.Meta.Relation).And
                .Contain(x => LinkRelation.Search == x.Meta.Relation);

            Form patientsCreateForm = patientsEndpoint.Forms.Single(x => LinkRelation.CreateForm == x.Meta.Relation);
            patientsCreateForm.Meta.Method.Should().Be("POST");
            patientsCreateForm.Meta.Href.Should().NotBeNullOrWhiteSpace();
            patientsCreateForm.Items.Should()
                .NotContainNulls().And
                .Contain(x => x.Name == nameof(CreatePatientInfo.Firstname)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.Lastname)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.BirthDate)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.BirthPlace)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.MainDoctorId));

            Form patientsSearchForm = patientsEndpoint.Forms.Single(x => LinkRelation.Search == x.Meta.Relation);
            patientsSearchForm.Meta.Method.Should().BeNull();
            patientsSearchForm.Meta.Href.Should().NotBeNullOrWhiteSpace();
            patientsSearchForm.Items.Should()
                .NotContainNulls().And
                .Contain(x => x.Name == nameof(SearchPatientInfo.Firstname)).And
                .Contain(x => x.Name == nameof(SearchPatientInfo.Lastname)).And
                .Contain(x => x.Name == nameof(SearchPatientInfo.BirthDate)).And
                .Contain(x => x.Name == nameof(SearchPatientInfo.Sort)).And
                .Contain(x => x.Name == nameof(SearchPatientInfo.Page)).And
                .Contain(x => x.Name == nameof(SearchPatientInfo.PageSize));

            // Doctor resource endpoint
            Endpoint doctorsEndpoint = endpoints.Single(x => x.Name == DoctorsController.EndpointName);
            doctorsEndpoint.Forms.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .NotContain(x => x.Meta == null).And
                .Contain(x => LinkRelation.CreateForm == x.Meta.Relation).And
                .Contain(x => LinkRelation.Search == x.Meta.Relation);


            Form doctorCreateForm = doctorsEndpoint.Forms.Single(x => LinkRelation.CreateForm == x.Meta.Relation);
            doctorCreateForm.Meta.Method.Should()
                .Be("POST");
            doctorCreateForm.Meta.Href.Should().NotBeNullOrWhiteSpace();
            doctorCreateForm.Items.Should()
                .NotContainNulls().And
                .Contain(x => x.Name == nameof(CreateDoctorInfo.Firstname)).And
                .Contain(x => x.Name == nameof(CreateDoctorInfo.Lastname)).And
                .Contain(x => x.Name == nameof(CreateDoctorInfo.SpecialtyId));

            Form doctorsSearchForm = doctorsEndpoint.Forms.Single(x => LinkRelation.Search == x.Meta.Relation);
            doctorsSearchForm.Meta.Method.Should()
                .Be("GET");
            doctorsSearchForm.Meta.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={DoctorsController.EndpointName}&firstname={Uri.EscapeDataString("{firstname}")}&lastname={Uri.EscapeDataString("{lastname}")}&specialtyId={Uri.EscapeDataString("{specialtyId}")}");
            doctorsSearchForm.Meta.Template.Should()
                .BeTrue("The search URL must be flagged as a template url");
            doctorsSearchForm.Items.Should()
                .NotContainNulls().And
                .Contain(x => x.Name == nameof(SearchDoctorInfo.Firstname)).And
                .Contain(x => x.Name == nameof(SearchDoctorInfo.Lastname)).And
                .Contain(x => x.Name == nameof(SearchDoctorInfo.Sort)).And
                .Contain(x => x.Name == nameof(SearchDoctorInfo.Page)).And
                .Contain(x => x.Name == nameof(SearchDoctorInfo.PageSize));

            if (environmentName != "Production")
            {
                endpoints.Should()
                    .Contain(x => x.Name == "documentation");

                Endpoint endpointProduction = endpoints.Single(x => x.Name == "documentation");
            }

            
        }
    }
}
