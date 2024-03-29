namespace Measures.API.Tests.Features.v1.BloodPressures
{
    using AutoMapper.QueryableExtensions;
    using Bogus;
    using DataFilters;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using Measures.API.Features.v1.BloodPressures;
    using Measures.API.Features.v1.Patients;
    using Measures.API.Routing;
    using Measures.DataStores;
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
    using static Moq.MockBehavior;
    using static Newtonsoft.Json.JsonConvert;
    using static System.StringComparison;
    using static MedEasy.RestObjects.LinkRelation;
    using NodaTime.Testing;
    using NodaTime;
    using NodaTime.Extensions;
    using Measures.Ids;
    using MedEasy.Ids;

    /// <summary>
    /// Unit tests for <see cref="BloodPressuresController"/>
    /// </summary>
    [UnitTest]
    [Feature("Blood pressures")]
    [Feature("Measures")]
    public class BloodPressuresControllerTests : IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresStore>>
    {
        private readonly ITestOutputHelper _outputHelper;

        private readonly IUnitOfWorkFactory _uowFactory;
        private static readonly MeasuresApiOptions ApiOptions = new() { DefaultPageSize = 30, MaxPageSize = 200 };
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<LinkGenerator> _urlHelperMock;
        private readonly Mock<IOptionsSnapshot<MeasuresApiOptions>> _apiOptionsMock;
        private readonly BloodPressuresController _controller;
        private const string BaseUrl = "http://host/api";

        public BloodPressuresControllerTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<MeasuresStore> database)
        {
            _outputHelper = outputHelper;

            _urlHelperMock = new Mock<LinkGenerator>(Strict);
            _urlHelperMock.Setup(mock => mock.GetPathByAddress(It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<PathString>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
                .Returns((string routename, RouteValueDictionary routeValues, PathString _, FragmentString _, LinkOptions _)
                => $"{BaseUrl}/{routename}/?{routeValues?.ToQueryString((string _, object value) => (value as StronglyTypedId<Guid>)?.Value ?? value)}");

            _apiOptionsMock = new Mock<IOptionsSnapshot<MeasuresApiOptions>>(Strict);

            _uowFactory = new EFUnitOfWorkFactory<MeasuresStore>(database.OptionsBuilder.Options, (options) =>
            {
                MeasuresStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _mediatorMock = new Mock<IMediator>(Strict);

            _controller = new BloodPressuresController(_urlHelperMock.Object, _apiOptionsMock.Object, _mediatorMock.Object);
        }

        public static IEnumerable<object[]> GetAllTestCases
        {
            get
            {
                int[] pageSizes = { 1, 10, 500 };
                int[] pages = { 1, 10, 500 };
                Faker faker = new();
                foreach (int pageSize in pageSizes)
                {
                    foreach (int page in pages)
                    {
                        yield return new object[]
                        {
                            Enumerable.Empty<Subject>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            (
                                firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null
                                    && x.Relation == First
                                    && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={Math.Min(pageSize, ApiOptions.MaxPageSize) }".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={Math.Min(pageSize, ApiOptions.MaxPageSize)}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        };
                    }
                }

                Faker<BloodPressure> bloodPressureFaker = new Faker<BloodPressure>()
                    .CustomInstantiator(_ => new BloodPressure(
                            id: BloodPressureId.New(),
                            subjectId: SubjectId.New(),
                            dateOfMeasure: 10.April(2016).Add(13.Hours().And(48.Minutes())).AsUtc().ToInstant(),
                            systolicPressure: 120, diastolicPressure: 80
                        ));

                {
                    Subject subject = new(SubjectId.New(), faker.Person.FullName, faker.Person.DateOfBirth.ToLocalDateTime().Date);

                    foreach (BloodPressure measure in bloodPressureFaker.Generate(400))
                    {
                        subject.AddBloodPressure(BloodPressureId.New(), 10.April(2016).Add(13.Hours().And(48.Minutes())).AsUtc().ToInstant(), systolic: 120, diastolic: 80);
                    }
                    yield return new object[]
                    {
                        new [] { subject },
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == First
                                && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }
                {
                    Subject subject = new(SubjectId.New(), faker.Person.FullName, faker.Person.DateOfBirth.ToLocalDateTime().Date);
                    IEnumerable<BloodPressure> items = bloodPressureFaker.Generate(400);
                    items.ForEach((measure) => subject.AddBloodPressure(measure.Id, measure.DateOfMeasure, systolic: measure.SystolicPressure, measure.DiastolicPressure));

                    yield return new object[]
                    {
                        new []{ subject },
                        10, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First  && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }
                {
                    Subject subject = new(SubjectId.New(), faker.Person.FullName, faker.Person.DateOfBirth.ToLocalDateTime().Date);
                    subject.AddBloodPressure(BloodPressureId.New(),
                                             10.April(2016).Add(13.Hours().And(48.Minutes())).AsUtc().ToInstant(),
                                             systolic: 120,
                                             diastolic: 80);
                    yield return new object[]
                    {
                        new [] { subject },
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        1,    //expected total
                        (
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == First
                                && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                            lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == Last
                                && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)) // expected link to last page
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Subject> items, int pageSize, int page,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) pageLinksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(BloodPressuresController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"store items count: {items.SelectMany(x => x.BloodPressures).OfType<BloodPressure>().Count()}");

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(ApiOptions);

            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoQuery>(), It.IsAny<CancellationToken>()))
                    .Returns((GetPageOfBloodPressureInfoQuery query, CancellationToken _) =>
                    {
                        PaginationConfiguration pagination = query.Data;
                        Expression<Func<BloodPressure, BloodPressureInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                        _outputHelper.WriteLine($"Selector : {selector}");

                        IEnumerable<BloodPressure> measures = items.SelectMany(x => x.BloodPressures).OfType<BloodPressure>();

                        _outputHelper.WriteLine($"Measures count : {measures.Count()}");

                        IEnumerable<BloodPressureInfo> results = measures.Select(selector.Compile())
                            .Skip(pagination.PageSize * (pagination.Page == 1 ? 0 : pagination.Page - 1))
                            .Take(pagination.PageSize)
                            .ToArray();

                        return Task.FromResult(new Page<BloodPressureInfo>(results, measures.Count(), pagination.PageSize));
                    });

            // Act
            IActionResult actionResult = await _controller.Get(page: page, pageSize: pageSize)
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfBloodPressureInfoQuery>(cmd => cmd.Data.Page == page && cmd.Data.PageSize == Math.Min(pageSize, ApiOptions.MaxPageSize)), It.IsAny<CancellationToken>()), Times.Once,
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
                Faker<Subject> patientFaker = new Faker<Subject>()
                    .CustomInstantiator(faker =>
                    {
                        Subject subject = new(SubjectId.New(), faker.Person.FullName, faker.Person.DateOfBirth.ToLocalDateTime().Date);

                        subject.ChangeNameTo(faker.Person.FullName);

                        return subject;
                    });
                Faker<BloodPressure> bloodPressureFaker = new Faker<BloodPressure>()
                    .CustomInstantiator(faker =>
                    {
                        return new BloodPressure(
                            id: BloodPressureId.New(),
                            subjectId: SubjectId.New(),
                            dateOfMeasure: faker.Noda().Instant.Between(start: 1.January(2001).AsUtc().ToInstant(), end: 31.January(2001).AsUtc().ToInstant()),
                            diastolicPressure: 80,
                            systolicPressure: 120
                        );
                    });

                {
                    IEnumerable<BloodPressure> items = bloodPressureFaker.Generate(400);
                    Subject subject = patientFaker.Generate();
                    foreach (BloodPressure measure in items)
                    {
                        subject.AddBloodPressure(measure.Id, measure.DateOfMeasure, measure.SystolicPressure, measure.DiastolicPressure);
                    }

                    yield return new object[]
                    {
                        new [] { subject },
                        new SearchBloodPressureInfo
                        {
                            From = 1.January(2001).AsUtc().ToInstant().InUtc(),
                            To = 31.January(2001).AsUtc().ToInstant().InUtc(),
                            Page = 1, PageSize = 30
                        },
                        (maxPageSize : 200, defaultPageSize : 30),
                        (
                            count : 400,
                            items :
                            (Expression<Func<IEnumerable<Browsable<BloodPressureInfo>>, bool>>)(resources =>
                                resources.All(x =>1.January(2001).AsUtc().ToInstant() <= x.Resource.DateOfMeasure && x.Resource.DateOfMeasure <= 31.January(2001).AsUtc().ToInstant() ))
                            ,
                            links :
                            (
                                firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                    && x.Relation == First
                                    && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2001-01-01T00:00:00Z&page=1&pageSize={PaginationConfiguration.DefaultPageSize}&to=2001-01-31T00:00:00Z".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Next && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2001-01-01T00:00:00Z&page=2&pageSize={PaginationConfiguration.DefaultPageSize}&to=2001-01-31T00:00:00Z".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                                lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2001-01-01T00:00:00Z&page=14&pageSize={PaginationConfiguration.DefaultPageSize}&to=2001-01-31T00:00:00Z".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        )
                    };
                }

                {
                    Subject subject = patientFaker.Generate();
                    subject.AddBloodPressure(BloodPressureId.New(),
                                             dateOfMeasure: 23.June(2012).Add(10.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                                             diastolic: 80,
                                             systolic: 120);
                    yield return new object[]
                    {
                        new [] { subject },
                        new SearchBloodPressureInfo { From = 23.June(2012).AsUtc().ToInstant().InUtc(), Page = 1, PageSize = 30 }, // request
                        (maxPageSize: 200, pageSize: 30),
                        (
                            count: 1,
                            items:
                              (Expression<Func<IEnumerable<Browsable<BloodPressureInfo>>, bool>>)(resources =>
                                resources.All(x => 23.June(2012).AsUtc().ToInstant() <= x.Resource.DateOfMeasure)),
                            links: (
                                firstPageUrlExpectation: (Expression<Func<Link, bool>>)(x => x != null
                                                                                             && x.Relation.Contains(First)
                                                                                             && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2012-06-23T00:00:00Z&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpectation: (Expression<Func<Link, bool>>)(x => x == null), // expected link to previous page
                                nextPageUrlExpectation: (Expression<Func<Link, bool>>)(x => x == null), // expected link to next page
                                lastPageUrlExpectation: (Expression<Func<Link, bool>>)(x => x != null
                                                                                            && x.Relation.Contains(Last)
                                                                                            && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2012-06-23T00:00:00Z&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)) // expected link to last page
                            )
                        )
                    };
                }

                {
                    Subject subject = patientFaker.Generate();
                    subject.AddBloodPressure(BloodPressureId.New(),
                                             dateOfMeasure: 23.June(2012).Add(10.Hours().And(30.Minutes())).AsUtc().ToInstant(),
                                             systolic: 120,
                                             diastolic: 80);

                    yield return new object[]
                    {
                        new [] { subject },
                        new SearchBloodPressureInfo { SubjectId = subject.Id }, // request
                        (maxPageSize : 200, pageSize : 30),
                        (
                            count : 1,
                            items :
                              (Expression<Func<IEnumerable<Browsable<BloodPressureInfo>>, bool>>)(resources =>
                                resources.Exactly(1) && resources.All(x => x.Resource.SubjectId == subject.Id)),
                            links : (
                                firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains(First) && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize=30&subjectId={subject.Id.Value}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains(Last) && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize=30&subjectId={subject.Id.Value}".Equals(x.Href, OrdinalIgnoreCase)) // expected link to last page
                            )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchTestCases))]
        [Feature("Search")]
        public async Task Search(IEnumerable<Subject> patients, SearchBloodPressureInfo searchQuery,
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
            _outputHelper.WriteLine($"store items count: {patients.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Subject>().Clear();
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
                uow.Repository<Subject>().Create(patients);
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
                    .OnlyContain(x => x.Links.Once(link => link.Relation == Self));
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
                    new SearchBloodPressureInfo { Page = 2, PageSize = 30, From = 31.July(2013).AsUtc().ToInstant().InUtc() },
                    (maxPageSize : 30, defaultPageSize : 30),
                    new [] { new Subject( SubjectId.New(), "Starr", 18.August(1983).AsLocal().ToLocalDateTime().Date) },
                    "page index is not 1 and there's no result for the search query"
                };

                {
                    Subject subject = new(SubjectId.New(), "Homelander", 18.August(1983).AsUtc().ToLocalDateTime().Date);
                    subject.AddBloodPressure(BloodPressureId.New(), 22.January(1987).AsUtc().ToInstant(), systolic: 120, diastolic: 80);

                    yield return new object[]
                    {
                        new SearchBloodPressureInfo { Page = 2, PageSize = 30 },
                        (maxPageSize : 30, defaultPageSize : 30),
                        new [] {
                            subject
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
            IEnumerable<Subject> patients, string reason)
        {
            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Subject>().Create(patients);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _apiOptionsMock.Setup(mock => mock.Value)
                .Returns(new MeasuresApiOptions { MaxPageSize = apiOptions.maxPageSize, DefaultPageSize = apiOptions.defaultPageSize });
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<BloodPressureInfo>>(), It.IsAny<CancellationToken>()))
                .Returns((SearchQuery<BloodPressureInfo> request, CancellationToken cancellationToken) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    SearchQueryInfo<BloodPressureInfo> search = request.Data;
                    Expression<Func<BloodPressure, bool>> filter = search.Filter?.ToExpression<BloodPressure>() ?? (_ => true);
                    Expression<Func<BloodPressure, BloodPressureInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();

                    return uow.Repository<BloodPressure>()
                              .WhereAsync(
                                    selector,
                                    filter,
                                    search.Sort,
                                    search.PageSize,
                                    search.Page,
                                    cancellationToken)
                              .AsTask();
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
            BloodPressureId idToDelete = BloodPressureId.New();
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
            BloodPressureId measureId = BloodPressureId.New();

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Subject subject = new(SubjectId.New(), "Bruce Wayne", 12.December(1953).ToLocalDateTime().Date);

                subject.AddBloodPressure(
                        measureId,
                        dateOfMeasure: 24.April(1997).AsUtc().ToInstant(),
                        systolic: 150,
                        diastolic: 90
                );
                uow.Repository<Subject>().Create(subject);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetBloodPressureInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetBloodPressureInfoByIdQuery query, CancellationToken cancellationToken) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    Expression<Func<BloodPressure, BloodPressureInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();

                    return uow.Repository<BloodPressure>()
                              .SingleOrDefaultAsync(
                                    selector,
                                    (BloodPressure x) => x.Id == query.Data,
                                    cancellationToken)
                              .AsTask();
                });

            // Act
            ActionResult<Browsable<BloodPressureInfo>> actionResult = await _controller.Get(measureId)
                                                                                       .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetBloodPressureInfoByIdQuery>(q => q.Data == measureId), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            actionResult.Value.Should()
                              .NotBeNull();

            Browsable<BloodPressureInfo> browsableResource = actionResult.Value;

            browsableResource.Links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href)).And
                .ContainSingle(x => x.Relation == Self).And
                .ContainSingle(x => x.Relation == "delete").And
                .ContainSingle(x => x.Relation == "subject");

            Link self = browsableResource.Links.Single(x => x.Relation == Self);
            self.Method.Should()
                .Be("GET");

            BloodPressureInfo resource = browsableResource.Resource;
            self.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={BloodPressuresController.EndpointName}&{nameof(resource.Id)}={resource.Id.Value}");

            Link delete = browsableResource.Links.Single(x => x.Relation == "delete");
            delete.Method.Should()
                .Be("DELETE");
            delete.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={BloodPressuresController.EndpointName}&{nameof(resource.Id)}={resource.Id.Value}");

            Link linkToPatient = browsableResource.Links.Single(x => x.Relation == "subject");
            linkToPatient.Method.Should()
                .Be("GET");
            linkToPatient.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={SubjectsController.EndpointName}&{nameof(SubjectInfo.Id)}={resource.SubjectId.Value}");

            resource.Id.Should().Be(measureId);
            resource.SystolicPressure.Should().Be(150);
            resource.DiastolicPressure.Should().Be(90);
            resource.SubjectId.Should()
                .NotBe(SubjectId.Empty);
        }

        [Fact]
        public async Task Get_UnknonwnId_Returns_NotFound()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetBloodPressureInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<BloodPressureInfo>());

            // Act
            ActionResult<Browsable<BloodPressureInfo>> actionResult = await _controller.Get(BloodPressureId.New())
                                                                                       .ConfigureAwait(false);

            // Assert
            actionResult.Result.Should()
                               .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteResource()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteBloodPressureInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            BloodPressureId idToDelete = BloodPressureId.New();
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
            BloodPressureId idToDelete = BloodPressureId.New();
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
            JsonPatchDocument<BloodPressureInfo> changes = new();
            changes.Replace(x => x.SystolicPressure, 120);

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<BloodPressureId, BloodPressureInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Failed_NotFound);

            // Act
            IActionResult actionResult = await _controller.Patch(BloodPressureId.New(), changes)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_Valid_Resource_Returns_NoContentResult()
        {
            // Arrange
            JsonPatchDocument<BloodPressureInfo> changes = new();
            changes.Replace(x => x.DiastolicPressure, 90);

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<BloodPressureId, BloodPressureInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Done);

            // Act
            IActionResult actionResult = await _controller.Patch(BloodPressureId.New(), changes)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<PatchCommand<BloodPressureId, BloodPressureInfo>>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }
    }
}
