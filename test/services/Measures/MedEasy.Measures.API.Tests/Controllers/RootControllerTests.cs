using FluentAssertions;
using Measures.API.Controllers;
using Measures.API.Routing;
using Measures.DTO;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using static MedEasy.RestObjects.FormFieldType;
using MedEasy.DTO.Search;

namespace Measures.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="RootController"/>
    /// </summary>
    public class RootControllerTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IHostingEnvironment> _hostingEnvironmentMock;
        private Mock<IUrlHelper> _urlHelperMock;
        private ActionContextAccessor _actionContextAccessor;
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

            _actionContextAccessor = new ActionContextAccessor()
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            _optionsMock = new Mock<IOptions<MeasuresApiOptions>>(Strict);
            _optionsMock.Setup(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = PaginationConfiguration.DefaultPageSize, MaxPageSize = PaginationConfiguration.MaxPageSize });

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
                .Contain(x => x.Name == BloodPressuresController.EndpointName.ToLowerKebabCase()).And
                .Contain(x => x.Name == PatientsController.EndpointName.ToLowerKebabCase());

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
                .NotContain(x => x.Meta == null, "Forms cannot contain any form with null metadata").And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Meta.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Meta.Href)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Meta.Method)).And
                .ContainSingle(x => x.Meta.Relation == LinkRelation.Search);


            Form bloodPressureSearchForm = bloodPressuresForms.Single(x => x.Meta.Relation == LinkRelation.Search);
            bloodPressureSearchForm.Items.Should()
                .NotContainNulls().And
                .NotContain(x => x.Name == null, $"All items must have {nameof(FormField.Name)} property set").And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.Page)).And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.PageSize)).And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.Sort)).And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.From)).And
                .ContainSingle(x => x.Name == nameof(SearchBloodPressureInfo.To));

            IEnumerable<FormField> bloodPressureSearchFormItems = bloodPressureSearchForm.Items;

            FormField bloodPressureSearchPage = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.Page));
            bloodPressureSearchPage.Description.Should().Be("Index of a page of results");
            bloodPressureSearchPage.Type.Should().Be(Integer);
            bloodPressureSearchPage.Min.Should().Be(1);

            FormField bloodPressureSearchPageSize = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.PageSize));
            bloodPressureSearchPageSize.Description.Should().Be("Number of items per page");
            bloodPressureSearchPageSize.Type.Should().Be(Integer);
            bloodPressureSearchPageSize.Min.Should().Be(1);
            bloodPressureSearchPageSize.Max.Should().Be(_optionsMock.Object.Value.MaxPageSize);

            FormField bloodPressuresSearchSortField = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.Sort));
            bloodPressuresSearchSortField.Type.Should().Be(FormFieldType.String);
            bloodPressuresSearchSortField.Pattern.Should().Be(SearchBloodPressureInfo.SortPattern, $"{nameof(SearchBloodPressureInfo.Sort)}.{nameof(FormField.Pattern)} must be specified");

            FormField bloodPressuresSearchFromField = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.From));
            bloodPressuresSearchFromField.Type.Should().Be(FormFieldType.DateTime);

            FormField bloodPressuresSearchToField = bloodPressureSearchFormItems.Single(x => x.Name == nameof(SearchBloodPressureInfo.To));
            bloodPressuresSearchToField.Type.Should().Be(FormFieldType.DateTime);

            #endregion

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
                .ContainSingle(x => x.Meta.Relation == LinkRelation.Search);


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
