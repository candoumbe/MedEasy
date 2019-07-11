using AutoMapper.QueryableExtensions;
using Bogus;
using DataFilters;
using FluentAssertions;
using FluentAssertions.Extensions;
using Measures.API.Features.BloodPressures;
using Measures.API.Features.Patients;
using Measures.API.Routing;
using Measures.Context;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Queries.BloodPressures;
using Measures.DTO;
using Measures.Mapping;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.DTO.Search;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
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
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;
using static System.StringComparison;

namespace Measures.API.Tests.Features.BloodPressures
{
    /// <summary>
    /// Unit tests for <see cref="BloodPressuresController"/>
    /// </summary>
    [UnitTest]
    [Feature("Blood pressures")]
    [Feature("Measures")]
    public class BloodPressuresControllerTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;

        private IUnitOfWorkFactory _uowFactory;
        private static readonly MeasuresApiOptions _apiOptions = new MeasuresApiOptions { DefaultPageSize = 30, MaxPageSize = 200 };
        private Mock<IMediator> _mediatorMock;
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<IOptionsSnapshot<MeasuresApiOptions>> _apiOptionsMock;
        private BloodPressuresController _controller;
        private const string _baseUrl = "http://host/api";

        public BloodPressuresControllerTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string routename, object routeValues) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            _apiOptionsMock = new Mock<IOptionsSnapshot<MeasuresApiOptions>>(Strict);

            DbContextOptionsBuilder<MeasuresContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MeasuresContext>();
            dbContextOptionsBuilder.UseSqlite(database.Connection)
                .EnableSensitiveDataLogging();

            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbContextOptionsBuilder.Options, (options) =>
            {
                MeasuresContext context = new MeasuresContext(options);
                context.Database.EnsureCreated();
                return context;
            });

            _mediatorMock = new Mock<IMediator>(Strict);

            _controller = new BloodPressuresController(_urlHelperMock.Object, _apiOptionsMock.Object, _mediatorMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _uowFactory = null;
            _urlHelperMock = null;
            _apiOptionsMock = null;
            _mediatorMock = null;
            _controller = null;
        }

        public static IEnumerable<object[]> GetAllTestCases
        {
            get
            {
                int[] pageSizes = { 1, 10, 500 };
                int[] pages = { 1, 10, 500 };

                foreach (int pageSize in pageSizes)
                {
                    foreach (int page in pages)
                    {
                        yield return new object[]
                        {
                            Enumerable.Empty<BloodPressure>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            (
                                firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First  && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={Math.Min(pageSize, _apiOptions.MaxPageSize) }".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={Math.Min(pageSize, _apiOptions.MaxPageSize)}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        };
                    }
                }

                Faker<Patient> patientFaker = new Faker<Patient>()
                    .CustomInstantiator(faker =>
                    {
                        Patient patient = new Patient(Guid.NewGuid());

                        patient.ChangeNameTo(faker.Person.FullName);

                        return patient;
                    });

                Faker<BloodPressure> bloodPressureFaker = new Faker<BloodPressure>()
                    .CustomInstantiator(faker => new BloodPressure(
                            Guid.NewGuid(),
                            patientId: Guid.NewGuid(),
                            dateOfMeasure: 10.April(2016).Add(13.Hours(48.Minutes())),
                            systolicPressure: 120, diastolicPressure: 80
                        ));
                {
                    IEnumerable<BloodPressure> items = bloodPressureFaker.Generate(400);
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }
                {
                    IEnumerable<BloodPressure> items = bloodPressureFaker.Generate(400);
                    items.ForEach((item, pos) =>
                    {

                        item.Patient = new Patient(Guid.NewGuid());
                    });

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First  && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }

                yield return new object[]
                {
                    new [] {
                        new BloodPressure(
                            Guid.NewGuid(),
                            patientId : Guid.NewGuid(),
                            dateOfMeasure: 10.April(2016).Add(13.Hours(48.Minutes())),
                            systolicPressure : 120, diastolicPressure : 80
                        )
                    },
                    PaginationConfiguration.DefaultPageSize, 1, // request
                    1,    //expected total
                    (
                        firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                        nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                        lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)) // expected link to last page
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<BloodPressure> items, int pageSize, int page,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) pageLinksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(BloodPressuresController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(_apiOptions);

            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetPageOfBloodPressureInfoQuery query, CancellationToken cancellationToken) =>
                {
                    PaginationConfiguration pagination = query.Data;
                    Func<BloodPressure, BloodPressureInfo> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>().Compile();
                    _outputHelper.WriteLine($"Selector : {selector}");

                    IEnumerable<BloodPressureInfo> results = items.Select(selector)
                        .ToArray();

                    results = results.Skip(pagination.PageSize * (pagination.Page == 1 ? 0 : pagination.Page - 1))
                         .Take(pagination.PageSize)
                         .ToArray();

                    return Task.FromResult(new Page<BloodPressureInfo>(results, items.Count(), pagination.PageSize));
                });

            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { PageSize = pageSize, Page = page })
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfBloodPressureInfoQuery>(cmd => cmd.Data.Page == page && cmd.Data.PageSize == Math.Min(pageSize, _apiOptions.MaxPageSize)), It.IsAny<CancellationToken>()), Times.Once,
                "Controller must cap pageSize of the query before sending it to the mediator");

            GenericPagedGetResponse<Browsable<BloodPressureInfo>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                        .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<GenericPagedGetResponse<Browsable<BloodPressureInfo>>>().Which;

            response.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            response.Total.Should()
                    .Be(expectedCount, $@"because the ""{nameof(GenericPagedGetResponse<Browsable<BloodPressureInfo>>)}.{nameof(GenericPagedGetResponse<Browsable<BloodPressureInfo>>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageLinksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageLinksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageLinksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageLinksExpectation.lastPageUrlExpectation);
        }

        public static IEnumerable<object[]> SearchTestCases
        {
            get
            {
                Faker<Patient> patientFaker = new Faker<Patient>()
                    .CustomInstantiator(faker =>
                    {
                        Patient patient = new Patient(Guid.NewGuid());

                        patient.ChangeNameTo(faker.Person.FullName);

                        return patient;
                    });
                Faker<BloodPressure> bloodPressureFaker = new Faker<BloodPressure>()
                    .CustomInstantiator(faker =>
                    {
                        BloodPressure measure = new BloodPressure(
                            Guid.NewGuid(),
                            patientId: Guid.NewGuid(),
                            dateOfMeasure: faker.Date.Between(start: 1.January(2001), end: 31.January(2001)),
                            diastolicPressure : 80,
                            systolicPressure : 120
                        );

                        return measure;
                    });

                {
                    IEnumerable<BloodPressure> items = bloodPressureFaker.Generate(400);


                    yield return new object[]
                    {
                        items,
                        new SearchBloodPressureInfo
                        {
                            From = 1.January(2001),
                            To = 31.January(2001),
                            Page = 1, PageSize = 30
                        },
                        (maxPageSize : 200, defaultPageSize : 30),
                        (
                            count : 400,
                            items :
                            (Expression<Func<IEnumerable<Browsable<BloodPressureInfo>>, bool>>)(resources =>
                                resources.All(x =>1.January(2001) <= x.Resource.DateOfMeasure && x.Resource.DateOfMeasure <= 31.January(2001) ))
                            ,
                            links :
                            (
                                firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                    && x.Relation == LinkRelation.First
                                    && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2001-01-01T00:00:00&page=1&pageSize={PaginationConfiguration.DefaultPageSize}&to=2001-01-31T00:00:00".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2001-01-01T00:00:00&page=2&pageSize={PaginationConfiguration.DefaultPageSize}&to=2001-01-31T00:00:00".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                                lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2001-01-01T00:00:00&page=14&pageSize={PaginationConfiguration.DefaultPageSize}&to=2001-01-31T00:00:00".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        )
                    };
                }

                yield return new object[]
                {
                    new [] {
                        new BloodPressure(

                            id: Guid.NewGuid(),
                            patientId : Guid.NewGuid(),
                            dateOfMeasure : 23.June(2012).Add(10.Hours().Add(30.Minutes())),
                            diastolicPressure : 80,
                            systolicPressure : 120
                        )
                    },
                    new SearchBloodPressureInfo { From = 23.June(2012), Page = 1, PageSize = 30 }, // request
                    (maxPageSize: 200, pageSize: 30),
                    (
                        count: 1,
                        items:
                          (Expression<Func<IEnumerable<Browsable<BloodPressureInfo>>, bool>>)(resources =>
                            resources.All(x => 23.June(2012) <= x.Resource.DateOfMeasure)),
                        links: (
                            firstPageUrlExpectation: (Expression<Func<Link, bool>>)(x => x != null && x.Relation.Contains(LinkRelation.First) && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2012-06-23T00:00:00&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpectation: (Expression<Func<Link, bool>>)(x => x == null), // expected link to previous page
                            nextPageUrlExpectation: (Expression<Func<Link, bool>>)(x => x == null), // expected link to next page
                            lastPageUrlExpectation: (Expression<Func<Link, bool>>)(x => x != null && x.Relation.Contains(LinkRelation.Last) && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2012-06-23T00:00:00&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)) // expected link to last page
                        )
                    )
                };

                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new [] {
                            new BloodPressure(Guid.NewGuid(), patientId, dateOfMeasure: 23.June(2012).Add(10.Hours().Add(30.Minutes()) ), systolicPressure:120, diastolicPressure: 80)
                        },
                        new SearchBloodPressureInfo { PatientId = patientId }, // request
                        (maxPageSize : 200, pageSize : 30),
                        (
                            count : 1,
                            items :
                              (Expression<Func<IEnumerable<Browsable<BloodPressureInfo>>, bool>>)(resources =>
                                resources.Count() == 1 && resources.All(x => x.Resource.PatientId == patientId)),
                            links : (
                                firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains(LinkRelation.First) && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize=30&patientId={patientId}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains(LinkRelation.Last) && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize=30&patientId={patientId}".Equals(x.Href, OrdinalIgnoreCase)) // expected link to last page
                            )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchTestCases))]
        [Feature("Search")]
        public async Task Search(IEnumerable<BloodPressure> items, SearchBloodPressureInfo searchQuery,
            (int maxPageSize, int defaultPageSize) apiOptions,
            (
                int count,
                Expression<Func<IEnumerable<Browsable<BloodPressureInfo>>, bool>> items,
                (
                    Expression<Func<Link, bool>> firstPageUrlExpectation,
                    Expression<Func<Link, bool>> previousPageUrlExpectation,
                    Expression<Func<Link, bool>> nextPageUrlExpectation,
                    Expression<Func<Link, bool>> lastPageUrlExpectation
                ) links
            ) pageExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(BloodPressuresController.Search)}({nameof(SearchBloodPressureInfo)})");
            _outputHelper.WriteLine($"Search : {SerializeObject(searchQuery)}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<BloodPressure>().Clear();
                uow.Repository<Patient>().Clear();
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
                uow.Repository<BloodPressure>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = apiOptions.defaultPageSize, MaxPageSize = apiOptions.maxPageSize });

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<BloodPressureInfo>>(), It.IsAny<CancellationToken>()))
                .Returns((SearchQuery<BloodPressureInfo> query, CancellationToken ct) =>
                    new HandleSearchQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder).Search<BloodPressure, BloodPressureInfo>(query, ct)
                );

            // Act
            IActionResult actionResult = await _controller.Search(searchQuery)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<SearchQuery<BloodPressureInfo>>(), It.IsAny<CancellationToken>()), Times.Once);
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.AtLeastOnce, $"because {nameof(BloodPressuresController)}.{nameof(BloodPressuresController.Search)} must always check that {nameof(SearchBloodPressureInfo.PageSize)} don't exceed {nameof(MeasuresApiOptions.MaxPageSize)} value");

            GenericPagedGetResponse<Browsable<BloodPressureInfo>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<GenericPagedGetResponse<Browsable<BloodPressureInfo>>>().Which;

            response.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null).And
                .NotContain(x => !x.Links.Any()).And
                .Match(pageExpectation.items);

            if (response.Items.Any())
            {
                response.Items.Should()
                    .OnlyContain(x => x.Links.Once(link => link.Relation == LinkRelation.Self));
            }

            response.Total.Should()
                    .Be(pageExpectation.count, $@"the ""{nameof(GenericPagedGetResponse<Browsable<BloodPressureInfo>>)}.{nameof(GenericPagedGetResponse<Browsable<BloodPressureInfo>>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageExpectation.links.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageExpectation.links.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageExpectation.links.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageExpectation.links.lastPageUrlExpectation);
        }

        public static IEnumerable<object[]> OutOfBoundSearchCases
        {
            get
            {
                yield return new object[]
                {
                    new SearchBloodPressureInfo { Page = 2, PageSize = 30, From = 31.July(2013) },
                    (maxPageSize : 30, defaultPageSize : 30),
                    Enumerable.Empty<BloodPressure>(),
                    "page index is not 1 and there's no result for the search query"
                };

                {
                    yield return new object[]
                    {
                        new SearchBloodPressureInfo { Page = 2, PageSize = 30 },
                        (maxPageSize : 30, defaultPageSize : 30),
                        new [] {
                            new BloodPressure(Guid.NewGuid(), Guid.NewGuid(), 22.January(1987), diastolicPressure : 80, systolicPressure: 120)
                        },
                        "page index is not 1 and there's no result for the search query"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(OutOfBoundSearchCases))]
        [Feature("Search")]
        public async Task Search_With_OutOfBound_PagingConfiguration_Returns_NotFound(
            SearchBloodPressureInfo query,
            (int maxPageSize, int defaultPageSize) apiOptions,
            IEnumerable<BloodPressure> measures, string reason)
        {
            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<BloodPressure>().Create(measures);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _apiOptionsMock.Setup(mock => mock.Value)
                .Returns(new MeasuresApiOptions { MaxPageSize = apiOptions.maxPageSize, DefaultPageSize = apiOptions.defaultPageSize });
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<BloodPressureInfo>>(), It.IsAny<CancellationToken>()))
                .Returns(async (SearchQuery<BloodPressureInfo> request, CancellationToken cancellationToken) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        SearchQueryInfo<BloodPressureInfo> search = request.Data;
                        Expression<Func<BloodPressure, bool>> filter = search.Filter?.ToExpression<BloodPressure>() ?? (_ => true);
                        Expression<Func<BloodPressure, BloodPressureInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();

                        return await uow.Repository<BloodPressure>()
                            .WhereAsync(
                                selector,
                                filter,
                                search.Sort,
                                search.PageSize,
                                search.Page,
                                cancellationToken)
                            .ConfigureAwait(false);
                    }
                });
            // Act
            IActionResult actionResult = await _controller.Search(query)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>(reason);
        }

        [Fact]
        public async Task Delete_Returns_NoContent()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<DeleteBloodPressureInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            Guid idToDelete = Guid.NewGuid();
            IActionResult actionResult = await _controller.Delete(idToDelete)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NoContentResult>();

            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<DeleteBloodPressureInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteBloodPressureInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Get_Returns_The_Element()
        {
            // Arrange
            Guid measureId = Guid.NewGuid();

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Patient patient = new Patient(Guid.NewGuid());
                patient.ChangeNameTo("Bruce Wayne");

                BloodPressure measure = new BloodPressure(
                    Guid.NewGuid(),
                    patient.Id,
                    dateOfMeasure: 24.April(1997),
                    systolicPressure : 150,
                    diastolicPressure : 90
                    
                );
                uow.Repository<BloodPressure>().Create(measure);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetBloodPressureInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetBloodPressureInfoByIdQuery query, CancellationToken cancellationToken) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Expression<Func<BloodPressure, BloodPressureInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                        Option<BloodPressureInfo> result = await uow.Repository<BloodPressure>()
                            .SingleOrDefaultAsync(
                                selector,
                                (BloodPressure x) => x.Id == query.Data,
                                cancellationToken)
                            .ConfigureAwait(false);

                        return result;
                    }
                });

            // Act
            IActionResult actionResult = await _controller.Get(measureId)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetBloodPressureInfoByIdQuery>(q => q.Data == measureId), It.IsAny<CancellationToken>()), Times.Once);

            Browsable<BloodPressureInfo> browsableResource = actionResult.Should()
                .BeAssignableTo<OkObjectResult>().Which
                .Value.Should()
                .BeAssignableTo<Browsable<BloodPressureInfo>>().Which;

            browsableResource.Links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href)).And
                .ContainSingle(x => x.Relation == LinkRelation.Self).And
                .ContainSingle(x => x.Relation == "delete").And
                .ContainSingle(x => x.Relation == "patient");

            Link self = browsableResource.Links.Single(x => x.Relation == LinkRelation.Self);
            self.Method.Should()
                .Be("GET");

            BloodPressureInfo resource = browsableResource.Resource;
            self.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={BloodPressuresController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            Link delete = browsableResource.Links.Single(x => x.Relation == "delete");
            delete.Method.Should()
                .Be("DELETE");
            delete.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={BloodPressuresController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            Link linkToPatient = browsableResource.Links.Single(x => x.Relation == "patient");
            linkToPatient.Method.Should()
                .Be("GET");
            linkToPatient.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={resource.PatientId}");

            resource.Id.Should().Be(measureId);
            resource.SystolicPressure.Should().Be(150);
            resource.DiastolicPressure.Should().Be(90);
            resource.PatientId.Should()
                .NotBeEmpty();
        }

        [Fact]
        public async Task Get_UnknonwnId_Returns_NotFound()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetBloodPressureInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<BloodPressureInfo>());

            // Act
            IActionResult actionResult = await _controller.Get(Guid.NewGuid())
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteResource()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteBloodPressureInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            Guid idToDelete = Guid.NewGuid();
            IActionResult actionResult = await _controller.Delete(idToDelete)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeleteBloodPressureInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteBloodPressureInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async Task Delete_Unknown_Resource_Returns_Not_Found()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteBloodPressureInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Failed_NotFound);

            // Act
            Guid idToDelete = Guid.NewGuid();
            IActionResult actionResult = await _controller.Delete(idToDelete)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeleteBloodPressureInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteBloodPressureInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_UnknownEntity_Returns_NotFound()
        {
            JsonPatchDocument<BloodPressureInfo> changes = new JsonPatchDocument<BloodPressureInfo>();
            changes.Replace(x => x.SystolicPressure, 120);

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, BloodPressureInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Failed_NotFound);

            // Act
            IActionResult actionResult = await _controller.Patch(Guid.NewGuid(), changes)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_Valid_Resource_Returns_NoContentResult()
        {
            // Arrange
            JsonPatchDocument<BloodPressureInfo> changes = new JsonPatchDocument<BloodPressureInfo>();
            changes.Replace(x => x.DiastolicPressure, 90);

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, BloodPressureInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Done);

            // Act
            IActionResult actionResult = await _controller.Patch(Guid.NewGuid(), changes)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<PatchCommand<Guid, BloodPressureInfo>>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }
    }
}
