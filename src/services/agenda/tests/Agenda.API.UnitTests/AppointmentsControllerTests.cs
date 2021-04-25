using Agenda.API.Resources.v1;
using Agenda.API.Resources.v1.Appointments;
using Agenda.API.Routing;
using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Agenda.Mapping;
using Agenda.Models.v1.Appointments;
using Agenda.Models.v1.Attendees;
using Agenda.Objects;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bogus;
using DataFilters;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
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
using static Newtonsoft.Json.Formatting;
using static Newtonsoft.Json.JsonConvert;
using static System.StringComparison;
using static MedEasy.RestObjects.LinkRelation;
using NodaTime.Extensions;
using NodaTime.Testing;
using NodaTime;
using System.Text.Json;
using NodaTime.Serialization.SystemTextJson;
using Agenda.Ids;
using MedEasy.Ids;

namespace Agenda.API.UnitTests.Features
{
    [Feature("Agenda")]
    [UnitTest]
    public class AppointmentsControllerTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<AgendaContext>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly SqliteEfCoreDatabaseFixture<AgendaContext> _database;
        private readonly Mock<LinkGenerator> _urlHelperMock;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly Mock<IOptionsSnapshot<AgendaApiOptions>> _apiOptionsMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly AppointmentsController _sut;
        private readonly FakeClock _clock;
        private const string _baseUrl = "agenda";
        private readonly Mock<IMapper> _mapperMock;
        private static readonly ApiVersion _apiVersion = new(1, 0);
        private readonly SqliteEfCoreDatabaseFixture<AgendaContext> _sqliteEfCoreDatabaseFixture;

