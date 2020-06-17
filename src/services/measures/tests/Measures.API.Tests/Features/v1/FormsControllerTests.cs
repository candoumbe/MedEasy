using AutoMapper.QueryableExtensions;

using Bogus;

using DataFilters;

using FluentAssertions;
using FluentAssertions.Extensions;
using Forms;

using Measures.API.Features.Patients;
using Measures.API.Features.v1.BloodPressures;
using Measures.API.Features.v1.Patients;
using Measures.API.Routing;
using Measures.Context;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Commands.Patients;
using Measures.CQRS.Queries.Patients;
using Measures.DTO;
using Measures.Mapping;
using Measures.Objects;

using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Moq;

using Optional;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

using static Microsoft.AspNetCore.Http.StatusCodes;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;
using static System.StringComparison;
using static Forms.LinkRelation;
using MedEasy.Models;
using System.Reflection;
using Measures.CQRS.Handlers.Patients;
using MedEasy.Attributes;
using Measures.Models.v1;
using Measures.CQRS.Commands;
using Bogus.DataSets;
using System.Text.Json;
using MedEasy.Core.Results;
using FluentAssertions.Common;
using Measures.API.Features.v1;
using AutoMapper;

namespace Measures.API.Tests.Features.v1
{
    [Feature("Forms")]
    [UnitTest]
    public class FormsControllerTests : IDisposable
    {
        private Mock<LinkGenerator> _urlHelperMock;
        private FormsController _sut;
        private ITestOutputHelper _outputHelper;
        private Mock<IOptionsSnapshot<MeasuresApiOptions>> _apiOptionsMock;
        private Mock<IMediator> _mediatorMock;
        private readonly Mock<IMapper> _mapperMock;
        private const string _baseUrl = "http://host/api";
        private IUnitOfWorkFactory _uowFactory;
        private static readonly ApiVersion _apiVersion = new ApiVersion(1, 0);
        private readonly Faker _faker;

