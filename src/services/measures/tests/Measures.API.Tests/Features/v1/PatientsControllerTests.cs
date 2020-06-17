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

namespace Measures.API.Tests.Features.v1.Patients
{
    [Feature("Patients")]
    public class PatientsControllerTests : IDisposable
    {
        private Mock<LinkGenerator> _urlHelperMock;
        private PatientsController _sut;
        private ITestOutputHelper _outputHelper;
        private Mock<IOptionsSnapshot<MeasuresApiOptions>> _apiOptionsMock;
        private Mock<IMediator> _mediatorMock;
        private const string _baseUrl = "http://host/api";
        private IUnitOfWorkFactory _uowFactory;
        private static readonly ApiVersion _apiVersion = new ApiVersion(1, 0);
        private readonly Faker _faker;

        public PatientsControllerTests(ITestOutputHelper outputHelper)
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

            _faker = new Faker();

            _sut = new PatientsController(
                _urlHelperMock.Object,
                _apiOptionsMock.Object,
                _mediatorMock.Object,
                _apiVersion);
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
        public void Has_endpoints_to_handle_PatientInfo_resources()
        {
            // Act
            Type patientsControllerType = typeof(PatientsController);

            // Assert
            patientsControllerType.Should()
                                  .BeDecoratedWith<ApiControllerAttribute>().And
                                  .BeDecoratedWith<ApiVersionAttribute>(attr => attr.Versions.Once(v => v.MajorVersion == 1 && v.MinorVersion == 0));

            patientsControllerType.Should()
                                  .HaveMethod("Get", new[] { typeof(Guid), typeof(CancellationToken) }, "controller be used to get").Which
                                  .Should()
                                  .Return<Task<ActionResult<Browsable<PatientInfo>>>>().And
                                  .BeDecoratedWith<HttpGetAttribute>(attr => attr.Template == "{id}").And
                                  .BeDecoratedWith<HttpHeadAttribute>(attr => attr.Template == "{id}");

            MethodInfo getAllPatientsEndpoints = patientsControllerType.Should()
                                                                       .HaveMethod("Get", new[] { typeof(PaginationConfiguration), typeof(CancellationToken) }, "controller be used to get").Which;

            getAllPatientsEndpoints.Should()
                                   .Return<Task<GenericPageModel<Browsable<PatientInfo>>>>().And
                                   .BeDecoratedWith<HttpGetAttribute>(attr => string.IsNullOrEmpty(attr.Template)).And
                                   .BeDecoratedWith<HttpHeadAttribute>(attr => string.IsNullOrEmpty(attr.Template));
        }

