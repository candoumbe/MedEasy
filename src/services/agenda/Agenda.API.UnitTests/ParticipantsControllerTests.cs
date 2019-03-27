using Agenda.API.Resources;
using Agenda.API.Routing;
using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Agenda.Mapping;
using Agenda.Objects;
using AutoMapper.QueryableExtensions;
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using MediatR;
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
using DataFilters;
using static MedEasy.RestObjects.LinkRelation;


using static Moq.MockBehavior;
using static System.StringComparison;

namespace Agenda.API.UnitTests
{
    [UnitTest]
    [Feature("Agenda")]
    public class ParticipantsControllerTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUrlHelper> _urlHelperMock;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<IOptionsSnapshot<AgendaApiOptions>> _apiOptionsMock;
        private Mock<IMediator> _mediatorMock;
        private ParticipantsController _sut;
        private const string _baseUrl = "agenda";

        public ParticipantsControllerTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string routename, object routeValues) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            DbContextOptionsBuilder<AgendaContext> dbOptions = new DbContextOptionsBuilder<AgendaContext>();
            dbOptions.UseSqlite(database.Connection);
            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(dbOptions.Options, (options) =>
            {
                AgendaContext dbContext = new AgendaContext(options);
                dbContext.Database.EnsureCreated();

                return dbContext;
            });

            _apiOptionsMock = new Mock<IOptionsSnapshot<AgendaApiOptions>>(Strict);

            _mediatorMock = new Mock<IMediator>(Strict);
            _sut = new ParticipantsController(urlHelper: _urlHelperMock.Object, mediator: _mediatorMock.Object, apiOptions: _apiOptionsMock.Object);
            _outputHelper = outputHelper;
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Clear();

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _uowFactory = null;
            _outputHelper = null;
            _urlHelperMock = null;
            _apiOptionsMock = null;
            _mediatorMock = null;
            _sut = null;
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
                            Enumerable.Empty<Participant>(), // Current store state
                            (pageSize, page), // request,
                            (defaultPageSize : 30, maxPageSize : 200),
                            0,    //expected total
                            (
                                first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First
                                    && ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={ParticipantsController.EndpointName}" +
                                    "&page=1" +
                                    $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                next :(Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last
                                    && ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={ParticipantsController.EndpointName}" +
                                    "&page=1" +
                                    $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase))
                            )  // expected link to last page
                        };
                    }
                }

                Faker<Participant> participantFaker = new Faker<Participant>()
                    .CustomInstantiator(faker => new Participant(faker.Person.FullName))
                    .RuleFor(participant => participant.Id, 0)
                    .RuleFor(participant => participant.UUID, () => Guid.NewGuid())
                    .RuleFor(participant => participant.Email, faker => faker.Internet.Email())
                    .RuleFor(participant => participant.PhoneNumber, faker => faker.Person.Phone);

                IEnumerable<Participant> items = participantFaker.Generate(20);

                yield return new object[]
                {
                    items,
                    (pageSize : 5, page : 1), // request
                    (defaultPageSize : 30, maxPageSize : 200),
                    20,    //expected total
                    (
                        first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=1&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                        next : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=2&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                        last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=4&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))
                    )  // expected link to last page
                };

                yield return new object[]
                {
                    items,
                    (pageSize : 5, page : 4), // request
                    (defaultPageSize : 30, maxPageSize : 200),
                    20,    //expected total
                    (
                        first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=1&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        previous : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Previous && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=3&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        next : (Expression<Func<Link, bool>>) (x => x == null ), // expected link to next page
                        last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=4&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))
                    )  // expected link to last page
                };

                yield return new object[]
               {
                    items,
                    (pageSize : 5, page : 2), // request
                    (defaultPageSize : 30, maxPageSize : 200),
                    20,    //expected total
                    (
                        first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=1&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        previous : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Previous && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=1&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        next : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=3&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=4&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))
                    )  // expected link to last page
               };
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Participant> items, (int pageSize, int page) request,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(ParticipantsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"{nameof(request)}{nameof(request.pageSize)}: {request.pageSize}");
            _outputHelper.WriteLine($"{nameof(request)}{nameof(request.page)}: {request.page}");
            _outputHelper.WriteLine($"items count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfParticipantInfoQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetPageOfParticipantInfoQuery query, CancellationToken ct) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Expression<Func<Participant, ParticipantInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Participant, ParticipantInfo>();
                        return await uow.Repository<Participant>()
                            .ReadPageAsync(
                                selector,
                                query.Data.PageSize,
                                query.Data.Page,
                                new Sort<ParticipantInfo>(nameof(ParticipantInfo.UpdatedDate)).ToOrderClause(),
                                ct)
                            .ConfigureAwait(false);
                    }
                });

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new AgendaApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            IActionResult actionResult = await _sut.Get(page: request.page, pageSize: request.pageSize, ct: default)
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"{nameof(ParticipantsController)}.{nameof(ParticipantsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(AgendaApiOptions.MaxPageSize)} value");
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfParticipantInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            GenericPagedGetResponse<Browsable<ParticipantInfo>> response = okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<GenericPagedGetResponse<Browsable<ParticipantInfo>>>().Which;

            _outputHelper.WriteLine($"response : {response}");

            response.Items.Should()
                .NotBeNull();

            if (response.Items.Any())
            {
                response.Items.Should()
                    .NotContainNulls().And
                    .OnlyContain(x => x.Resource != null, "resource must not be null").And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == Self), "links must contain only self relation");
            }

            response.Total.Should()
                    .Be(expectedCount, $@"the ""{nameof(GenericPagedGetResponse<ParticipantInfo>)}.{nameof(GenericPagedGetResponse<ParticipantInfo>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(linksExpectation.lastPageUrlExpectation);
        }

        [Fact]
        public async Task WhenMediatorReturnsNotFound_GetById_ReturnsNotFoundResult()
        {
            // Arrange
            Guid participantId = Guid.NewGuid();

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<IRequest<Option<ParticipantInfo>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<ParticipantInfo>());

            // Act
            IActionResult actionResult = await _sut.Get(id: participantId, ct: default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();

            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IRequest<Option<ParticipantInfo>>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WhenMediatorReturnsNotFound_GetAppointmentsByParticipantId_Returns_NotFound()
        {
            // Arrange
            Guid participantId = Guid.NewGuid();

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<IRequest<Option<IEnumerable<AppointmentInfo>>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<IEnumerable<AppointmentInfo>>());

            // Act
            ActionResult<IEnumerable<Browsable<AppointmentInfo>>> actionResult = await _sut.Planning(id: participantId, from: 1.January(2019), to: 31.January(2019), ct: default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Result.Should()
                .NotBeNull().And
                .BeOfType<NotFoundResult>();

            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IRequest<Option<IEnumerable<AppointmentInfo>>>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPlanningByParticipantIdQuery>(query =>
                query.Data.participantId == participantId
                && query.Data.start == 1.January(2019) && query.Data.end == 31.January(2019)), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WhenMediatorReturnsData_GetAppointmentsByParticipantId_Returns_Results()
        {
            // Arrange
            Guid participantId = Guid.NewGuid();
            Faker<AppointmentInfo> appointmentInfo = new Faker<AppointmentInfo>()
                .RuleFor(x => x.Id, () => Guid.NewGuid())
                .RuleFor(x => x.StartDate, () => 13.January(2010).Add(14.Hours()))
                .RuleFor(x => x.EndDate, (_, current) => current.StartDate.Add(1.Hours()))
                .RuleFor(x => x.CreatedDate, (faker) => faker.Date.Recent())
                .RuleFor(x => x.Participants, _ => new[] {
                    new ParticipantInfo {Name = "Hugo strange", Id = participantId },
                    new ParticipantInfo {Name = "Joker", Id = Guid.NewGuid()}
                })
                ;

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<IRequest<Option<IEnumerable<AppointmentInfo>>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.Some(appointmentInfo.GenerateLazy(count : 10)));

            // Act
            ActionResult<IEnumerable<Browsable<AppointmentInfo>>> actionResult = await _sut.Planning(id: participantId, from: 1.January(2019), to: 31.January(2019), ct: default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Value.Should()
                .NotBeNull().And
                .BeAssignableTo<IEnumerable<Browsable<AppointmentInfo>>>();

            IEnumerable<Browsable<AppointmentInfo>> appointments = actionResult.Value;
            appointments.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .OnlyContain(browsable => browsable.Resource != default).And
                .OnlyContain(browsable => browsable.Links.Once()).And
                .OnlyContain(browsable => browsable.Links.Once(link => link.Relation == Self));

            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IRequest<Option<IEnumerable<AppointmentInfo>>>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPlanningByParticipantIdQuery>(query =>
                query.Data.participantId == participantId
                && query.Data.start == 1.January(2019) && query.Data.end == 31.January(2019)), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();
        }

        public static IEnumerable<object[]> SearchCases
        {
            get
            {
                (int defaultPageSize, int maxPageSize) pagingOptions = (10, 200);
                {
                    SearchParticipantInfo searchInfo = new SearchParticipantInfo
                    {
                        Name = "bruce",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-Name"
                    };

                    yield return new object[]
                    {
                        Page<ParticipantInfo>.Empty,
                        searchInfo,
                        pagingOptions,
                        0,
                        (
                        (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={ParticipantsController.EndpointName}" +
                                $"&name={searchInfo.Name}"+
                                "&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        (Expression<Func<Link, bool>>)(previous => previous == null),
                        (Expression<Func<Link, bool>>)(next => next == null),
                        (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={ParticipantsController.EndpointName}" +
                                $"&name={searchInfo.Name}"+
                                "&page=1" +
                                "&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase)))
                    };
                }
                {
                    SearchParticipantInfo searchInfo = new SearchParticipantInfo
                    {
                        Name = "!*Wayne",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-Name"
                    };
                    yield return new object[]
                    {
                        new Page<ParticipantInfo> (
                            entries: new []
                            {
                                new ParticipantInfo { Name = "Bruce Wayne" }
                            },
                            total : 1,
                            size : searchInfo.PageSize),
                        searchInfo,
                        pagingOptions,
                        1,
                        (
                           (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={ParticipantsController.EndpointName}" +
                                $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                "&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase)),
                            (Expression<Func<Link, bool>>)(previous => previous == null),
                            (Expression<Func<Link, bool>>)(next => next == null),
                            (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={ParticipantsController.EndpointName}" +
                                    $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                    "&page=1&pageSize=30" +
                                    $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))
                        )
                    };
                }
                {
                    SearchParticipantInfo searchInfo = new SearchParticipantInfo
                    {
                        Name = "Bruce",
                        Page = 1,
                        PageSize = 30,
                    };
                    yield return new object[]
                    {
                        new Page<ParticipantInfo> (
                            entries: new []
                            {
                                new ParticipantInfo { Name = "Bruce Wayne" }
                            },
                            total : 1,
                            size : searchInfo.PageSize),
                        searchInfo,
                        pagingOptions,
                        1,
                        (
                            (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == First
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={ParticipantsController.EndpointName}" +
                                    $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                    "&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            (Expression<Func<Link, bool>>)(previous => previous == null),
                            (Expression<Func<Link, bool>>)(next => next == null),
                            (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={ParticipantsController.EndpointName}" +
                                    $"&name={Uri.EscapeDataString(searchInfo.Name)}"+
                                    "&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(Page<ParticipantInfo> page, SearchParticipantInfo request,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(ParticipantsController)}({nameof(ParticipantsController.Search)})");
            _outputHelper.WriteLine($"{nameof(request)} : {request.Stringify()}");
            _outputHelper.WriteLine($"page of result returned by mediator : {page.Stringify()}");

            // Arrange

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchParticipantInfoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _apiOptionsMock.SetupGet(mock => mock.Value)
                .Returns(new AgendaApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            ActionResult<GenericPagedGetResponse<Browsable<ParticipantInfo>>> actionResult = await _sut.Search(request, ct: default)
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"{nameof(ParticipantsController)}.{nameof(ParticipantsController.Search)} must always check that {nameof(SearchParticipantInfo.PageSize)} don't exceed {nameof(AgendaApiOptions.MaxPageSize)} value");
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IRequest<Page<ParticipantInfo>>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            GenericPagedGetResponse<Browsable<ParticipantInfo>> response = actionResult.Value;

            _outputHelper.WriteLine($"response : {response}");

            response.Items.Should()
                .NotBeNull();

            if (response.Items.Any())
            {
                response.Items.Should()
                    .NotContainNulls().And
                    .OnlyContain(x => x.Resource != null, "resource must not be null").And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == Self), "links must contain only self relation");
            }

            response.Total.Should()
                    .Be(expectedCount, $@"the ""{nameof(GenericPagedGetResponse<ParticipantInfo>)}.{nameof(GenericPagedGetResponse<ParticipantInfo>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(linksExpectation.lastPageUrlExpectation);
        }

        public static IEnumerable<object[]> SearchReturnsNotFoundCases
        {
            get
            {
                (int defaultPageSize, int maxPageSize) pagingOptions = (defaultPageSize: 10, maxPageSize: 300);
                yield return new object[]
                {
                    Page<ParticipantInfo>.Empty,
                    pagingOptions,
                    new SearchParticipantInfo { Page = 2, Name = "Bruce*" },
                    (Expression<Func<PageLinks, bool>>)(pageLinks => pageLinks.First != null
                        && ($"agenda/{RouteNames.DefaultSearchResourcesApi}/?" +
                        $"controller={ParticipantsController.EndpointName}" +
                        $"&name={Uri.EscapeDataString("Bruce*")}" +
                        "&page=1&pageSize=30").Equals(pageLinks.First.Href, OrdinalIgnoreCase)
                    ),
                    "The result is empty so there's no page with index 2"
                };
            }
        }

        [Theory]
        [MemberData(nameof(SearchReturnsNotFoundCases))]
        public async Task Search_Returns_NotFound_When_PageIndex_Exceed_PageCount(Page<ParticipantInfo> page, (int defaultPageSize, int maxPageSize) pagingOptions, SearchParticipantInfo query,  Expression<Func<PageLinks, bool>> resultExpectation, string reason)
        {
            // Arrange

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchParticipantInfoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _apiOptionsMock.SetupGet(mock => mock.Value)
                .Returns(new AgendaApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            ActionResult<GenericPagedGetResponse<Browsable<ParticipantInfo>>> actionResult = await _sut.Search(query, default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Result.Should()
                .BeAssignableTo<NotFoundObjectResult>().Which.Value.Should()
                .BeAssignableTo<PageLinks>().Which.Should()
                .Match(resultExpectation, reason);
        }
    }
}
