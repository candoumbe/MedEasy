using FluentAssertions;
using Measures.API.Features;
using Measures.API.Features.BloodPressures;
using Measures.API.Features.Patients;
using Measures.API.Routing;
using Measures.DTO;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static MedEasy.RestObjects.FormFieldType;
using static Moq.MockBehavior;

namespace Measures.API.Tests.Features
{
    /// <summary>
    /// Unit tests for <see cref="RootController"/>
    /// </summary>
    [UnitTest]
    [Feature("Measures")]
    public class RootControllerTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IHostingEnvironment> _hostingEnvironmentMock;
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<IOptions<MeasuresApiOptions>> _optionsMock;
        private RootController _controller;
        private const string _baseUrl = "http://host/api";

        public RootControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _hostingEnvironmentMock = new Mock<IHostingEnvironment>(Strict);
            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string routename, object routeValues) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            
            _optionsMock = new Mock<IOptions<MeasuresApiOptions>>(Strict);
            _optionsMock.Setup(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = PaginationConfiguration.DefaultPageSize, MaxPageSize = PaginationConfiguration.MaxPageSize });

            _controller = new RootController(_hostingEnvironmentMock.Object, _urlHelperMock.Object, _optionsMock.Object);
        }

        public void Dispose()
        {
            _hostingEnvironmentMock = null;
            _urlHelperMock = null;
            _controller = null;
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
            IEnumerable<Endpoint> endpoints = _controller.Index();
            
            // Assert
            endpoints.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Name)).And
                .NotContain(x => x.Link == null, $"{nameof(Endpoint)}'s {nameof(Endpoint.Link)} must be provided").And
                .BeInAscendingOrder(x => x.Name).And
                .ContainSingle(x => x.Name == BloodPressuresController.EndpointName.ToLowerKebabCase()).And
                .ContainSingle(x => x.Name == PatientsController.EndpointName.ToLowerKebabCase());

            // BloodPressures endpoint
            #region Blood pressures endpoint
            Endpoint bloodPressuresEndpoint = endpoints.Single(x => x.Name == BloodPressuresController.EndpointName.ToLowerKebabCase());
            bloodPressuresEndpoint.Link.Should()
                .NotBeNull($"{nameof(Endpoint)}.{nameof(Endpoint.Link)} must be provided");
            bloodPressuresEndpoint.Link.Relation.Should()
                .Be(LinkRelation.Collection);
            bloodPressuresEndpoint.Link.Method.Should()
                .Be("GET");
            bloodPressuresEndpoint.Link.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize=30");
            bloodPressuresEndpoint.Link.Title.Should()
                .Be("Collection of blood pressures");

            IEnumerable<Form> bloodPressuresForms = bloodPressuresEndpoint.Forms;
            bloodPressuresForms.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .HaveCount(2).And
                .NotContain(x => x.Meta == null, "Forms cannot contain any form with null metadata").And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Meta.Relation), $"{nameof(Link.Relation)} must be provided for each {nameof(Form)}'s {nameof(Form.Meta)} property.").And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Meta.Href), $"{nameof(Link.Href)} must be provided for each {nameof(Form)}'s {nameof(Form.Meta)} property.").And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Meta.Method), $"{nameof(Link.Method)} must be provided for each {nameof(Form)}'s {nameof(Form.Meta)} property.").And
                .ContainSingle(x => x.Meta.Relation == LinkRelation.Search, $"a search form must be provided for {BloodPressuresController.EndpointName} endpoint.").And
                .ContainSingle(x => x.Meta.Relation == LinkRelation.EditForm, $"an edit form must be provided for {BloodPressuresController.EndpointName} endpoint.");

            #region Search form
            Form bloodPressureSearchForm = bloodPressuresForms.Single(x => x.Meta.Relation == LinkRelation.Search);
            bloodPressureSearchForm.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Name == null, $"All items must have {nameof(FormField.Name)} property set").And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.Page)).And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.PageSize)).And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.Sort)).And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.From)).And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.To));

            IEnumerable<FormField> bloodPressureSearchFormItems = bloodPressureSearchForm.Items;

            FormField bloodPressureSearchPageField = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.Page));
            bloodPressureSearchPageField.Description.Should().Be("Index of a page of results");
            bloodPressureSearchPageField.Type.Should().Be(Integer);
            bloodPressureSearchPageField.Min.Should().Be(1);
            bloodPressureSearchPageField.Label.Should().Be("Page");
            

            FormField bloodPressureSearchPageSizeField = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.PageSize));
            bloodPressureSearchPageSizeField.Description.Should().Be("Number of items per page");
            bloodPressureSearchPageSizeField.Type.Should().Be(Integer);
            bloodPressureSearchPageSizeField.Min.Should().Be(1);
            bloodPressureSearchPageSizeField.Max.Should().Be(_optionsMock.Object.Value.MaxPageSize);

            FormField bloodPressuresSearchSortField = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.Sort));
            bloodPressuresSearchSortField.Type.Should().Be(FormFieldType.String);
            bloodPressuresSearchSortField.Pattern.Should().Be(SearchBloodPressureInfo.SortPattern, $"{nameof(SearchBloodPressureInfo.Sort)}.{nameof(FormField.Pattern)} must be specified");
            bloodPressuresSearchSortField.Label.Should().Be(nameof(SearchBloodPressureInfo.Sort));

            FormField bloodPressuresSearchFromField = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.From));
            bloodPressuresSearchFromField.Type.Should().Be(FormFieldType.DateTime);
            bloodPressuresSearchFromField.Description.Should()
                .Be("Minimum date of measure");

            FormField bloodPressuresSearchToField = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.To));
            bloodPressuresSearchToField.Type.Should().Be(FormFieldType.DateTime);
            bloodPressuresSearchToField.Description.Should()
                .Be("Maximum date of measure");
            #endregion

            
            #region Edit form
            Form bloodPressureEditForm = bloodPressuresEndpoint.Forms.Single(x => x.Meta.Relation == LinkRelation.EditForm);

            bloodPressureEditForm.Meta.Method.Should()
                .Be("PATCH");
            bloodPressureEditForm.Meta.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={BloodPressuresController.EndpointName}&{nameof(BloodPressureInfo.Id)}={Uri.EscapeDataString($"{{{nameof(BloodPressureInfo.Id)}}}")}");

            bloodPressureEditForm.Items.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .OnlyContain(x => !string.IsNullOrWhiteSpace(x.Name)).And
                .ContainSingle(x => x.Name == nameof(BloodPressureInfo.Id)).And
                .ContainSingle(x => x.Name == nameof(BloodPressureInfo.DateOfMeasure)).And
                .ContainSingle(x => x.Name == nameof(BloodPressureInfo.DiastolicPressure)).And
                .ContainSingle(x => x.Name == nameof(BloodPressureInfo.SystolicPressure));

            FormField bloodPressureEditFormDateOfMeasureField = bloodPressureEditForm.Items.Single(x => x.Name == nameof(BloodPressureInfo.DateOfMeasure));
            bloodPressureEditFormDateOfMeasureField.Type.Should()
                .Be(FormFieldType.DateTime);

            FormField bloodPressureEditFormDiastolicField = bloodPressureEditForm.Items.Single(x => x.Name == nameof(BloodPressureInfo.DiastolicPressure));
            bloodPressureEditFormDiastolicField.Type.Should()
                .Be(Integer);
            bloodPressureEditFormDiastolicField.Min.Should().Be(0);
            bloodPressureEditFormDiastolicField.Max.Should().BeNull();


            FormField bloodPressureEditFormSystolicField = bloodPressureEditForm.Items.Single(x => x.Name == nameof(BloodPressureInfo.SystolicPressure));
            bloodPressureEditFormSystolicField.Type.Should()
                .Be(Integer);
            bloodPressureEditFormSystolicField.Min.Should().Be(0);
            bloodPressureEditFormSystolicField.Max.Should().BeNull();
            #endregion

            #endregion

            #region Patients endpoint
            // Patients endpoint
            Endpoint patientsEndpoint = endpoints.Single(x => x.Name == PatientsController.EndpointName.ToLowerKebabCase());
            patientsEndpoint.Link.Should()
                .NotBeNull($"{nameof(Endpoint)}.{nameof(Endpoint.Link)} must be provided");
            patientsEndpoint.Link.Relation.Should()
                .Be(LinkRelation.Collection);
            patientsEndpoint.Link.Method.Should()
                .Be("GET");
            patientsEndpoint.Link.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName.ToLowerKebabCase()}&page=1&pageSize=30");
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
                .ContainSingle(x => x.Meta.Relation == LinkRelation.CreateForm).And
                .ContainSingle(x => x.Meta.Relation == "create-form-bloodpressure")
                ;

            #region Search form
            Form patientsSearchForm = patientsEndpoint.Forms.Single(x => x.Meta.Relation == LinkRelation.Search);
            patientsSearchForm.Meta.Should()
                .NotBeNull();
            patientsSearchForm.Meta.Method.Should()
                .Be("GET");
            patientsSearchForm.Meta.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={PatientsController.EndpointName}&page=1&pageSize=30");

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

            #region Create form bloodpressure
            Form bloodPressureCreateForm = patientsEndpoint.Forms.Single(f => f.Meta.Relation == "create-form-bloodpressure");
            bloodPressureCreateForm.Meta.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?action={nameof(PatientsController.PostBloodPressure)}&controller=patients&id={Uri.EscapeDataString("{id}")}");
            bloodPressureCreateForm.Meta.Method.Should()
                .Be("POST");
            bloodPressureCreateForm.Meta.Template.Should()
                .BeTrue();

            bloodPressureCreateForm.Items.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .ContainSingle(field => field.Name == nameof(NewBloodPressureModel.SystolicPressure)).And
                .ContainSingle(field => field.Name == nameof(NewBloodPressureModel.DiastolicPressure)).And
                .ContainSingle(field => field.Name == nameof(NewBloodPressureModel.DateOfMeasure))
                ;

            FormField bloodPressureDateOfMeasureField = bloodPressureCreateForm.Items.Single(x => x.Name == nameof(NewBloodPressureModel.DateOfMeasure));
            bloodPressureDateOfMeasureField.Type.Should()
                .Be(FormFieldType.DateTime);

            FormField bloodPressureDiastolicField = bloodPressureCreateForm.Items.Single(x => x.Name == nameof(NewBloodPressureModel.DiastolicPressure));
            bloodPressureDiastolicField.Type.Should()
                .Be(Integer);
            bloodPressureDiastolicField.Min.Should().Be(0);
            bloodPressureDiastolicField.Max.Should().BeNull();


            FormField bloodPressureSystolicField = bloodPressureCreateForm.Items.Single(x => x.Name == nameof(NewBloodPressureModel.SystolicPressure));
            bloodPressureSystolicField.Type.Should()
                .Be(Integer);
            bloodPressureSystolicField.Min.Should().Be(0);
            bloodPressureSystolicField.Max.Should().BeNull();

            #endregion
            #region Create Form
            Form patientsCreateForm = patientsEndpoint.Forms.Single(form => form.Meta.Relation == LinkRelation.CreateForm);
            patientsCreateForm.Items.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .ContainSingle(field => field.Name == nameof(NewPatientInfo.Firstname)).And
                .ContainSingle(field => field.Name == nameof(NewPatientInfo.Lastname)).And
                .ContainSingle(field => field.Name == nameof(NewPatientInfo.BirthDate))
                ;

            FormField createPatientFirstnameField = patientsCreateForm.Items.Single(field => field.Name == nameof(NewPatientInfo.Firstname));
            createPatientFirstnameField.Type.Should()
                .Be(FormFieldType.String);
            createPatientFirstnameField.MaxLength.Should()
                .Be(255);

            FormField createPatientLastnameField = patientsCreateForm.Items.Single(field => field.Name == nameof(NewPatientInfo.Lastname));
            createPatientLastnameField.Type.Should()
                .Be(FormFieldType.String);
            createPatientLastnameField.MaxLength.Should()
                .Be(255);

            FormField createPatientBirthDateField = patientsCreateForm.Items.Single(field => field.Name == nameof(NewPatientInfo.BirthDate));
            createPatientBirthDateField.Type.Should()
                .Be(Date);

            #endregion
            #endregion

            if (environmentName != "Production")
            {
                endpoints.Should()
                    .ContainSingle(x => x.Name == "documentation");

                Endpoint documentationEnpoint = endpoints.Single(x => x.Name == "documentation");
                documentationEnpoint.Link.Should()
                    .NotBeNull();
            }
            
        }
    }
}