        public FormsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _urlHelperMock = new Mock<LinkGenerator>(Strict);
            _urlHelperMock.Setup(mock => mock.GetPathByAddress(It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<PathString>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
                .Returns((string routename, RouteValueDictionary routeValues, PathString _, FragmentString __, LinkOptions ___) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            DbContextOptionsBuilder<MeasuresContext> dbOptions = new DbContextOptionsBuilder<MeasuresContext>();
            string dbName = $"InMemoryMedEasyDb_{Guid.NewGuid()}";
            dbOptions.UseInMemoryDatabase(dbName);
            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbOptions.Options, (options) => new MeasuresContext(options));

            _apiOptionsMock = new Mock<IOptionsSnapshot<MeasuresApiOptions>>(Strict);

            _mediatorMock = new Mock<IMediator>(Strict);
            _mapperMock = new Mock<IMapper>(Strict);

            _faker = new Faker();

            _sut = new FormsController(_urlHelperMock.Object, _apiOptionsMock.Object, _mediatorMock.Object, _apiVersion, _mapperMock.Object);
        }

        public void Dispose()
        {
            _urlHelperMock = null;
            _sut = null;
            _outputHelper = null;
            _apiOptionsMock = null;
            _mediatorMock = null;
            _uowFactory = null;
        }

        [Fact]
        public void Has_endpoints_to_handle_MeasureFormInfo_resources()
        {
            // Act
            Type formsControllerType = typeof(FormsController);

            // Assert
            formsControllerType.Should()
                                  .BeDecoratedWith<ApiControllerAttribute>().And
                                  .BeDecoratedWith<ApiVersionAttribute>(attr => attr.Versions.Once(v => v.MajorVersion == 1 && v.MinorVersion == 0));

            formsControllerType.Should()
                                  .HaveMethod("Get", new[] { typeof(Guid), typeof(CancellationToken) }, "controller be used to get").Which
                                  .Should()
                                  .Return<Task<ActionResult<Browsable<GenericMeasureFormModel>>>>().And
                                  .BeDecoratedWith<HttpGetAttribute>(attr => attr.Template == "{id}").And
                                  .BeDecoratedWith<HttpHeadAttribute>(attr => attr.Template == "{id}");

            MethodInfo getAllFormsEndpoints = formsControllerType.Should()
                                                                 .HaveMethod("Get", new[] { typeof(int), typeof(int), typeof(CancellationToken) }, "controller be used to get").Which;

            getAllFormsEndpoints.Should()
                                .Return<Task<GenericPageModel<Browsable<GenericMeasureFormModel>>>>().And
                                .BeDecoratedWith<HttpGetAttribute>(attr => string.IsNullOrEmpty(attr.Template)).And
                                .BeDecoratedWith<HttpHeadAttribute>(attr => string.IsNullOrEmpty(attr.Template));

            ParameterInfo[] parameters = getAllFormsEndpoints.GetParameters();

            parameters.Should()
                      .HaveCount(3).And
                      .ContainSingle(pi => pi.Name == "page" && pi.Position == 0
                                           && pi.ParameterType == typeof(int)
                                           && !pi.HasDefaultValue
                                           && pi.GetCustomAttribute<MinimumAttribute>() == new MinimumAttribute(1)).And
                      .ContainSingle(pi => pi.Name == "pageSize" && pi.Position == 1
                                           && pi.ParameterType == typeof(int)
                                           && !pi.HasDefaultValue
                                           && pi.GetCustomAttribute<MinimumAttribute>() == new MinimumAttribute(1)).And
                      .ContainSingle(pi => pi.Position == 2 && pi.ParameterType.Equals(typeof(CancellationToken))
                                           && pi.HasDefaultValue)
                      ;
        }

        [Fact]
        public void Has_endpoints_for_getting_all_forms()
        {
            // Act
            Type controllerType = typeof(FormsController);

            // Assert
            controllerType.Should()
                          .BeDecoratedWith<ApiControllerAttribute>().And
                          .BeDecoratedWith<ApiVersionAttribute>(attr => attr.Versions.Once(v => v.MajorVersion == 1 && v.MinorVersion == 0));

            MethodInfo getAllEndpoint = controllerType.Should()
                                                      .HaveMethod("Get", new[] { typeof(int), typeof(int), typeof(CancellationToken) }, "controller must be usable to get measures").Which;

            getAllEndpoint.Should()
                          .Return<Task<GenericPageModel<Browsable<GenericMeasureFormModel>>>>().And
                          .BeDecoratedWith<HttpGetAttribute>(attr => string.IsNullOrEmpty(attr.Template)).And
                          .BeDecoratedWith<HttpHeadAttribute>(attr => string.IsNullOrWhiteSpace(attr.Template)).And
                          .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status200OK && typeof(GenericPageModel<Browsable<GenericMeasureFormModel>>).Equals(attr.Type));

            ParameterInfo[] parameters = getAllEndpoint.GetParameters();
            parameters.Should()
                      .HaveCount(3).And
                      .ContainSingle(pi => pi.Name == "page"
                                           && pi.ParameterType.Equals(typeof(int))
                                           && pi.GetCustomAttribute<MinimumAttribute>() == new MinimumAttribute(1)).And
                      .ContainSingle(pi => pi.Name == "pageSize"
                                           && pi.ParameterType.Equals(typeof(int))
                                           && pi.GetCustomAttribute<MinimumAttribute>() == new MinimumAttribute(1)).And
                      .ContainSingle(pi => pi.ParameterType.Equals(typeof(CancellationToken)));
        }

        [Fact]
        public void Has_endpoints_for_getting_one_form()
        {
            // Act
            Type controllerType = typeof(FormsController);

            // Assert
            MethodInfo getOneMeasureEndpoint = controllerType.Should()
                                                             .HaveMethod("Get", new[] { typeof(Guid), typeof(CancellationToken) }, "controller must be usable to get one form").Which;

            getOneMeasureEndpoint.Should()
                                 .Return<Task<ActionResult<Browsable<GenericMeasureFormModel>>>>().And
                                 .BeDecoratedWith<HttpGetAttribute>(attr => attr.Template == "{id}").And
                                 .BeDecoratedWith<HttpHeadAttribute>(attr => attr.Template == "{id}").And
                                 .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status404NotFound, "the endpoint will returns 404 if the form does not exist");

