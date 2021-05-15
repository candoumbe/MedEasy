namespace Patients.API.UnitTests.Controllers
{
    using FluentAssertions;
    using Patients.API.Controllers;
    using Patients.API.Routing;
    using Patients.DTO;
    using MedEasy.RestObjects;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;
    using static Moq.MockBehavior;
    using static MedEasy.RestObjects.FormFieldType;
    using Xunit.Categories;
    using Endpoint = MedEasy.RestObjects.Endpoint;
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Routing;
    using MedEasy.Ids;

    /// <summary>
    /// Unit tests for <see cref="RootController"/>
    /// </summary>
    [UnitTest]
    [Feature("Documentation")]
    public class RootControllerTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private Mock<IHostEnvironment> _hostingEnvironmentMock;
        private Mock<LinkGenerator> _urlHelperMock;
        private Mock<IOptions<PatientsApiOptions>> _optionsMock;
        private RootController _sut;
        private const string _baseUrl = "http://host/api";
        private readonly ApiVersion _apiVersion;

        public RootControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _hostingEnvironmentMock = new Mock<IHostEnvironment>(Strict);
            _urlHelperMock = new Mock<LinkGenerator>(Strict);
            _urlHelperMock.Setup(mock => mock.GetPathByAddress(It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<PathString>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
                .Returns((string routename, RouteValueDictionary routeValues, PathString _, FragmentString __, LinkOptions ___)
                => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString((string ____, object value) => (value as StronglyTypedId<Guid>)?.Value ?? value)}");

            _optionsMock = new Mock<IOptions<PatientsApiOptions>>(Strict);
            _optionsMock.Setup(mock => mock.Value).Returns(new PatientsApiOptions { DefaultPageSize = PaginationConfiguration.DefaultPageSize, MaxPageSize = PaginationConfiguration.MaxPageSize });

            _apiVersion = new ApiVersion(1, 0);

            _sut = new RootController(_hostingEnvironmentMock.Object, _urlHelperMock.Object, _optionsMock.Object, _apiVersion);
        }

        public void Dispose()
        {
            _hostingEnvironmentMock = null;
            _urlHelperMock = null;
            _sut = null;
            _optionsMock = null;
        }

        [Theory]
        [InlineData("Production")]
        [InlineData("Development")]
        public void Endpoints(string environmentName)
        {
            _outputHelper.WriteLine($"Environment name : '{environmentName}'");

            // Arrange
            _hostingEnvironmentMock.Setup(mock => mock.EnvironmentName)
                                   .Returns(environmentName);

            // Act
            IEnumerable<Endpoint> endpoints = _sut.Index();

            // Assert
            endpoints.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Name)).And
                .NotContain(x => x.Link == null, $"{nameof(Endpoint)}'s {nameof(Endpoint.Link)} must be provided").And
                .BeInAscendingOrder(x => x.Name).And
                .Contain(x => x.Name == PatientsController.EndpointName.Slugify());

            #region Patients endpoint
            // Patients endpoint
            Endpoint patientsEndpoint = endpoints.Single(x => x.Name == PatientsController.EndpointName.Slugify());
            patientsEndpoint.Link.Should()
                .NotBeNull($"{nameof(Endpoint)}.{nameof(Endpoint.Link)} must be provided");
            patientsEndpoint.Link.Relation.Should()
                .Be(LinkRelation.Collection);
            patientsEndpoint.Link.Method.Should()
                .Be("GET");
            patientsEndpoint.Link.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName.Slugify()}&page=1&pageSize=30&version={_apiVersion}");
            patientsEndpoint.Link.Title.Should()
                .Be("Collection of patients");

            patientsEndpoint.Forms.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Meta == null, "Forms cannot contain any form with null metadata").And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Meta.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Meta.Href)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Meta.Method)).And
                .ContainSingle(x => x.Meta.Relation == LinkRelation.Search).And
                .ContainSingle(x => x.Meta.Relation == LinkRelation.CreateForm)
                ;

            #region Search form
            Form patientsSearchForm = patientsEndpoint.Forms.Single(x => x.Meta.Relation == LinkRelation.Search);
            patientsSearchForm.Meta.Should()
                .NotBeNull();
            patientsSearchForm.Meta.Method.Should()
                .Be("GET");
            patientsSearchForm.Meta.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={PatientsController.EndpointName}&page=1&pageSize=30&version={_apiVersion}");

            patientsSearchForm.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .OnlyContain(x => !string.IsNullOrWhiteSpace(x.Name)).And
                .ContainSingle(x => x.Name == nameof(SearchPatientInfo.Firstname)).And
                .ContainSingle(x => x.Name == nameof(SearchPatientInfo.Lastname)).And
                .ContainSingle(x => x.Name == nameof(SearchPatientInfo.BirthDate)).And
                .ContainSingle(x => x.Name == nameof(SearchPatientInfo.Page)).And
                .ContainSingle(x => x.Name == nameof(SearchPatientInfo.PageSize));

            FormField patientFirstnameField = patientsSearchForm.Items.Single(x => x.Name == nameof(SearchPatientInfo.Firstname));
            patientFirstnameField.Type.Should().Be(FormFieldType.String);

            FormField patientLastnameField = patientsSearchForm.Items.Single(x => x.Name == nameof(SearchPatientInfo.Lastname));
            patientLastnameField.Type.Should().Be(FormFieldType.String);

            FormField patientBirthDateField = patientsSearchForm.Items.Single(x => x.Name == nameof(SearchPatientInfo.BirthDate));
            patientBirthDateField.Type.Should().Be(Date);

            FormField patientPageField = patientsSearchForm.Items.Single(x => x.Name == nameof(SearchPatientInfo.Page));
            patientPageField.Type.Should().Be(Integer);
            patientPageField.Min.Should().Be(1);
            patientPageField.Max.Should().BeNull();

            FormField patientPageSizeField = patientsSearchForm.Items.Single(x => x.Name == nameof(SearchPatientInfo.PageSize));
            patientPageSizeField.Type.Should().Be(Integer);
            patientPageSizeField.Min.Should().Be(1);
            patientPageSizeField.Max.Should().Be(_optionsMock.Object.Value.MaxPageSize);

            FormField patientSortField = patientsSearchForm.Items.Single(x => x.Name == nameof(SearchPatientInfo.Sort));
            patientSortField.Type.Should().Be(FormFieldType.String);
            patientSortField.Pattern.Should().Be(SearchPatientInfo.SortPattern);

            #endregion 
            #region Create form
            Form patientsCreateForm = patientsEndpoint.Forms.Single(x => x.Meta.Relation == LinkRelation.CreateForm);
            patientsCreateForm.Meta.Should()
                .NotBeNull();
            patientsCreateForm.Meta.Method.Should()
                .Be("POST");
            patientsCreateForm.Meta.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName}&version={_apiVersion}");

            patientsCreateForm.Items.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .OnlyContain(x => !string.IsNullOrWhiteSpace(x.Name)).And
                .ContainSingle(x => x.Name == nameof(PatientInfo.Firstname)).And
                .ContainSingle(x => x.Name == nameof(PatientInfo.Lastname)).And
                .ContainSingle(x => x.Name == nameof(PatientInfo.MainDoctorId)).And
                .ContainSingle(x => x.Name == nameof(PatientInfo.BirthDate));

            FormField patientCreateFormFirstnameField = patientsCreateForm.Items.Single(x => x.Name == nameof(PatientInfo.Firstname));
            patientFirstnameField.Type.Should().Be(FormFieldType.String);

            FormField patientCreateFormLastnameField = patientsCreateForm.Items.Single(x => x.Name == nameof(PatientInfo.Lastname));
            patientLastnameField.Type.Should().Be(FormFieldType.String);

            FormField patientCreateFormBirthDateField = patientsCreateForm.Items.Single(x => x.Name == nameof(PatientInfo.BirthDate));
            patientBirthDateField.Type.Should().Be(Date);

            FormField patientCreateFormMainDoctorIdField = patientsCreateForm.Items.Single(x => x.Name == nameof(PatientInfo.MainDoctorId));
            patientCreateFormMainDoctorIdField.Type.Should().Be(FormFieldType.String);

            #endregion
            #endregion

            if (environmentName != "Production")
            {
                endpoints.Should()
                    .Contain(x => x.Name == "documentation");

                Endpoint documentationEnpoint = endpoints.Single(x => x.Name == "documentation");
                documentationEnpoint.Link.Should()
                    .NotBeNull();
            }
        }
    }
}