        [Fact]
        public void Has_endpoints_for_getting_all_measures()
        {
            // Act
            Type patientsControllerType = typeof(PatientsController);

            // Assert
            patientsControllerType.Should()
                                  .BeDecoratedWith<ApiControllerAttribute>().And
                                  .BeDecoratedWith<ApiVersionAttribute>(attr => attr.Versions.Once(v => v.MajorVersion == 1 && v.MinorVersion == 0));

            MethodInfo getMeasuresEndpoints = patientsControllerType.Should()
                                                                    .HaveMethod("GetMeasures", new[] { typeof(Guid), typeof(string), typeof(int), typeof(int), typeof(CancellationToken) }, "controller must be usable to get measures").Which;

            getMeasuresEndpoints.Should()
                                .Return<Task<ActionResult<GenericPageModel<Browsable<GenericMeasureModel>>>>>().And
                                .BeDecoratedWith<HttpGetAttribute>(attr => attr.Template == "{id}/{measure}").And
                                .BeDecoratedWith<HttpHeadAttribute>(attr => attr.Template == "{id}/{measure}").And
                                .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status404NotFound, "the endpoint will returns 404 if the measure does not exist").And
                                .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status206PartialContent && typeof(GenericPageModel<Browsable<GenericMeasureModel>>).Equals(attr.Type), "the endpoint can return a subset of large result set").And
                                .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status200OK && typeof(GenericPageModel<Browsable<GenericMeasureModel>>).Equals(attr.Type));

            ParameterInfo[] parameters = getMeasuresEndpoints.GetParameters();
            parameters.Should()
                      .ContainSingle(pi => pi.Name == "id"
                                           && pi.ParameterType.Equals(typeof(Guid))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.Name == "measure"
                                           && pi.ParameterType.Equals(typeof(string))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.Name == "page"
                                           && pi.ParameterType.Equals(typeof(int))
                                           && pi.GetCustomAttribute<MinimumAttribute>() == new MinimumAttribute(1)).And
                      .ContainSingle(pi => pi.Name == "pageSize"
                                           && pi.ParameterType.Equals(typeof(int))
                                           && pi.GetCustomAttribute<MinimumAttribute>() == new MinimumAttribute(1));
        }

        [Fact]
        public void Has_endpoints_for_getting_one_measure()
        {
            // Act
            Type patientsControllerType = typeof(PatientsController);

            // Assert
            MethodInfo getOneMeasureEndpoint = patientsControllerType.Should()
                                                                    .HaveMethod("GetOneMeasurementByPatientId", new[] { typeof(Guid), typeof(string), typeof(Guid), typeof(CancellationToken) }, "controller must be usable to get measures").Which;

            getOneMeasureEndpoint.Should()
                                 .Return<Task<ActionResult<Browsable<GenericMeasureModel>>>>().And
                                 .BeDecoratedWith<HttpGetAttribute>(attr => attr.Template == "{id}/{measure}/{measureId}").And
                                 .BeDecoratedWith<HttpHeadAttribute>(attr => attr.Template == "{id}/{measure}/{measureId}").And
                                 .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status404NotFound, "the endpoint will returns 404 if the measure does not exist").And
                                 .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status200OK && typeof(Browsable<GenericMeasureModel>).Equals(attr.Type));

            ParameterInfo[] parameters = getOneMeasureEndpoint.GetParameters();
            parameters.Should()
                      .HaveCount(4).And
                      .ContainSingle(pi => pi.Name == "id"
                                           && pi.ParameterType.Equals(typeof(Guid))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.Name == "measure"
                                           && pi.ParameterType.Equals(typeof(string))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.Name == "measureId"
                                           && pi.ParameterType.Equals(typeof(Guid))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.ParameterType.Equals(typeof(CancellationToken)));
        }

        [Fact]
        public void Has_endpoints_for_deleting_one_measure()
        {
            // Act
            Type patientsControllerType = typeof(PatientsController);

            // Assert
            MethodInfo getOneMeasureEndpoint = patientsControllerType.Should()
                                                                     .HaveMethod("DeleteOneMeasurementByPatientId", new[] { typeof(Guid), typeof(string), typeof(Guid), typeof(CancellationToken) }, "controller must be usable to get measures").Which;

            getOneMeasureEndpoint.Should()
                                 .Return<Task<ActionResult>>().And
                                 .BeDecoratedWith<HttpDeleteAttribute>(attr => attr.Template == "{id}/{measure}/{measureId}").And
                                 .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status404NotFound, "the endpoint will returns 404 if the measure does not exist").And
                                 .BeDecoratedWith<ProducesResponseTypeAttribute>(attr => attr.StatusCode == Status204NoContent);

            ParameterInfo[] parameters = getOneMeasureEndpoint.GetParameters();
            parameters.Should()
                      .HaveCount(4).And
                      .ContainSingle(pi => pi.Name == "id"
                                           && pi.ParameterType.Equals(typeof(Guid))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.Name == "measure"
                                           && pi.ParameterType.Equals(typeof(string))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.Name == "measureId"
                                           && pi.ParameterType.Equals(typeof(Guid))
                                           && pi.GetCustomAttribute<RequireNonDefaultAttribute>() != null).And
                      .ContainSingle(pi => pi.ParameterType.Equals(typeof(CancellationToken)));
        }

        public static IEnumerable<object> GetLastBloodPressuresMesuresCases
        {
            get
            {
                Faker faker = new Faker();
                yield return new object[]
                {
                    Enumerable.Empty<BloodPressure>(),
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },
                    (Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any())
                };

                yield return new object[]
                {
                    new []
                    {
                        new BloodPressure(id:Guid.NewGuid(), patientId: Guid.NewGuid(), dateOfMeasure: faker.Date.Recent(), systolicPressure: 120, diastolicPressure: 80)
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },

                    (Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any())
                };

                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new []
                        {
                           new BloodPressure(Guid.NewGuid(), patientId, dateOfMeasure: faker.Date.Recent(), systolicPressure: 120, diastolicPressure: 80)
                        },
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 10 },
                        (Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => x.All(measure => measure.PatientId == patientId) && x.Count() == 1)
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetMostRecentTemperaturesMeasuresCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Temperature>(),
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },
                    (Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any())
                };

                yield return new object[]
                {
                    new []
                    {
                        new Temperature(Guid.NewGuid(), Guid.NewGuid(), dateOfMeasure: 18.August(2003), value : 37)
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },
                    (Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any())
                };
                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new []
                        {
                            new Temperature(Guid.NewGuid(), patientId, dateOfMeasure: 18.August(2003), value : 37)
                        },
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 10 },
                        (Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => x.All(measure => measure.PatientId == patientId) && x.Count() == 1)
                    };
                }
            }
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
                            Enumerable.Empty<Patient>(), // Current store state
                            (pageSize, page), // request,
                            (defaultPageSize : 30, maxPageSize : 200),
                            0,    //expected total
                            (
                                first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First
                                    &&
                                        ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                        $"Controller={PatientsController.EndpointName}" +
                                        $"&page=1" +
                                        $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                next :(Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last
                                    &&
                                        ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                        $"Controller={PatientsController.EndpointName}" +
                                        $"&page=1" +
                                        $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase))
                            )  // expected link to last page
                        };
                    }
                }

                Faker<Patient> patientFaker = new Faker<Patient>()
                    .CustomInstantiator(faker =>
                    {
                        Patient patient = new Patient(Guid.NewGuid(), faker.Person.FullName);

                        return patient;
                    });
                {
                    IEnumerable<Patient> items = patientFaker.Generate(400);

                    yield return new object[]
                    {
                        items,
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : (Expression<Func<Link, bool>>) (x => x != null
                                                                         && x.Relation == First
                                                                         && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            next : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Next
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            last : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Last
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase))
                        )  // expected link to last page
                    };
                }
                {
                    IEnumerable<Patient> items = patientFaker.Generate(400);

                    yield return new object[]
                    {
                        items,
                        (pageSize : 10, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : (Expression<Func<Link, bool>>) (x => x != null
                                                                         && x.Relation == First
                                                                         && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize=10&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            next : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Next
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize=10&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            last : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Last
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=40&pageSize=10&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }

                {
                    Patient patient = new Patient(Guid.NewGuid(), "Bruce Wayne");
                    yield return new object[]
                    {
                        new [] { patient },
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        1,    //expected total
                        (
                            first : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == First
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    "&page=1" +
                                    $"&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previous :(Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            next : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                            last : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Last
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase))
                        ), // expected link to last page
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Patient> items, (int pageSize, int page) request,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(PatientsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {request.pageSize}");
            _outputHelper.WriteLine($"Page : {request.page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfPatientInfoQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetPageOfPatientInfoQuery query, CancellationToken cancellationToken) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
                    return uow.Repository<Patient>()
                              .ReadPageAsync(
                                    selector,
                                    query.Data.PageSize,
                                    query.Data.Page,
                                    new Sort<PatientInfo>(nameof(PatientInfo.UpdatedDate)),
                                    cancellationToken)
                              .AsTask();
                });

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            GenericPageModel<Browsable<PatientInfo>> response = await _sut.Get(new PaginationConfiguration { Page = request.page, PageSize = request.pageSize })
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(PatientsController)}.{nameof(PatientsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MeasuresApiOptions.MaxPageSize)} value");
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfPatientInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);

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
                    .Be(expectedCount, $@"because the ""{nameof(GenericPageModel<PatientInfo>)}.{nameof(GenericPageModel<PatientInfo>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(linksExpectation.lastPageUrlExpectation);
        }

        public static IEnumerable<object[]> SearchCases
        {
            get
            {
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Name = "bruce",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };

                    yield return new object[]
                    {
                        Enumerable.Empty<Patient>(),
                        searchInfo,
                        (
                        (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&name={searchInfo.Name}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        (Expression<Func<Link, bool>>)(previous => previous == null),
                        (Expression<Func<Link, bool>>)(next => next == null),
                        (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&name={searchInfo.Name}"+
                                $"&page=1" +
                                $"&pageSize=30" +
                                $"&sort={searchInfo.Sort}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)))
                    };
                }
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Name = "!wayne",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };
                    Patient patient = new Patient(Guid.NewGuid(), "Bruce wayne");

                    yield return new object[]
                    {
                        new [] { patient },
                        searchInfo,
                        (
                           (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)),
                            (Expression<Func<Link, bool>>)(previous => previous == null),
                            (Expression<Func<Link, bool>>)(next => next == null),
                            (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                    $"&page=1&pageSize=30" +
                                    $"&sort={searchInfo.Sort}&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase))
                        )
                    };
                }
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Name = "bruce",
                        Page = 1,
                        PageSize = 30,
                    };
                    Patient patient = new Patient(Guid.NewGuid(), "Bruce");

                    yield return new object[]
                    {
                        new [] { patient },
                        searchInfo,
                        (
                            (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == First
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                    $"&page=1&pageSize=30&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            (Expression<Func<Link, bool>>)(previous => previous == null),
                            (Expression<Func<Link, bool>>)(next => next == null),
                            (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                    $"&page=1&pageSize=30&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase))
                        )

                    };
                }

                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Name = "bruce",
                        Page = 1,
                        PageSize = 30,
                        BirthDate = 31.July(2010)
                    };
                    Patient patient = new Patient(Guid.NewGuid(), "Bruce wayne")
                        .WasBornIn(31.July(2010));

                    yield return new object[]
                    {
                        new [] { patient },
                        searchInfo,
                        ( (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value:s}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                $"&page=1&pageSize=30&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        (Expression<Func<Link, bool>>)(previous => previous == null),
                        (Expression<Func<Link, bool>>)(next => next == null),
                        (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value:s}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                $"&page=1&pageSize=30&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)))

                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(IEnumerable<Patient> entries, SearchPatientInfo searchRequest,
        (Expression<Func<Link, bool>> firstPageLink, Expression<Func<Link, bool>> previousPageLink, Expression<Func<Link, bool>> nextPageLink, Expression<Func<Link, bool>> lastPageLink) linksExpectation)
        {
            _outputHelper.WriteLine($"Entries : {SerializeObject(entries)}");
            _outputHelper.WriteLine($"Request : {SerializeObject(searchRequest)}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(entries);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()))
                .Returns((SearchQuery<PatientInfo> query, CancellationToken cancellationToken) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();

                    Expression<Func<Patient, bool>> filter = query.Data.Filter?.ToExpression<Patient>() ?? (_ => true);

                    return uow.Repository<Patient>()
                              .WhereAsync(
                                    selector,
                                    filter,
                                    query.Data.Sort,
                                    query.Data.PageSize,
                                    query.Data.Page,
                                    cancellationToken)
                              .AsTask();
                });

            // Act
            IActionResult actionResult = await _sut.Search(searchRequest, default)
                    .ConfigureAwait(false);

            // Assert
            GenericPageModel<Browsable<PatientInfo>> content = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<GenericPageModel<Browsable<PatientInfo>>>().Which;

            content.Items.Should()
                .NotBeNull($"{nameof(GenericPageModel<object>.Items)} must not be null").And
                .NotContainNulls($"{nameof(GenericPageModel<object>.Items)} must not contains null").And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            content.Links.Should()
                .NotBeNull();
            PageLinksModel links = content.Links;

            links.First.Should().Match(linksExpectation.firstPageLink);
            links.Previous.Should().Match(linksExpectation.previousPageLink);
            links.Next.Should().Match(linksExpectation.nextPageLink);
            links.Last.Should().Match(linksExpectation.lastPageLink);
        }

        [Fact]
        public async Task GivenMediatorReturnsEmptyPage_Search_Returns_NotFound_When_Requesting_PageTwoOfResult()
        {
            // Arrange
            SearchPatientInfo searchRequest = new SearchPatientInfo
            {
                Page = 2,
                PageSize = 10,
                Name = "*e*"
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Page<PatientInfo>.Empty(pageSize: 10));

            // Act
            IActionResult actionResult = await _sut.Search(searchRequest, default)
                    .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<NotFoundResult>();
        }

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
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPatientInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetPatientInfoByIdQuery query, CancellationToken ct) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder
                        .GetMapExpression<Patient, PatientInfo>();

                    return await uow.Repository<Patient>().SingleOrDefaultAsync(
                        selector,
                        (Patient x) => x.Id == query.Data,
                        ct)
                        .ConfigureAwait(false);
                });

            //Act
            ActionResult<Browsable<PatientInfo>> actionResult = await _sut.Get(Guid.NewGuid())
                .ConfigureAwait(false);

            //Assert
            actionResult.Result.Should()
                .NotBeNull().And
                .BeOfType<NotFoundResult>().Which
                    .StatusCode.Should().Be(404);

            _mediatorMock.Verify();
        }

        [Fact]
        public async Task Get()
        {
            //Arrange
            Patient patient = new Patient(Guid.NewGuid(), "Bruce Wayne");
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(patient);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            PatientInfo expectedResource = new PatientInfo
            {
                Id = patient.Id,
                Name = "Bruce Wayne"
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPatientInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetPatientInfoByIdQuery query, CancellationToken ct) =>
               {
                   using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                   Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();

                   return await uow.Repository<Patient>().SingleOrDefaultAsync(selector, (Patient x) => x.Id == query.Data, ct)
                       .ConfigureAwait(false);
               });

            //Act
            ActionResult<Browsable<PatientInfo>> actionResult = await _sut.Get(patient.Id)
                                                                                 .ConfigureAwait(false);

            //Assert
            actionResult.Value.Should().NotBeNull();
            Browsable<PatientInfo> result = actionResult.Value;

            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href), $"{nameof(Browsable<PatientInfo>)}{nameof(Browsable<PatientInfo>.Links)} cannot contain any element " +
                    $"with null/empty/whitespace {nameof(Link.Href)}s").And
                .ContainSingle(x => x.Relation == Self).And
                .ContainSingle(x => x.Relation == "delete").And
                .ContainSingle(x => x.Relation == BloodPressuresController.EndpointName.Slugify());

            Link self = links.Single(x => x.Relation == Self);
            self.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={expectedResource.Id}&version={_apiVersion}");
            self.Relation.Should()
                .NotBeNullOrWhiteSpace()
                .And.BeEquivalentTo(Self);
            self.Method.Should()
                .Be("GET");

            Link linkDelete = links.Single(x => x.Relation == "delete");
            linkDelete.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={expectedResource.Id}&version={_apiVersion}");
            linkDelete.Method.Should().Be("DELETE");

            Link bloodPressuresLink = links.Single(x => x.Relation == BloodPressuresController.EndpointName.Slugify());
            bloodPressuresLink.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?Controller={BloodPressuresController.EndpointName}&{nameof(BloodPressureInfo.PatientId)}={expectedResource.Id}&version={_apiVersion}");
            bloodPressuresLink.Method.Should().Be("GET");

            PatientInfo actualResource = result.Resource;
            actualResource.Should().NotBeNull();
            actualResource.Id.Should().Be(expectedResource.Id);
            actualResource.Name.Should().Be(expectedResource.Name);

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

        [Fact]
        public async Task GivenMediatorReturnsNone_GetBloodPressures_ReturnsNotFound()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            PaginationConfiguration pagination = new PaginationConfiguration { Page = 1, PageSize = 50 };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPatientInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<PatientInfo>());

            // Act
            IActionResult actionResult = await _sut.GetBloodPressures(id: patientId, pagination: pagination, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPatientInfoByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        public static IEnumerable<object[]> GetBloodPressuresWhenPatientExistsCases
        {
            get
            {
                yield return new object[]
                {
                    (page :1, pageSize:10),
                    (defaultPageSize : 30, maxPageSize : 200)
                };

                yield return new object[]
                {
                    (page :1, pageSize:10),
                    (defaultPageSize : 30, maxPageSize : 5)
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetBloodPressuresWhenPatientExistsCases))]
        public async Task GivenMediatorReturnsSome_GetBloodPressures_RedirectToSearch((int page, int pageSize) pagination, (int defaultPageSize, int maxPageSize) pagingConfiguration)
        {
            // Arrange
            PaginationConfiguration paging = new PaginationConfiguration
            {
                Page = pagination.page,
                PageSize = pagination.pageSize
            };
            Guid patientId = Guid.NewGuid();

            MeasuresApiOptions apiOptions = new MeasuresApiOptions
            {
                DefaultPageSize = pagingConfiguration.defaultPageSize,
                MaxPageSize = pagingConfiguration.maxPageSize
            };
            _apiOptionsMock.Setup(mock => mock.Value).Returns(apiOptions);

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPatientInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                    .Returns((GetPatientInfoByIdQuery query, CancellationToken ct) => new ValueTask<Option<PatientInfo>>(new PatientInfo
                    {
                        Id = query.Data
                    }.Some()).AsTask());

            // Act
            IActionResult actionResult = await _sut.GetBloodPressures(id: patientId, pagination: paging, ct: default)
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPatientInfoByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            RedirectToRouteResult redirect = actionResult.Should()
                .BeAssignableTo<RedirectToRouteResult>().Which;

            redirect.RouteName.Should()
                            .Be(RouteNames.DefaultSearchResourcesApi);
            redirect.PreserveMethod.Should().BeTrue();
            redirect.Permanent.Should().BeFalse();
            redirect.RouteValues.Should()
                        .ContainKey("controller").And
                        .ContainKey("patientId").And
                        .ContainKey("page").And
                        .ContainKey("pageSize");

            redirect.RouteValues["controller"].Should()
                .Be(BloodPressuresController.EndpointName);
            redirect.RouteValues["patientId"].Should()
                        .Be(patientId);
            redirect.RouteValues["page"].Should()
                        .Be(pagination.page);
            redirect.RouteValues["pageSize"].Should()
                        .Be(Math.Min(pagination.pageSize, apiOptions.MaxPageSize), "request pageSize must be capped by the controller");
        }

        [Fact]
        public async Task Post_BloodPressure_For_Patient()
        {
            // Arrange
            NewBloodPressureModel newMeasure = new NewBloodPressureModel
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 30.September(2010).AddHours(14).AddMinutes(53)
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreateBloodPressureInfoForPatientIdCommand>(), It.IsAny<CancellationToken>()))
                .Returns((CreateBloodPressureInfoForPatientIdCommand cmd, CancellationToken cancellationToken) =>
                {
                    return Task.FromResult(new BloodPressureInfo
                    {
                        DateOfMeasure = cmd.Data.DateOfMeasure,
                        Id = Guid.NewGuid(),
                        DiastolicPressure = cmd.Data.DiastolicPressure,
                        PatientId = cmd.Data.PatientId,
                        SystolicPressure = cmd.Data.SystolicPressure,
                        UpdatedDate = 23.June(2010)
                    }.Some<BloodPressureInfo, CreateCommandResult>());
                })
                .Verifiable();
            Guid patientId = Guid.NewGuid();
            // Act

            IActionResult actionResult = await _sut.PostBloodPressure(patientId, newMeasure)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify();

            Browsable<BloodPressureInfo> browsableResource = actionResult.Should()
                .BeOfType<CreatedAtRouteResult>().Which
                .Value.Should()
                    .BeAssignableTo<Browsable<BloodPressureInfo>>().Which;

            BloodPressureInfo resource = browsableResource.Resource;
            resource.Id.Should()
                .NotBeEmpty();
            resource.DateOfMeasure.Should()
                .Be(newMeasure.DateOfMeasure);
            resource.DiastolicPressure.Should()
                .Be(newMeasure.DiastolicPressure);
            resource.PatientId.Should()
                .Be(patientId);

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeEmpty().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href), $"{nameof(Link.Href)} must be provided for each link of the resource").And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation), $"{nameof(Link.Relation)} must be provided for each link of the resource").And
                .Contain(x => x.Relation == "delete-bloodpressure").And
                .Contain(x => x.Relation == Self).And
                .Contain(x => x.Relation == "patient");

            Link linkToPatient = links.Single(x => x.Relation == "patient");
            linkToPatient.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={PatientsController.EndpointName}&id={resource.PatientId}&version={_apiVersion}");

            Link linkToSelf = links.Single(x => x.Relation == Self);
            linkToSelf.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={BloodPressuresController.EndpointName}&id={resource.Id}&version={_apiVersion}");

            Link linkToDelete = links.Single(x => x.Relation == "delete-bloodpressure");
            linkToSelf.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={BloodPressuresController.EndpointName}&id={resource.Id}&version={_apiVersion}");
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

        [Theory]
        [MemberData(nameof(MediatorReturnsErrorCases))]
        public async Task GivenMediatorReturnsError_Controller_ReturnsAssociatedResponse(CreateCommandResult mediatorResult, Expression<Func<IActionResult, bool>> actionResultExpectation)
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreateBloodPressureInfoForPatientIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<BloodPressureInfo, CreateCommandResult>(mediatorResult));

            // Act
            IActionResult actionResult = await _sut.PostBloodPressure(Guid.NewGuid(), new NewBloodPressureModel())
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .Match(actionResultExpectation);
        }

        [Fact]
        public async Task GivenModel_Post_Create_PatientResource()
        {
            // Arrange
            NewPatientInfo newPatient = new NewPatientInfo
            {
                Name = "Solomon Grundy"
            };

            MeasuresApiOptions apiOptions = new MeasuresApiOptions { DefaultPageSize = 25, MaxPageSize = 10 };
            _apiOptionsMock.Setup(mock => mock.Value).Returns(apiOptions);
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreatePatientInfoCommand>(), It.IsAny<CancellationToken>()))
                .Returns((CreatePatientInfoCommand cmd, CancellationToken ct) =>
                {
                    return Task.FromResult(new PatientInfo
                    {
                        Name = cmd.Data.Name,
                        BirthDate = cmd.Data.BirthDate,
                        Id = Guid.NewGuid()
                    });
                });

            // Act
            IActionResult actionResult = await _sut.Post(newPatient, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<CreatePatientInfoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            CreatedAtRouteResult createdAtRouteResult = actionResult.Should()
                .BeAssignableTo<CreatedAtRouteResult>().Which;

            Browsable<PatientInfo> browsablePatientInfo = createdAtRouteResult.Value.Should()
                                                                                    .BeAssignableTo<Browsable<PatientInfo>>().Which;

            PatientInfo resource = browsablePatientInfo.Resource;
            resource.Should()
                .NotBeNull();

            createdAtRouteResult.RouteName.Should()
                .Be(RouteNames.DefaultGetOneByIdApi);
            createdAtRouteResult.RouteValues.Should()
                                            .Contain("id", resource.Id, "resource id must be provided in routeValues");


            IEnumerable<Link> links = browsablePatientInfo.Links;
            links.Should().NotBeNullOrEmpty().And
                          .NotContainNulls().And
                          .NotContain(link => string.IsNullOrWhiteSpace(link.Href), $"each resource link must provide its {nameof(Link.Href)}").And
                          .NotContain(link => string.IsNullOrWhiteSpace(link.Method), $"each resource link must provide its {nameof(Link.Method)}").And
                          .NotContain(link => string.IsNullOrWhiteSpace(link.Relation), $"each resource link must provide its {nameof(Link.Relation)}").And
                          .Contain(link => link.Relation == Self).And
                          .Contain(link => link.Relation == "bloodpressures");

            Link linkToSelf = links.Single(link => link.Relation == Self);
            linkToSelf.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={PatientsController.EndpointName}&id={resource.Id}&version={_apiVersion}");
            linkToSelf.Method.Should()
                .Be("GET");

            Link linkToBloodPressures = links.Single(link => link.Relation == "bloodpressures");
            linkToBloodPressures.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={apiOptions.DefaultPageSize}&patientId={resource.Id}&version={_apiVersion}");
            linkToSelf.Method.Should()
                .Be("GET");
        }

        [Fact]
        public async Task Patch_UnknownEntity_Returns_NotFound()
        {
            JsonPatchDocument<PatientInfo> changes = new JsonPatchDocument<PatientInfo>();
            changes.Replace(x => x.Name, string.Empty);

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, PatientInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Failed_NotFound);

            // Act
            IActionResult actionResult = await _sut.Patch(Guid.NewGuid(), changes)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_Valid_Resource_Returns_NoContentResult()
        {
            // Arrange
            JsonPatchDocument<PatientInfo> changes = new JsonPatchDocument<PatientInfo>();
            changes.Replace(x => x.Name, string.Empty);

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, PatientInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Done);

            // Act
            IActionResult actionResult = await _sut.Patch(Guid.NewGuid(), changes)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<PatchCommand<Guid, PatientInfo>>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }

        [Theory]
        [InlineData(10, 5)]
        [InlineData(20, 10)]
        public async Task GetAllMeasures_returns_NotFound_when_Command_execution_returns_None(int pageSizeSubmitted, int maxPageSize)
        {
            Guid patientId = Guid.NewGuid();
            const string measure = "blood-pressures";
            int page = _faker.Random.Int(min: 1);

            _apiOptionsMock.Setup(mock => mock.Value)
                           .Returns(new MeasuresApiOptions { DefaultPageSize = 30, MaxPageSize = maxPageSize });
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfMeasuresInfoByPatientIdQuery>(),
                                                  It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Option.None<Page<GenericMeasureInfo>>());

            // Act
            ActionResult<GenericPageModel<Browsable<GenericMeasureModel>>> actionResult = await _sut.GetMeasures(patientId, measure, page, pageSizeSubmitted, default)
                                                                                                           .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfMeasuresInfoByPatientIdQuery>(query => query.Id != Guid.Empty
                                                                                                         && query.Data.patientId == patientId
                                                                                                         && query.Data.name == measure
                                                                                                         && query.Data.pagination.Page == page && query.Data.pagination.PageSize <= maxPageSize),
                                                   It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _apiOptionsMock.VerifyNoOtherCalls();

            actionResult.Result.Should()
                               .BeOfType<NotFoundResult>();
        }

        [Theory]
        [InlineData(10, 5)]
        [InlineData(20, 10)]
        public async Task GetAllMeasures_returns_PartialObjectResult_when_Command_execution_returns_one_page_out_of_several(int pageSizeSubmitted, int maxPageSize)
        {
            Guid patientId = Guid.NewGuid();
            const string measure = "blood-pressures";
            int page = _faker.Random.Int(min: 1);

            Faker<GenericMeasureInfo> faker = new Faker<GenericMeasureInfo>()
                .RuleFor(measure => measure.PatientId, () => patientId)
                .RuleFor(measure => measure.FormId, () => Guid.NewGuid())
                .RuleFor(measure => measure.DateOfMeasure, f => f.Date.Recent())
                .RuleFor(measure => measure.Id, () => Guid.NewGuid())
                .RuleFor(measure => measure.Data, f => new Dictionary<string, object>
                {
                    ["systolic"] = f.Random.Float(min: 9, max: 15),
                    ["diastolic"] = f.Random.Float(max: 9)
                });

            _apiOptionsMock.Setup(mock => mock.Value)
                           .Returns(new MeasuresApiOptions { DefaultPageSize = 30, MaxPageSize = maxPageSize });
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfMeasuresInfoByPatientIdQuery>(),
                                                  It.IsAny<CancellationToken>()))
                         .ReturnsAsync((GetPageOfMeasuresInfoByPatientIdQuery query, CancellationToken _) => Option.Some(new Page<GenericMeasureInfo>(faker.Generate(10), 200, query.Data.pagination.PageSize)));

            // Act
            ActionResult<GenericPageModel<Browsable<GenericMeasureModel>>> actionResult = await _sut.GetMeasures(patientId, measure, page, pageSizeSubmitted, default)
                                                                                                           .ConfigureAwait(false);

            // Assert
            int realPageSize = Math.Min(pageSizeSubmitted, maxPageSize);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfMeasuresInfoByPatientIdQuery>(query => query.Id != Guid.Empty
                                                                                                         && query.Data.patientId == patientId
                                                                                                         && query.Data.name == measure
                                                                                                         && query.Data.pagination.Page == page && query.Data.pagination.PageSize == realPageSize),
                                                   It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _apiOptionsMock.VerifyNoOtherCalls();

            GenericPageModel<Browsable<GenericMeasureModel>> response = actionResult.Result.Should()
                                                                                   .BeOfType<PartialObjectResult>().Which
                                                                                   .Value.Should().BeOfType<GenericPageModel<Browsable<GenericMeasureModel>>>().Which;

            response.Items.Should()
                          .NotBeEmpty().And
                          .NotContainNulls().And
                          .OnlyContain(item => item.Links != null).And
                          .OnlyContain(item => item.Links.Once(link => link.Relation == "patient"
                                                                       && link.Href.Equals($"http://host/api/{RouteNames.DefaultGetOneByIdApi}/?controller=Patients&id={patientId}&version={_apiVersion}"))).And
                          .OnlyContain(item => item.Links.Once(link => link.Relation == Self
                                                                       && link.Href.Equals($"http://host/api/{RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi}/?controller=Patients&measure={measure}&measureId={item.Resource.Id}&patientId={patientId}&version={_apiVersion}")));

            PageLinksModel pageLinks = response.Links;

            response.Links.Should()
                          .NotBeNull();
            Link first = response.Links.First;
            first.Should()
                 .NotBeNull();
            first.Href.Should()
                      .BeEquivalentTo($"http://host/api/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?controller=Patients&id={patientId}&measure={measure}&page=1&pageSize={realPageSize}&version={_apiVersion}");
            first.Relation.Should()
                          .Be(First);

            Link previous = response.Links.Previous;
            previous.Should()
                 .NotBeNull();
            previous.Href.Should()
                      .BeEquivalentTo($"http://host/api/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?controller=Patients&id={patientId}&measure={measure}&page={(page - 1)}&pageSize={realPageSize}&version={_apiVersion}");
            previous.Relation.Should()
                          .Be(Previous);

            Link last = response.Links.Last;
            last.Should()
                 .NotBeNull();
            last.Href.Should()
                      .BeEquivalentTo($"http://host/api/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?controller=Patients&id={patientId}&measure={measure}&page={response.Total}&pageSize={realPageSize}&version={_apiVersion}");
            last.Relation.Should()
                          .Be(Last);

        }

        [Theory]
        [InlineData(10, 5)]
        [InlineData(20, 10)]
        public async Task GetAllMeasures_returns_200Ok_when_Command_execution_returns_the_whole_content(int pageSizeSubmitted, int maxPageSize)
        {
            Guid patientId = Guid.NewGuid();
            const string measure = "blood-pressures";
            int page = 1;

            Faker<GenericMeasureInfo> faker = new Faker<GenericMeasureInfo>()
                .RuleFor(measure => measure.PatientId, () => patientId)
                .RuleFor(measure => measure.FormId, () => Guid.NewGuid())
                .RuleFor(measure => measure.DateOfMeasure, f => f.Date.Recent())
                .RuleFor(measure => measure.Id, () => Guid.NewGuid())
                .RuleFor(measure => measure.Data, f => new Dictionary<string, object>
                {
                    ["systolic"] = f.Random.Float(min: 9, max: 15),
                    ["diastolic"] = f.Random.Float(max: 9)
                });

            int realPageSize = Math.Min(pageSizeSubmitted, maxPageSize);
            _apiOptionsMock.Setup(mock => mock.Value)
                           .Returns(new MeasuresApiOptions { DefaultPageSize = 30, MaxPageSize = maxPageSize });
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfMeasuresInfoByPatientIdQuery>(),
                                                  It.IsAny<CancellationToken>()))
                         .ReturnsAsync((GetPageOfMeasuresInfoByPatientIdQuery query, CancellationToken _) => Option.Some(new Page<GenericMeasureInfo>(faker.Generate(1), 1, query.Data.pagination.PageSize)));

            // Act
            ActionResult<GenericPageModel<Browsable<GenericMeasureModel>>> actionResult = await _sut.GetMeasures(patientId, measure, page, pageSizeSubmitted, default)
                                                                                                    .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfMeasuresInfoByPatientIdQuery>(query => query.Id != Guid.Empty
                                                                                                         && query.Data.patientId == patientId
                                                                                                         && query.Data.name == measure
                                                                                                         && query.Data.pagination.Page == page && query.Data.pagination.PageSize == realPageSize),
                                                   It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _apiOptionsMock.VerifyNoOtherCalls();

            actionResult.Value.Should().NotBeNull();
            GenericPageModel<Browsable<GenericMeasureModel>> response = actionResult.Value;

            response.Items.Should()
                          .NotBeEmpty().And
                          .NotContainNulls().And
                          .OnlyContain(item => item.Links != null).And
                          .OnlyContain(item => item.Links.Once(link => link.Relation == "patient"
                                                                       && link.Href.Equals($"http://host/api/{RouteNames.DefaultGetOneByIdApi}/?controller=Patients&id={patientId}&version={_apiVersion}"))).And
                          .OnlyContain(item => item.Links.Once(link => link.Relation == Self
                                                                       && link.Href.Equals($"http://host/api/{RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi}/?controller=Patients&measure={measure}&measureId={item.Resource.Id}&patientId={patientId}&version={_apiVersion}")));

            PageLinksModel pageLinks = response.Links;

            response.Links.Should()
                          .NotBeNull();
            Link first = response.Links.First;
            first.Should()
                 .NotBeNull();
            first.Href.Should()
                      .BeEquivalentTo($"http://host/api/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?controller=Patients&id={patientId}&measure={measure}&page=1&pageSize={realPageSize}&version={_apiVersion}");
            first.Relation.Should()
                          .Be(First);

            response.Links.Previous.Should()
                    .BeNull();
            response.Links.Next.Should()
                               .BeNull();

            Link last = response.Links.Last;
            last.Should()
                 .NotBeNull();
            last.Href.Should()
                      .BeEquivalentTo($"http://host/api/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?controller=Patients&id={patientId}&measure={measure}&page=1&pageSize={realPageSize}&version={_apiVersion}");
            last.Relation.Should()
                          .Be(Last);

        }

        [Fact]
        public async Task GetOneMeasureByPatientId_returns_404NotFound_when_query_returns_none()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            Guid measureId = Guid.NewGuid();
            const string measure = "heartbeat";

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneMeasureInfoByPatientIdQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Option.None<GenericMeasureInfo>());

            // Act
            ActionResult<Browsable<GenericMeasureModel>> response = await _sut.GetOneMeasurementByPatientId(patientId, measure, measureId, default)
                                                                              .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneMeasureInfoByPatientIdQuery>(query => query.Id != null
                                                                                                     && query.Data.patientId == patientId
                                                                                                     && query.Data.name == measure
                                                                                                     && query.Data.measureId == measureId),
                                                   It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            response.Result.Should()
                           .BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetOneMeasureByPatientId_returns_200OK_when_query_returns_something()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            Guid measureId = Guid.NewGuid();
            const string measure = "heartbeat";

            GenericMeasureInfo measureInfo = new GenericMeasureInfo
            {
                FormId = Guid.NewGuid(),
                CreatedDate = _faker.Date.Recent(),
                Data = new Dictionary<string, object>
                {
                    ["prop"] = _faker.Random.Float(),
                    ["comments"] = _faker.Lorem.Words()
                },
                DateOfMeasure = _faker.Date.Recent(),
                Id = measureId,
                PatientId = patientId,
                UpdatedDate = _faker.Date.Recent()
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneMeasureInfoByPatientIdQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Option.Some(measureInfo));

            // Act
            ActionResult<Browsable<GenericMeasureModel>> actionResult = await _sut.GetOneMeasurementByPatientId(patientId, measure, measureId, default)
                                                                                  .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneMeasureInfoByPatientIdQuery>(query => query.Id != null
                                                                                                     && query.Data.patientId == patientId
                                                                                                     && query.Data.name == measure
                                                                                                     && query.Data.measureId == measureId),
                                                   It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            Browsable<GenericMeasureModel> browsableMeasureModel = actionResult.Value;

            browsableMeasureModel.Should()
                                 .NotBeNull();

            GenericMeasureModel resource = browsableMeasureModel.Resource;

            resource.Should().NotBeNull();
            resource.Id.Should().Be(measureInfo.Id);
            resource.PatientId.Should().Be(measureInfo.PatientId);
            resource.DateOfMeasure.Should().Be(measureInfo.DateOfMeasure);
            resource.Values.Should().ContainKeys(measureInfo.Data.Keys);

            foreach (KeyValuePair<string, object> kv in resource.Values)
            {
                kv.Value.Should().Be(measureInfo.Data[kv.Key]);
            }

            IEnumerable<Link> links = browsableMeasureModel.Links;
            links.Should()
                 .NotBeNull().And
                 .NotContainNulls().And
                 .ContainSingle(link => link.Relation == Self).And
                 .ContainSingle(link => link.Relation == "patient");

            Link self = links.Single(link => link.Relation == Self);
            self.Href.Should()
                     .BeEquivalentTo($"http://host/api/{RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi}/?controller=Patients&id={patientId}&measure={measure}&measureId={measureId}&version={_apiVersion}");

            Link patient = links.Single(link => link.Relation == "patient");
            patient.Href.Should()
                        .BeEquivalentTo($"http://host/api/{RouteNames.DefaultGetOneByIdApi}/?controller=Patients&id={patientId}&version={_apiVersion}");
        }

    }
}