            ParameterInfo[] parameters = getOneMeasureEndpoint.GetParameters();
            parameters.Should()
                      .HaveCount(2).And
                      .ContainSingle(pi => pi.Name == "id"
                                           && pi.ParameterType.Equals(typeof(Guid))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.ParameterType.Equals(typeof(CancellationToken)));
        }

        [Fact]
        public void Has_endpoints_for_deleting_one_form()
        {
            // Act
            Type controllerType = typeof(FormsController);

            // Assert
            MethodInfo deleteOneResourceEndpoint = controllerType.Should()
                                                                 .HaveMethod("Delete", new[] { typeof(Guid), typeof(CancellationToken) }, "controller must be usable to get measures").Which;

            deleteOneResourceEndpoint.Should()
                                     .Return<Task<ActionResult>>().And
                                     .BeDecoratedWith<HttpDeleteAttribute>(attr => attr.Template == "{id}").And
                                     .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status404NotFound, "the endpoint will returns 404 if the measure does not exist").And
                                     .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status204NoContent);

            ParameterInfo[] parameters = deleteOneResourceEndpoint.GetParameters();
            parameters.Should()
                      .HaveCount(2).And
                      .ContainSingle(pi => pi.Name == "id"
                                           && pi.ParameterType.Equals(typeof(Guid))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.ParameterType.Equals(typeof(CancellationToken)));
        }

        public static IEnumerable<object[]> GetAllTestCases
        {
            get
            {
                int[] pageSizes = { 1, 10, 20 };
                int[] pages = { 1, 5, 10 };

                foreach (int pageSize in pageSizes)
                {
                    foreach (int page in pages)
                    {
                        yield return new object[]
                        {
                            Enumerable.Empty<MeasureForm>(), // Current store state
                            (pageSize, page), // request,
                            (defaultPageSize : 30, maxPageSize : 200),
                            0,    //expected total
                            (
                                first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First
                                    &&
                                        ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                        $"Controller={FormsController.EndpointName}" +
                                        $"&page=1" +
                                        $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                next :(Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last
                                    &&
                                        ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                        $"Controller={FormsController.EndpointName}" +
                                        $"&page=1" +
                                        $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase))
                            )  // expected link to last page
                        };
                    }
                }

                Faker<MeasureForm> formFaker = new Faker<MeasureForm>()
                    .CustomInstantiator(faker =>
                    {
                        MeasureForm form = new MeasureForm(Guid.NewGuid(), faker.Person.FullName);

                        form.AddFloatField("systolic", min: faker.Random.Float(min: 10, max: 15));
                        form.AddFloatField("diastolic", min: faker.Random.Float(min: 0, max: 10));

                        return form;
                    });
                {
                    IEnumerable<MeasureForm> items = formFaker.Generate(400);

                    yield return new object[]
                    {
                        items,
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : (Expression<Func<Link, bool>>) (x => x != null
                                                                         && x.Relation == First
                                                                         && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={FormsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            next : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Next
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={FormsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            last : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Last
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={FormsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase))
                        )  // expected link to last page
                    };
                }
                {
                    IEnumerable<MeasureForm> items = formFaker.Generate(400);

                    yield return new object[]
                    {
                        items,
                        (pageSize : 10, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : (Expression<Func<Link, bool>>) (x => x != null
                                                                         && x.Relation == First
                                                                         && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={FormsController.EndpointName}&page=1&pageSize=10&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            next : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Next
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={FormsController.EndpointName}&page=2&pageSize=10&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            last : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Last
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={FormsController.EndpointName}&page=40&pageSize=10&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }

                {
                    MeasureForm form = new MeasureForm(Guid.NewGuid(), "heartbeat");
                    yield return new object[]
                    {
                        new [] { form },
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        1,    //expected total
                        (
                            first : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == First
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={FormsController.EndpointName}" +
                                    "&page=1" +
                                    $"&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previous :(Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            next : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                            last : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Last
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={FormsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase))
                        ), // expected link to last page
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<MeasureForm> items, (int pageSize, int page) request,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(FormsController.Get)}({nameof(request.page)},{request.pageSize})");
            _outputHelper.WriteLine($"Page size : {request.pageSize}");
            _outputHelper.WriteLine($"Page : {request.page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<MeasureForm>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfGenericMeasureFormInfoQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetPageOfGenericMeasureFormInfoQuery query, CancellationToken cancellationToken) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    Expression<Func<MeasureForm, GenericMeasureFormInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder
                                                                                                     .GetMapExpression<MeasureForm, GenericMeasureFormInfo>();
                    return uow.Repository<MeasureForm>()
                              .ReadPageAsync(
                                    selector,
                                    query.Data.PageSize,
                                    query.Data.Page,
                                    new Sort<GenericMeasureFormInfo>(nameof(GenericMeasureFormInfo.UpdatedDate)),
                                    cancellationToken)
                              .AsTask();
                });

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            GenericPageModel<Browsable<GenericMeasureFormModel>> response = await _sut.Get(request.page, request.pageSize )
                                                                                      .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(FormsController)}.{nameof(FormsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MeasuresApiOptions.MaxPageSize)} value");
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfGenericMeasureFormInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            _outputHelper.WriteLine($"response : {response}");

            response.Items.Should()
                .NotBeNull();

            if (response.Items.Any())
            {
                response.Items.Should()
                    .NotContainNulls().And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == Self));
            }

            response.Total.Should()
                    .Be(expectedCount, $@"because the ""{nameof(GenericPageModel<GenericMeasureFormModel>)}.{nameof(GenericPageModel<GenericMeasureFormModel>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(linksExpectation.lastPageUrlExpectation);
        }

        //public static IEnumerable<object[]> SearchCases
        //{
        //    get
        //    {
        //        {
        //            SearchMeasureFormInfo searchInfo = new SearchPatientInfo
        //            {
        //                Name = "bruce",
        //                Page = 1,
        //                PageSize = 30,
        //                Sort = "-birthdate"
        //            };

        //            yield return new object[]
        //            {
        //                Enumerable.Empty<Patient>(),
        //                searchInfo,
        //                (
        //                (Expression<Func<Link, bool>>) (x => x != null
        //                    && x.Relation == First
        //                    && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
        //                        $"Controller={PatientsController.EndpointName}" +
        //                        $"&name={searchInfo.Name}"+
        //                        $"&page=1&pageSize=30" +
        //                        $"&sort={searchInfo.Sort}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
        //                (Expression<Func<Link, bool>>)(previous => previous == null),
        //                (Expression<Func<Link, bool>>)(next => next == null),
        //                (Expression<Func<Link, bool>>) (x => x != null
        //                    && x.Relation == Last
        //                    && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
        //                        $"Controller={PatientsController.EndpointName}" +
        //                        $"&name={searchInfo.Name}"+
        //                        $"&page=1" +
        //                        $"&pageSize=30" +
        //                        $"&sort={searchInfo.Sort}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)))
        //            };
        //        }
        //        {
        //            SearchPatientInfo searchInfo = new SearchPatientInfo
        //            {
        //                Name = "!wayne",
        //                Page = 1,
        //                PageSize = 30,
        //                Sort = "-birthdate"
        //            };
        //            Patient patient = new Patient(Guid.NewGuid(), "Bruce wayne");

        //            yield return new object[]
        //            {
        //                new [] { patient },
        //                searchInfo,
        //                (
        //                   (Expression<Func<Link, bool>>) (x => x != null
        //                    && x.Relation == First
        //                    && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
        //                        $"Controller={PatientsController.EndpointName}" +
        //                        $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
        //                        $"&page=1&pageSize=30" +
        //                        $"&sort={searchInfo.Sort}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)),
        //                    (Expression<Func<Link, bool>>)(previous => previous == null),
        //                    (Expression<Func<Link, bool>>)(next => next == null),
        //                    (Expression<Func<Link, bool>>) (x => x != null
        //                        && x.Relation == Last
        //                        && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
        //                            $"Controller={PatientsController.EndpointName}" +
        //                            $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
        //                            $"&page=1&pageSize=30" +
        //                            $"&sort={searchInfo.Sort}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase))
        //                )
        //            };
        //        }
        //        {
        //            SearchPatientInfo searchInfo = new SearchPatientInfo
        //            {
        //                Name = "bruce",
        //                Page = 1,
        //                PageSize = 30,
        //            };
        //            Patient patient = new Patient(Guid.NewGuid(), "Bruce");

        //            yield return new object[]
        //            {
        //                new [] { patient },
        //                searchInfo,
        //                (
        //                    (Expression<Func<Link, bool>>) (x => x != null
        //                        && x.Relation == First
        //                        && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
        //                            $"Controller={PatientsController.EndpointName}" +
        //                            $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
        //                            $"&page=1&pageSize=30&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
        //                    (Expression<Func<Link, bool>>)(previous => previous == null),
        //                    (Expression<Func<Link, bool>>)(next => next == null),
        //                    (Expression<Func<Link, bool>>) (x => x != null
        //                        && x.Relation == Last
        //                        && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
        //                            $"Controller={PatientsController.EndpointName}" +
        //                            $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
        //                            $"&page=1&pageSize=30&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase))
        //                )

        //            };
        //        }

        //        {
        //            SearchPatientInfo searchInfo = new SearchPatientInfo
        //            {
        //                Name = "bruce",
        //                Page = 1,
        //                PageSize = 30,
        //                BirthDate = 31.July(2010)
        //            };
        //            Patient patient = new Patient(Guid.NewGuid(), "Bruce wayne")
        //                .WasBornIn(31.July(2010));

        //            yield return new object[]
        //            {
        //                new [] { patient },
        //                searchInfo,
        //                ( (Expression<Func<Link, bool>>) (x => x != null
        //                    && x.Relation == First
        //                    && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
        //                        $"birthdate={searchInfo.BirthDate.Value:s}" +
        //                        $"&Controller={PatientsController.EndpointName}" +
        //                        $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
        //                        $"&page=1&pageSize=30&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
        //                (Expression<Func<Link, bool>>)(previous => previous == null),
        //                (Expression<Func<Link, bool>>)(next => next == null),
        //                (Expression<Func<Link, bool>>) (x => x != null
        //                    && x.Relation == Last
        //                    && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
        //                        $"birthdate={searchInfo.BirthDate.Value:s}" +
        //                        $"&Controller={PatientsController.EndpointName}" +
        //                        $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
        //                        $"&page=1&pageSize=30&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)))

        //            };
        //        }
        //    }
        //}

        //[Theory]
        //[MemberData(nameof(SearchCases))]
        //public async Task Search(IEnumerable<Patient> entries, SearchPatientInfo searchRequest,
        //(Expression<Func<Link, bool>> firstPageLink, Expression<Func<Link, bool>> previousPageLink, Expression<Func<Link, bool>> nextPageLink, Expression<Func<Link, bool>> lastPageLink) linksExpectation)
        //{
        //    _outputHelper.WriteLine($"Entries : {SerializeObject(entries)}");
        //    _outputHelper.WriteLine($"Request : {SerializeObject(searchRequest)}");

        //    // Arrange
        //    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
        //    {
        //        uow.Repository<Patient>().Create(entries);
        //        await uow.SaveChangesAsync()
        //            .ConfigureAwait(false);
        //    }

        //    _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()))
        //        .Returns((SearchQuery<PatientInfo> query, CancellationToken cancellationToken) =>
        //        {
        //            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
        //            Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();

        //            Expression<Func<Patient, bool>> filter = query.Data.Filter?.ToExpression<Patient>() ?? (_ => true);

        //            return uow.Repository<Patient>()
        //                      .WhereAsync(
        //                            selector,
        //                            filter,
        //                            query.Data.Sort,
        //                            query.Data.PageSize,
        //                            query.Data.Page,
        //                            cancellationToken)
        //                      .AsTask();
        //        });

        //    // Act
        //    IActionResult actionResult = await _sut.Search(searchRequest, default)
        //            .ConfigureAwait(false);

        //    // Assert
        //    GenericPageModel<Browsable<PatientInfo>> content = actionResult.Should()
        //        .NotBeNull().And
        //        .BeOfType<OkObjectResult>().Which
        //            .Value.Should()
        //                .NotBeNull().And
        //                .BeAssignableTo<GenericPageModel<Browsable<PatientInfo>>>().Which;

        //    content.Items.Should()
        //        .NotBeNull($"{nameof(GenericPageModel<object>.Items)} must not be null").And
        //        .NotContainNulls($"{nameof(GenericPageModel<object>.Items)} must not contains null").And
        //        .NotContain(x => x.Resource == null).And
        //        .NotContain(x => x.Links == null);

        //    content.Links.Should()
        //        .NotBeNull();
        //    PageLinksModel links = content.Links;

        //    links.First.Should().Match(linksExpectation.firstPageLink);
        //    links.Previous.Should().Match(linksExpectation.previousPageLink);
        //    links.Next.Should().Match(linksExpectation.nextPageLink);
        //    links.Last.Should().Match(linksExpectation.lastPageLink);
        //}

        //[Fact]
        //public async Task GivenMediatorReturnsEmptyPage_Search_Returns_NotFound_When_Requesting_PageTwoOfResult()
        //{
        //    // Arrange
        //    SearchPatientInfo searchRequest = new SearchPatientInfo
        //    {
        //        Page = 2,
        //        PageSize = 10,
        //        Name = "*e*"
        //    };

        //    _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(Page<PatientInfo>.Empty(pageSize: 10));

        //    // Act
        //    IActionResult actionResult = await _sut.Search(searchRequest, default)
        //            .ConfigureAwait(false);

        //    // Assert
        //    actionResult.Should()
        //        .NotBeNull().And
        //        .BeAssignableTo<NotFoundResult>();
        //}

        public static IEnumerable<object> PatchCases
        {
            get
            {
                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.Name, "Bruce");
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new Patient(Guid.NewGuid(), "John Doe"),
                        patchDocument,
                        (Expression<Func<Patient, bool>>)(x => x.Id == patientId && x.Name == "Bruce")
                    };
                }
            }
        }

        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneMeasureFormByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetOneMeasureFormByIdQuery query, CancellationToken ct) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    Expression<Func<MeasureForm, GenericMeasureFormInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder
                        .GetMapExpression<MeasureForm, GenericMeasureFormInfo>();

                    return uow.Repository<MeasureForm>()
                              .SingleOrDefaultAsync(selector, (MeasureForm x) => x.Id == query.Id, ct)
                              .AsTask();
                });

            //Act
            ActionResult<Browsable<GenericMeasureFormModel>> actionResult = await _sut.Get(Guid.NewGuid())
                                                                                      .ConfigureAwait(false);

            //Assert
            actionResult.Result.Should()
                .NotBeNull().And
                .BeOfType<NotFoundResult>().Which
                    .StatusCode.Should().Be(404);

            _mediatorMock.Verify();
        }

        [Fact]
        public async Task Get_returns_200Ok_when_mediator_returns_something()
        {
            //Arrange
            MeasureForm form = new MeasureForm(Guid.NewGuid(), "heart-beat");
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<MeasureForm>().Create(form);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneMeasureFormByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetOneMeasureFormByIdQuery query, CancellationToken ct) =>
               {
                   using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                   Expression<Func<MeasureForm, GenericMeasureFormInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<MeasureForm, GenericMeasureFormInfo>();

                   return await uow.Repository<MeasureForm>().SingleOrDefaultAsync(selector, (MeasureForm x) => x.Id == query.Data, ct)
                       .ConfigureAwait(false);
               });

            //Act
            ActionResult<Browsable<GenericMeasureFormModel>> actionResult = await _sut.Get(form.Id)
                                                                                      .ConfigureAwait(false);

            //Assert
            actionResult.Value.Should().NotBeNull();
            Browsable<GenericMeasureFormModel> result = actionResult.Value;

            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .HaveCount(2).And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href), $"{nameof(Browsable<GenericMeasureFormModel>)}{nameof(Browsable<GenericMeasureFormModel>.Links)} cannot contain any element " +
                    $"with null/empty/whitespace {nameof(Link.Href)}s").And
                .ContainSingle(x => x.Relation == Self).And
                .ContainSingle(x => x.Relation == "delete");

            Link self = links.Single(x => x.Relation == Self);
            self.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={FormsController.EndpointName}&{nameof(GenericMeasureFormInfo.Id)}={form.Id}&version={_apiVersion}");
            self.Relation.Should()
                .NotBeNullOrWhiteSpace()
                .And.BeEquivalentTo(Self);
            self.Method.Should()
                .Be("GET");

            Link linkDelete = links.Single(x => x.Relation == "delete");
            linkDelete.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={FormsController.EndpointName}&{nameof(GenericMeasureFormInfo.Id)}={form.Id}&version={_apiVersion}");
            linkDelete.Method.Should().Be("DELETE");

            GenericMeasureFormModel actualResource = result.Resource;
            actualResource.Should().NotBeNull();
            actualResource.Id.Should().Be(form.Id);
            actualResource.Name.Should().Be(form.Name);
            actualResource.Fields.Should().BeEquivalentTo(form.Fields);

            _urlHelperMock.Verify();
            _mediatorMock.Verify();
        }

        [Fact]
        public async Task WhenMediatorReturnsNotFound_Delete_Returns_NotFound()
        {
            // Arrange
            Guid idToDelete = Guid.NewGuid();

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteMeasureFormInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Failed_NotFound);

            // Act
            IActionResult actionResult = await _sut.Delete(id: idToDelete, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeleteMeasureFormInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteMeasureFormInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task WhenMediatorReturnsSuccess_Delete_Returns_NoContent()
        {
            // Arrange
            Guid idToDelete = Guid.NewGuid();

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteMeasureFormInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            IActionResult actionResult = await _sut.Delete(id: idToDelete, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeleteMeasureFormInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteMeasureFormInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }

        public static IEnumerable<object[]> MediatorReturnsErrorCases
        {
            get
            {
                yield return new object[]
                {
                    CreateCommandResult.Failed_NotFound,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is NotFoundResult)
                };

                yield return new object[]
                {
                    CreateCommandResult.Failed_Conflict,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is StatusCodeResult
                        && ((StatusCodeResult)actionResult).StatusCode == Status409Conflict)
                };
            }
        }

        [Fact]
        public async Task GivenModel_Post_creates_resource()
        {
            // Arrange
            NewMeasureFormModel newForm = new NewMeasureFormModel
            {
                Name = "height",
                Fields = new[]
                {
                    new FormField { Name = "value", Min = 0 },
                    new FormField { Name = "comments", Type = FormFieldType.String }
                }
            };

            MeasuresApiOptions apiOptions = new MeasuresApiOptions { DefaultPageSize = 25, MaxPageSize = 10 };
            _apiOptionsMock.Setup(mock => mock.Value).Returns(apiOptions);
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreateGenericMeasureFormInfoCommand>(), It.IsAny<CancellationToken>()))
                .Returns((CreateGenericMeasureFormInfoCommand cmd, CancellationToken _) =>
                {
                    return Task.FromResult(new GenericMeasureFormInfo
                    {
                        Name = cmd.Data.Name,
                        Id = Guid.NewGuid(),
                        Fields = cmd.Data.Fields
                    });
                });

            // Act
            ActionResult<Browsable<GenericMeasureFormModel>> actionResult = await _sut.Post(newForm, ct: default)
                                                                                      .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<CreateGenericMeasureFormInfoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            CreatedAtRouteResult createdAtRouteResult = actionResult.Result.Should()
                                                                           .BeAssignableTo<CreatedAtRouteResult>().Which;

            Browsable<GenericMeasureFormModel> browsableResource = createdAtRouteResult.Value.Should()
                .BeAssignableTo<Browsable<GenericMeasureFormModel>>().Which;

            GenericMeasureFormModel resource = browsableResource.Resource;
            resource.Should()
                .NotBeNull();

            createdAtRouteResult.RouteName.Should()
                .Be(RouteNames.DefaultGetOneByIdApi);
            createdAtRouteResult.RouteValues.Should()
                                            .Contain("id", resource.Id, "resource id must be provided in routeValues");

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Href), $"each resource link must provide its {nameof(Link.Href)}").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Method), $"each resource link must provide its {nameof(Link.Method)}").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Relation), $"each resource link must provide its {nameof(Link.Relation)}").And
                .HaveCount(2).And
                .Contain(link => link.Relation == Self).And
                .Contain(link => link.Relation == "delete");

            Link linkToSelf = links.Single(link => link.Relation == Self);
            linkToSelf.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={FormsController.EndpointName}&id={resource.Id}&version={_apiVersion}");
            linkToSelf.Method.Should()
                .Be("GET");
        }

        [Fact]
        public async Task Patch_UnknownEntity_Returns_NotFound()
        {
            JsonPatchDocument<GenericMeasureFormModel> changes = new JsonPatchDocument<GenericMeasureFormModel>();
            changes.Replace(x => x.Name, string.Empty);

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, GenericMeasureFormInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Failed_NotFound);
            _mapperMock.Setup(mock => mock.Map<JsonPatchDocument<GenericMeasureFormInfo>>(It.IsAny<JsonPatchDocument<GenericMeasureFormModel>>()))
                       .Returns((JsonPatchDocument<GenericMeasureFormModel> input) => AutoMapperConfig.Build().CreateMapper().Map<JsonPatchDocument<GenericMeasureFormInfo>>(input));

            // Act
            IActionResult actionResult = await _sut.Patch(Guid.NewGuid(), changes)
                                                   .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<PatchCommand<Guid, GenericMeasureFormInfo>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            _mapperMock.Verify(mock => mock.Map<JsonPatchDocument<GenericMeasureFormInfo>>(It.IsAny<JsonPatchDocument<GenericMeasureFormModel>>()), Times.Once);
            _mapperMock.VerifyNoOtherCalls();

            actionResult.Should()
                        .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_Valid_Resource_Returns_NoContentResult()
        {
            // Arrange
            JsonPatchDocument<GenericMeasureFormModel> changes = new JsonPatchDocument<GenericMeasureFormModel>();
            changes.Replace(x => x.Name, string.Empty);

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, GenericMeasureFormInfo>>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(ModifyCommandResult.Done);

            _mapperMock.Setup(mock => mock.Map<JsonPatchDocument<GenericMeasureFormInfo>>(It.IsAny<JsonPatchDocument<GenericMeasureFormModel>>()))
                       .Returns((object input) => AutoMapperConfig.Build().CreateMapper().Map<JsonPatchDocument<GenericMeasureFormInfo>>(input));

            // Act
            IActionResult actionResult = await _sut.Patch(Guid.NewGuid(), changes)
                                                   .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<PatchCommand<Guid, GenericMeasureFormInfo>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            _mapperMock.Verify(mock => mock.Map<JsonPatchDocument<GenericMeasureFormInfo>>(It.IsAny<JsonPatchDocument<GenericMeasureFormModel>>()), Times.Once);
            _mapperMock.VerifyNoOtherCalls();

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async Task Delete_returns_404NotFound_when_mediator_returns_NotFound()
        {
            // Arrange
            Guid formId = Guid.NewGuid();
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteMeasureFormInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(DeleteCommandResult.Failed_NotFound);

            // Act
            ActionResult actionResult = await _sut.Delete(formId, default)
                                                  .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteMeasureFormInfoByIdCommand>(cmd => cmd.Data == formId), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            actionResult.Should()
                        .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_returns_409Conflict_when_mediator_returns_Conflict()
        {
            // Arrange
            Guid formId = Guid.NewGuid();
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteMeasureFormInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(DeleteCommandResult.Failed_Conflict);

            // Act
            ActionResult actionResult = await _sut.Delete(formId, default)
                                                  .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteMeasureFormInfoByIdCommand>(cmd => cmd.Data == formId), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            actionResult.Should()
                        .BeAssignableTo<ConflictResult>();
        }

        [Fact]
        public async Task Delete_returns_403Unauthorized_when_mediator_returns_Unauthorize()
        {
            // Arrange
            Guid formId = Guid.NewGuid();
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteMeasureFormInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(DeleteCommandResult.Failed_Unauthorized);

            // Act
            ActionResult actionResult = await _sut.Delete(formId, default)
                                                  .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteMeasureFormInfoByIdCommand>(cmd => cmd.Data == formId), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            actionResult.Should()
                        .BeAssignableTo<UnauthorizedResult>();
        }
    }
}