        public AppointmentsControllerTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<AgendaContext> database)
        {
            _outputHelper = outputHelper;
            _database = database;
            _urlHelperMock = new Mock<LinkGenerator>(Strict);
            _urlHelperMock.Setup(mock => mock.GetPathByAddress(It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<PathString>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
                          .Returns((string routename, RouteValueDictionary routeValues, PathString _, FragmentString __, LinkOptions ___)
                          => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString((string ____, object value) => (value as StronglyTypedId<Guid>)?.Value ?? value)}");
            _clock = new FakeClock(new Instant());

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(database.OptionsBuilder.Options, (options) =>
            {
                AgendaContext context = new(options, _clock);
                context.Database.EnsureCreated();
                return context;
            });
            _database = database;
            _apiOptionsMock = new Mock<IOptionsSnapshot<AgendaApiOptions>>(Strict);

            _mediatorMock = new Mock<IMediator>(Strict);

            _mapperMock = new Mock<IMapper>(Strict);

            IMapper mapper = AutoMapperConfig.Build().CreateMapper();
            _mapperMock.Setup(mock => mock.Map<IEnumerable<AppointmentModel>>(It.IsAny<IEnumerable<AppointmentInfo>>()))
                .Returns((IEnumerable<AppointmentInfo> input) => mapper.Map<IEnumerable<AppointmentModel>>(input));
            _mapperMock.Setup(mock => mock.Map<AppointmentModel>(It.IsAny<AppointmentInfo>()))
                .Returns((AppointmentInfo input) => mapper.Map<AppointmentModel>(input));
            _mapperMock.Setup(mock => mock.Map<SearchAppointmentInfo>(It.IsAny<SearchAppointmentModel>()))
                .Returns((SearchAppointmentModel input) => mapper.Map<SearchAppointmentInfo>(input));

            _mapperMock.Setup(mock => mock.Map<NewAppointmentInfo>(It.IsAny<NewAppointmentModel>()))
                .Returns((NewAppointmentModel input) => mapper.Map<NewAppointmentInfo>(input));

            _sut = new AppointmentsController(_urlHelperMock.Object,
                                              _mediatorMock.Object,
                                              _apiOptionsMock.Object,
                                              _mapperMock.Object,
                                              _apiVersion);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Attendee>().Clear();
            uow.Repository<Appointment>().Clear();

            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                LinkGenerator[] urlHelperCases = { null, Mock.Of<LinkGenerator>() };
                IOptionsSnapshot<AgendaApiOptions>[] optionsCases = { null, Mock.Of<IOptionsSnapshot<AgendaApiOptions>>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };
                IMapper[] mapperCases = { null, Mock.Of<IMapper>() };
                ApiVersion[] apiVersionCases = { null, new ApiVersion(DateTime.UtcNow) };

                return urlHelperCases.CrossJoin(optionsCases, (urlHelper, options) => (urlHelper, options))
                                     .Select((tuple) => new { tuple.urlHelper, tuple.options })
                                     .CrossJoin(mediatorCases, (tuple, mediator) => (tuple.urlHelper, tuple.options, mediator))
                                     .Select((tuple) => new { tuple.urlHelper, tuple.options, tuple.mediator })
                                     .CrossJoin(mapperCases, (tuple, mapper) => (tuple.urlHelper, tuple.options, tuple.mediator, mapper))
                                     .Where(tuple => tuple.urlHelper == null || tuple.options == null || tuple.mediator == null || tuple.mapper == null)
                                     .Select(tuple => new { tuple.urlHelper, tuple.mediator, tuple.options, tuple.mapper })
                                     .CrossJoin(apiVersionCases, (tuple, apiVersion) => new { tuple.urlHelper, tuple.mediator, tuple.options, tuple.mapper, apiVersion })
                                     .Where(tuple => tuple.urlHelper == null || tuple.options == null || tuple.mediator == null || tuple.mapper == null || tuple.apiVersion == null)
                                     .Select(tuple => new object[] { tuple.urlHelper, tuple.mediator, tuple.options, tuple.mapper, tuple.apiVersion });
            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void GivenNullParameter_Ctor_ThrowsArgumentNullException(LinkGenerator urlHelper, IMediator mediator, IOptionsSnapshot<AgendaApiOptions> apiOptions, IMapper mapper, ApiVersion apiVersion)
        {
            _outputHelper.WriteLine($"{nameof(urlHelper)} is null : {urlHelper == null}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {mediator == null}");
            _outputHelper.WriteLine($"{nameof(apiOptions)} is null : {apiOptions == null}");
            _outputHelper.WriteLine($"{nameof(mapper)} is null : {mapper == null}");
            _outputHelper.WriteLine($"{nameof(apiVersion)} is null : {apiVersion == null}");

            // Act
            Action action = () => new AppointmentsController(urlHelper, mediator, apiOptions, mapper, apiVersion);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GivenNoDataFromMediator_GetAll_Returns_EmptyPage()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;

            _apiOptionsMock.Setup(mock => mock.Value).Returns(new AgendaApiOptions { MaxPageSize = 200, DefaultPageSize = 30 });

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfAppointmentInfoQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetPageOfAppointmentInfoQuery request, CancellationToken _) => Task.FromResult(Page<AppointmentInfo>.Empty(request.Data.PageSize)));


            // Act
            ActionResult<GenericPagedGetResponse<Browsable<AppointmentModel>>> actionResult = await _sut.Get(page: 1, pageSize: 10, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IRequest<Page<AppointmentInfo>>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfAppointmentInfoQuery>(q => q.Data.Page == page && q.Data.PageSize == pageSize), It.IsAny<CancellationToken>()), Times.Once);

            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _apiOptionsMock.VerifyNoOtherCalls();

            _mapperMock.Verify(mock => mock.Map<IEnumerable<AppointmentModel>>(It.IsAny<IEnumerable<AppointmentInfo>>()), Times.Once);
            _mapperMock.VerifyNoOtherCalls();

            actionResult.Should()
                .NotBeNull();

            GenericPagedGetResponse<Browsable<AppointmentModel>> response = actionResult.Value;

            response.Items.Should()
                .BeEmpty();

            PageLinks links = response.Links;
            links.Should()
                .NotBeNull();

            Link first = links.First;
            first.Should()
                .NotBeNull("Link to the first page of the query must be set");
            first.Method.Should().Be("GET");
            first.Relation.Should().Be(First);
            first.Href.Should().BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize={pageSize}&version={_apiVersion}");

            links.Previous.Should()
                .BeNull("there's no data");
            links.Next.Should()
                .BeNull("there's no data");

            links.Next.Should()
                .BeNull("there's no data");

            Link last = links.Last;
            last.Should()
                .NotBeNull("Link to the last page of the query must be set");
            last.Method.Should().Be("GET");
            last.Relation.Should().Be(Last);
            last.Href.Should()
                     .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize={pageSize}&version={_apiVersion}");
        }

        [Fact]
        public async Task GivenNoDataFromMediator_GetAppointmentById_Returns_NotFound()
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();

            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAppointmentInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<AppointmentInfo>());

            // Act
            ActionResult<Browsable<AppointmentModel>> actionResult = await _sut.Get(id: appointmentId, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IRequest<Option<AppointmentInfo>>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<GetOneAppointmentInfoByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            _mapperMock.Verify(mock => mock.Map<AppointmentModel>(It.IsAny<AppointmentInfo>()), Times.Never);
            _mapperMock.VerifyNoOtherCalls();

            actionResult.Result.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task GivenOneResultFromMediator_GetAppointmentById_Returns_Ok()
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();
            _clock.Reset(1.July(2013).Add(12.Seconds()).AsUtc().ToInstant());
            Appointment appointment = new(id: appointmentId,
                                          subject: "Confidential",
                                          location: "Wayne Tower",
                                          startDate: 12.July(2013).Add(14.Hours().And(30.Minutes()).And(10.Milliseconds())).AsUtc().ToInstant(),
                                          endDate: 12.July(2013).Add(14.Hours().And(45.Minutes()).And(15.Milliseconds())).AsUtc().ToInstant()

            );

            appointment.AddAttendee(new Attendee(id: AttendeeId.New(), name: "Bruce Wayne"));
            appointment.AddAttendee(new Attendee(id: AttendeeId.New(), name: "Dick Grayson"));

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneAppointmentInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetOneAppointmentInfoByIdQuery query, CancellationToken ct) =>
                {
                    _outputHelper.WriteLine($"Executing query : {query.Jsonify()}");
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    Expression<Func<Appointment, AppointmentInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Appointment, AppointmentInfo>();

                    _outputHelper.WriteLine($"selector : {selector}");

                    AppointmentInfo appointmentInfo = await uow.Repository<Appointment>()
                                                               .SingleAsync(selector,
                                                                            x => x.Id == query.Data,
                                                                            ct)
                                                               .ConfigureAwait(false);

                    return appointmentInfo.Some();
                });

            // Act
            ActionResult<Browsable<AppointmentModel>> actionResult = await _sut.Get(appointmentId, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetOneAppointmentInfoByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            Browsable<AppointmentModel> browsableResource = actionResult.Value;

            browsableResource.Should()
                .NotBeNull();

            AppointmentModel resource = browsableResource.Resource;
            resource.Id.Should()
                       .Be(appointmentId);
            resource.StartDate.Should()
                              .Be(appointment.StartDate.InUtc());
            resource.EndDate.Should()
                            .Be(appointment.EndDate.InUtc());
            resource.Attendees.Should()
                .HaveCount(appointment.Attendees.Count());

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .HaveCount(2).And
                .ContainSingle(link => link.Relation == Self).And
                .ContainSingle(link => link.Relation == "delete");

            Link selfLink = links.Single(link => link.Relation == Self);
            selfLink.Method.Should().Be("GET");
            selfLink.Href.Should().BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AppointmentsController.EndpointName}&id={appointmentId.Value}&version={_apiVersion}");

            Link deleteLink = links.Single(link => link.Relation == "delete");
            deleteLink.Method.Should().Be("DELETE");
            deleteLink.Href.Should().BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AppointmentsController.EndpointName}&id={appointmentId.Value}&version={_apiVersion}");
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
                            Enumerable.Empty<Appointment>(), // Current store state
                            new PaginationConfiguration{ Page = page, PageSize = pageSize }, // request,
                            (defaultPageSize : 30, maxPageSize : 200),
                            1.January(2000).AsUtc().ToInstant(),
                            0,    //expected total
                            (
                                first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={AppointmentsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={pageSize}" +
                                    $"&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                next :(Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={AppointmentsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={pageSize}" +
                                    $"&version={_apiVersion}").Equals(x.Href, OrdinalIgnoreCase))
                            )
                        };
                    }
                }

                Faker<Attendee> participantFaker = new Faker<Attendee>()
                    .CustomInstantiator(faker => new Attendee(AttendeeId.New(), faker.Person.FullName));

                {
                    Faker<Appointment> appointmentFaker = new Faker<Appointment>()
                        .RuleFor(appointment => appointment.EndDate, (_, appointment) => appointment.StartDate + 11.Hours().ToDuration())
                        .CustomInstantiator(_ => new(id: AppointmentId.New(),
                                                      subject: string.Empty,
                                                      location: string.Empty,
                                                      startDate: 10.January(2016).At(10.Hours()).AsUtc().ToInstant(),
                                                      endDate: 10.January(2016).At(10.Hours().And(30.Minutes())).AsUtc().ToInstant()))
                        .FinishWith((faker, appointment) =>
                        {
                            foreach (Attendee item in participantFaker.Generate(faker.Random.Int(min: 1, max: 5)))
                            {
                                appointment.AddAttendee(item);
                            }
                        });

                    IEnumerable<Appointment> items = appointmentFaker.Generate(20);

                    yield return new object[]
                    {
                        items,
                        new PaginationConfiguration{ Page = 1, PageSize = 5 }, // request,
                            (defaultPageSize : 30, maxPageSize : 200),
                        1.January(2015).AsUtc().ToInstant(),
                        20,    //expected total
                        (
                            first : (Expression<Func<Link, bool>>) (x => x != null
                                                                         && x.Relation == First
                                                                         && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=1&pageSize=5&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            next : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Next
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=2&pageSize=5&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            last : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Last
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=4&pageSize=5&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase))
                        )  // expected link to last page
                    };

                    yield return new object[]
                    {
                        items,
                        new PaginationConfiguration{ Page = 1, PageSize = 5 }, // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        1.January(2016).AsUtc().ToInstant(),
                        20,    //expected total
                        (
                            first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=1&pageSize=5&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            next : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=2&pageSize=5&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=4&pageSize=5&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase))
                        )  // expected link to last page
                    };

                    yield return new object[]
                    {
                        items,
                        new PaginationConfiguration{ Page = 1, PageSize = 5 }, // request,
                        (defaultPageSize : 30, maxPageSize : 200),
                        1.January(2017).AsUtc().ToInstant(),
                        0,    //expected total
                        (
                            first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=1&pageSize=5&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            next : (Expression<Func<Link, bool>>) (x => x == null ), // expected link to next page
                            last : (Expression<Func<Link, bool>>) (x => x != null
                                                                        && x.Relation == Last
                                                                        && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=1&pageSize=5&version={_apiVersion}".Equals(x.Href, OrdinalIgnoreCase))
                        )  // expected link to last page
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Appointment> items, PaginationConfiguration request,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            Instant now,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            JsonSerializerOptions jsonSerializerOptions = new();
            jsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            _outputHelper.WriteLine($"Testing {nameof(AppointmentsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"{nameof(items)} : {items.Jsonify(jsonSerializerOptions)}");
            _outputHelper.WriteLine($"items count: {items.Count()}");
            _outputHelper.WriteLine($"{nameof(request)}.{nameof(PaginationConfiguration.PageSize)}: {request.PageSize}");
            _outputHelper.WriteLine($"{nameof(request)}.{nameof(PaginationConfiguration.Page)}: {request.Page}");
            _outputHelper.WriteLine($"Current server dateTime : {now}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Clear();
                uow.Repository<Appointment>().Clear();
                uow.Repository<Appointment>().Create(items);
                await uow.SaveChangesAsync()
                            .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfAppointmentInfoQuery>(), It.IsAny<CancellationToken>()))
                                 .Returns(async (GetPageOfAppointmentInfoQuery query, CancellationToken ct) =>
                                 {
                                     using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                                     Expression<Func<Appointment, AppointmentInfo>> selector = AutoMapperConfig.Build()
                                                                                                       .ExpressionBuilder
                                                                                                       .GetMapExpression<Appointment, AppointmentInfo>();

                                     return await uow.Repository<Appointment>()
                                             .WhereAsync(
                                                 selector,
                                                 (Appointment x) => now <= x.EndDate,
                                                 new Sort<AppointmentInfo>(nameof(AppointmentInfo.StartDate)),
                                                 query.Data.PageSize,
                                                 query.Data.Page,
                                                 ct)
                                             .ConfigureAwait(false);
                                 });

            _apiOptionsMock.SetupGet(mock => mock.Value)
                           .Returns(new AgendaApiOptions
                           {
                               DefaultPageSize = pagingOptions.defaultPageSize,
                               MaxPageSize = pagingOptions.maxPageSize
                           });

            // Act
            ActionResult<GenericPagedGetResponse<Browsable<AppointmentModel>>> actionResult = await _sut.Get(page: request.Page, pageSize: request.PageSize)
                                                                                                        .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"{nameof(AppointmentsController)}.{nameof(AppointmentsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(AgendaApiOptions.MaxPageSize)} value");
            _apiOptionsMock.VerifyNoOtherCalls();

            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfAppointmentInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            _mapperMock.Verify(mock => mock.Map<IEnumerable<AppointmentModel>>(It.IsAny<IEnumerable<AppointmentInfo>>()), Times.Exactly(actionResult.Value != null ? 1 : 0));
            _mapperMock.VerifyNoOtherCalls();

            actionResult.Should()
                    .NotBeNull();

            GenericPagedGetResponse<Browsable<AppointmentModel>> response = actionResult.Value;

            _outputHelper.WriteLine($"response : {response}");

            response.Items.Should()
                          .NotBeNull();

            if (response.Items.AtLeastOnce())
            {
                response.Items.Should()
                    .NotContainNulls().And
                    .OnlyContain(x => x.Resource != null, "resource must not be null").And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == Self), "links must contain only self relation");
            }

            response.Links.First.Should().Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(linksExpectation.lastPageUrlExpectation);
            response.Total.Should()
                          .Be(expectedCount, $@"the ""{nameof(GenericPagedGetResponse<AppointmentInfo>)}.{nameof(GenericPagedGetResponse<AppointmentInfo>.Total)}"" property indicates the number of elements");
        }

        public static IEnumerable<object[]> SearchCases
        {
            get
            {
                yield return new object[]
                {
                    Page<AppointmentInfo>.Empty(10),
                    new SearchAppointmentModel { Page = 1, PageSize = 10},
                    (defaultPageSize : 20, maxPageSize : 50),
                    (
                        firstPageUrlExpectation : (Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                         && link.Relation == First
                                                                                         && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                        ),
                        previousPageUrl : (Expression<Func<Link, bool>>)(link => link == null),
                        nextPageUrl : (Expression<Func<Link, bool>>)(link => link == null),
                        lastPageUrlExpectation :(Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                       && link.Relation == Last
                                                                                       && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                        )
                    )
                };

                {
                    Faker<AppointmentInfo> appointmentFaker = new Faker<AppointmentInfo>()
                        .RuleFor(x => x.Id, AppointmentId.New())
                        .RuleFor(x => x.Location, faker => faker.Address.City())
                        .RuleFor(x => x.Subject, faker => faker.Lorem.Sentence(wordCount: 5))
                        .RuleFor(x => x.StartDate, faker => faker.Noda().Instant.Recent())
                        .RuleFor(x => x.EndDate, (faker, app) => app.StartDate + 30.Minutes().ToDuration())
                        .RuleFor(x => x.UpdatedDate, faker => faker.Noda().Instant.Recent());

                    yield return new object[]
                    {
                        new Page<AppointmentInfo>(appointmentFaker.Generate(10), total : 50, size :10),
                        new SearchAppointmentModel { Page = 1, PageSize = 10},
                        (defaultPageSize : 20, maxPageSize : 50),
                        (
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                             && link.Relation == First
                                                                                             && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                            ),
                            previousPageUrl : (Expression<Func<Link, bool>>)(link => link == null),
                            nextPageUrl : (Expression<Func<Link, bool>>)(link => link != null
                                                                                 && link.Method == "GET"
                                                                                 && link.Relation == Next
                                                                                 && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=2&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)),
                            lastPageUrlExpectation :(Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                           && link.Relation == Last
                                                                                           && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=5&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                            )
                        )
                    };

                    yield return new object[]
                    {
                        new Page<AppointmentInfo>(appointmentFaker.Generate(10), total : 50, size :10),
                        new SearchAppointmentModel { Page = 2, PageSize = 10},
                        (defaultPageSize : 20, maxPageSize : 50),
                        (
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                             && link.Relation == First
                                                                                             && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                            ),
                            previousPageUrl : (Expression<Func<Link, bool>>)(link => link != null
                                                                                     && link.Method == "GET"
                                                                                     && link.Relation == Previous
                                                                                     && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)),
                            nextPageUrl : (Expression<Func<Link, bool>>)(link => link != null
                                                                                 && link.Method == "GET"
                                                                                 && link.Relation == Next
                                                                                 && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=3&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)),
                            lastPageUrlExpectation :(Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                           && link.Relation == Last
                                                                                           && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=5&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                            )
                        )
                    };

                    yield return new object[]
                    {
                        new Page<AppointmentInfo>(appointmentFaker.Generate(10), total : 50, size :10),
                        new SearchAppointmentModel { Page = 5, PageSize = 10},
                        (defaultPageSize : 20, maxPageSize : 50),
                        (
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                             && link.Relation == First
                                                                                             && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                            ),
                            previousPageUrl : (Expression<Func<Link, bool>>)(link => link != null
                                                                                     && link.Method == "GET"
                                                                                     && link.Relation == Previous
                                                                                     && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=4&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)),
                            nextPageUrl : (Expression<Func<Link, bool>>)(link => link == null ),
                            lastPageUrlExpectation :(Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                           && link.Relation == Last
                                                                                           && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=5&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                            )
                        )
                    };

                    yield return new object[]
                    {
                        new Page<AppointmentInfo>(appointmentFaker.Generate(5), total : 45, size :10),
                        new SearchAppointmentModel { Page = 5, PageSize = 10},
                        (defaultPageSize : 20, maxPageSize : 50),
                        (
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                             && link.Relation == First
                                                                                             && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                            ),
                            previousPageUrl : (Expression<Func<Link, bool>>)(link => link != null
                                                                                     && link.Method == "GET"
                                                                                     && link.Relation == Previous
                                                                                     && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=4&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)),
                            nextPageUrl : (Expression<Func<Link, bool>>)(link => link == null ),
                            lastPageUrlExpectation :(Expression<Func<Link, bool>>)(link => link.Method == "GET"
                                                                                           && link.Relation == Last
                                                                                           && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=5&pageSize=10&version={_apiVersion}".Equals(link.Href, OrdinalIgnoreCase)
                            )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task GivenMediatorReturnsData_Search_ReturnsOk(Page<AppointmentInfo> page, SearchAppointmentModel search,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            _outputHelper.WriteLine($"Mediator response: {SerializeObject(page, Indented)}");

            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchAppointmentInfoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new AgendaApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            ActionResult<GenericPagedGetResponse<Browsable<AppointmentModel>>> actionResult = await _sut.Search(search, ct: default)
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"{nameof(AppointmentsController)}.{nameof(AppointmentsController.Search)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(AgendaApiOptions.MaxPageSize)} value");
            _apiOptionsMock.VerifyNoOtherCalls();

            _mediatorMock.Verify(mock => mock.Send(It.IsAny<SearchAppointmentInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<SearchAppointmentInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            _mapperMock.Verify(mock => mock.Map<SearchAppointmentInfo>(It.IsAny<SearchAppointmentModel>()), Times.Once);
            _mapperMock.Verify(mock => mock.Map<IEnumerable<AppointmentModel>>(It.IsAny<IEnumerable<AppointmentInfo>>()), Times.Once);
            _mapperMock.VerifyNoOtherCalls();

            GenericPagedGetResponse<Browsable<AppointmentModel>> response = actionResult.Value;

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
                    .Be(page.Total, $@"the ""{nameof(GenericPagedGetResponse<AppointmentModel>)}.{nameof(GenericPagedGetResponse<AppointmentModel>.Total)}"" property indicates the number of elements");

            response.Links.First.Should()
                .NotBeNull().And
                .Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should()
                .NotBeNull().And
                .Match(linksExpectation.lastPageUrlExpectation);
        }

        [Fact]
        public async Task GivenValidInfo_Post_Returns_CreatedResource()
        {
            // Arrange
            NewAppointmentModel newAppointment = new()
            {
                StartDate = 23.July(2012).Add(13.Hours().And(30.Minutes())).AsUtc().ToInstant().InUtc(),
                EndDate = 23.July(2012).Add(14.Hours().Add(30.Minutes())).AsUtc().ToInstant().InUtc(),
                Subject = "Confidential",
                Location = "Gotham",
                Attendees = new[]
                {
                    new AttendeeModel { Id = AttendeeId.New(), Name = "Dick Grayson", UpdatedDate = 10.January(2005).AsUtc().ToInstant() },
                    new AttendeeModel { Id = AttendeeId.New(), Name = "Bruce Wayne", UpdatedDate = 10.January(2005).AsUtc().ToInstant() }
                }
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreateAppointmentInfoCommand>(), It.IsAny<CancellationToken>()))
                .Returns((CreateAppointmentInfoCommand cmd, CancellationToken ct) =>
                {
                    return new HandleCreateAppointmentInfoCommand(_uowFactory, AutoMapperConfig.Build().CreateMapper())
                        .Handle(cmd, ct);
                });

            _outputHelper.WriteLine($"{nameof(newAppointment)} : {newAppointment}");

            // Act
            IActionResult actionResult = await _sut.Post(newAppointment, ct: default)
                                                   .ConfigureAwait(false);

            // Assert
            _mapperMock.Verify(mock => mock.Map<NewAppointmentInfo>(It.IsAny<NewAppointmentModel>()), Times.Once);
            _mapperMock.Verify(mock => mock.Map<NewAppointmentInfo>(It.Is<NewAppointmentModel>(input => input == newAppointment)), Times.Once);
            _mapperMock.VerifyNoOtherCalls();

            CreatedAtRouteResult createdAtRouteResult = actionResult.Should()
                                                                    .BeAssignableTo<CreatedAtRouteResult>().Which;

            createdAtRouteResult.RouteName.Should()
                .Be(RouteNames.DefaultGetOneByIdApi);
            RouteValueDictionary routeValues = createdAtRouteResult.RouteValues;
            routeValues.Should()
                .ContainKey(nameof(AppointmentInfo.Id)).And
                .ContainKey("controller");

            routeValues[nameof(AppointmentInfo.Id)].Should()
                .BeAssignableTo<Guid>().Which.Should()
                .NotBe(Guid.Empty);
            routeValues["controller"].Should()
                .BeAssignableTo<string>().Which.Should()
                .Be(AppointmentsController.EndpointName);

            Browsable<AppointmentInfo> browsableResource = createdAtRouteResult.Value.Should()
                .BeAssignableTo<Browsable<AppointmentInfo>>().Which;

            AppointmentInfo resource = browsableResource.Resource;
            resource.Should()
                .NotBeNull();

            resource.Id.Should()
                .NotBe(AppointmentId.Empty).And
                .NotBeNull();
            resource.StartDate.Should()
                .Be(newAppointment.StartDate.ToInstant());
            resource.EndDate.Should()
                .Be(newAppointment.EndDate.ToInstant());
            resource.Subject.Should()
                .Be(newAppointment.Subject);
            resource.Location.Should()
                .Be(newAppointment.Location);
            IEnumerable<AttendeeInfo> participants = resource.Attendees;
            participants.Should()
                .HaveSameCount(newAppointment.Attendees).And
                .OnlyContain(item => item.Id != default).And
                .OnlyContain(item => newAppointment.Attendees.Select(x => x.Name).Contains(item.Name));

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Href)).And
                .HaveCount(1 + newAppointment.Attendees.Count()).And
                .ContainSingle(link => link.Relation == Self).And
                .Contain(link => link.Relation.Like("get-participant-*"));

            Link linkToSelf = links.Single(link => link.Relation == Self);
            linkToSelf.Method.Should()
                .Be("GET");
            linkToSelf.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AppointmentsController.EndpointName}&id={resource.Id.Value}&version={_apiVersion}");

            IEnumerable<AttendeeId> participantsGuids = participants.Select(p => p.Id);
            IEnumerable<Link> linksToParticipants = links.Where(link => link.Relation.Like("get-participant-*"));
            linksToParticipants.Should()
                               .HaveSameCount(participantsGuids, "Link(s) to GET participant(s) details must be provided").And
                               .OnlyContain(link => link.Method == "GET");
        }

        public static IEnumerable<object[]> MediatorCompletedCommandToRemoveParticipantsFromAppointmentCases
        {
            get
            {
                yield return new object[]
                {
                    DeleteCommandResult.Done,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is NoContentResult),
                    "appointment <=> participant association was sucessfully deleted"
                };
                yield return new object[]
                {
                    DeleteCommandResult.Failed_NotFound,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is NotFoundResult),
                    "Appointment/Participant was not found"
                };

                yield return new object[]
                {
                    DeleteCommandResult.Failed_Unauthorized,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is UnauthorizedResult),
                    "Removing participant from the appointment is not allowed"
                };

                yield return new object[]
                {
                    DeleteCommandResult.Failed_Conflict,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is StatusCodeResult && ((StatusCodeResult)actionResult).StatusCode == Status409Conflict),
                    "Removing participant from the appointment is not allowed"
                };
            }
        }

        [Theory]
        [MemberData(nameof(MediatorCompletedCommandToRemoveParticipantsFromAppointmentCases))]
        public async Task GivenMediatorCompleteCommandExecution_Delete_Returns_ActionResult(DeleteCommandResult cmdResult, Expression<Func<IActionResult, bool>> actionResultExpectation, string reason)
        {
            // Arrange
            AppointmentId appointmentId = AppointmentId.New();
            AttendeeId participantId = AttendeeId.New();

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<RemoveAttendeeFromAppointmentByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cmdResult);

            // Act
            IActionResult actionResult = await _sut.Delete(appointmentId, participantId, ct: default)
                                                   .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                        .NotBeNull().And
                        .Match(actionResultExpectation, reason);
        }
    }
}
