using Agenda.API.Controllers;
using Agenda.API.Routing;
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.Mapping;
using Agenda.Objects;
using AutoMapper.QueryableExtensions;
using FluentAssertions;
using FluentAssertions.Extensions;
using GenFu;
using MedEasy.DAL.Context;
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
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;
using static System.StringComparison;
using static Newtonsoft.Json.Formatting;
using Agenda.DTO.Resources.Search;

namespace Agenda.API.UnitTests.Features
{
    [Feature("Agenda")]
    [UnitTest]
    public class AppointmentsControllerTests : IDisposable, IClassFixture<DatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUrlHelper> _urlHelperMock;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<IOptionsSnapshot<AgendaApiOptions>> _apiOptionsMock;
        private Mock<IMediator> _mediatorMock;
        private AppointmentsController _sut;
        private const string _baseUrl = "agenda";

        public AppointmentsControllerTests(ITestOutputHelper outputHelper, DatabaseFixture database)
        {
            _outputHelper = outputHelper;

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

            _sut = new AppointmentsController(_urlHelperMock.Object, _mediatorMock.Object, _apiOptionsMock.Object);
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Delete(x => true);
                uow.Repository<Appointment>().Delete(x => true);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            A.Reset<Appointment>();
            _uowFactory = null;
            _outputHelper = null;
            _urlHelperMock = null;
            _apiOptionsMock = null;
            _mediatorMock = null;
            _sut = null;
        }


        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUrlHelper[] urlHelperCases = { null, Mock.Of<IUrlHelper>() };
                IOptionsSnapshot<AgendaApiOptions>[] optionsCases = { null, Mock.Of<IOptionsSnapshot<AgendaApiOptions>>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };


                IEnumerable<object[]> cases = urlHelperCases
                    .CrossJoin(optionsCases, (urlHelper, options) => ((urlHelper, options)))
                    .Select((tuple) => new { tuple.urlHelper, tuple.options })
                    .CrossJoin(mediatorCases, (tuple, mediator) => ((tuple.urlHelper, tuple.options, mediator)))
                    .Select((tuple) => new { tuple.urlHelper, tuple.options, tuple.mediator })
                    .Where(tuple => tuple.urlHelper == null || tuple.options == null || tuple.mediator == null)
                    .Select(tuple => (new object[] { tuple.urlHelper, tuple.mediator, tuple.options }));

                return cases;


            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void GivenNullParameter_Ctor_ThrowsArgumentNullException(IUrlHelper urlHelper, IMediator mediator, IOptionsSnapshot<AgendaApiOptions> apiOptions)
        {
            // Act
            Action action = () => new AppointmentsController(urlHelper, mediator, apiOptions);

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
            int page = 1;
            int pageSize = 10;
            _apiOptionsMock.Setup(mock => mock.Value).Returns(new AgendaApiOptions { MaxPageSize = 200, DefaultPageSize = 30 });
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfAppointmentInfoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Page<AppointmentInfo>.Default);

            // Act
            IActionResult actionResult = await _sut.Get(page: 1, pageSize: 10, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IRequest<Page<AppointmentInfo>>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfAppointmentInfoQuery>(q => q.Data.Page == page && q.Data.PageSize == pageSize), It.IsAny<CancellationToken>()), Times.Once);
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);


            GenericPagedGetResponse<BrowsableResource<AppointmentInfo>> response = actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<OkObjectResult>("the request completed successfully").Which
                .Value.Should()
                .BeAssignableTo<GenericPagedGetResponse<BrowsableResource<AppointmentInfo>>>().Which;


            response.Items.Should()
                .BeEmpty();

            PageLinks links = response.Links;
            links.Should()
                .NotBeNull();

            Link first = links.First;
            first.Should()
                .NotBeNull("Link to the first page of the query must be set");
            first.Method.Should().Be("GET");
            first.Relation.Should().Be(LinkRelation.First);
            first.Href.Should().BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize={pageSize}");

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
            last.Relation.Should().Be(LinkRelation.Last);
            last.Href.Should().BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize={pageSize}");


        }




        [Fact]
        public async Task GivenNoDataFromMediator_GetAppointmentById_Returns_NotFound()
        {
            // Arrange
            Guid appointmentId = Guid.NewGuid();

            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAppointmentInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<AppointmentInfo>());

            // Act
            IActionResult actionResult = await _sut.Get(id: appointmentId, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IRequest<Option<AppointmentInfo>>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<GetOneAppointmentInfoByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }


        [Fact]
        public async Task GivenOneResultFromMediator_GetAppointmentById_Returns_Ok()
        {
            // Arrange
            Guid appointmentId = Guid.NewGuid();

            Appointment appointment = new Appointment
            {
                UUID = appointmentId,
                Location = "Wayne Tower",
                Subject = "Confidential",
                StartDate = 12.July(2013).AddHours(14).AddMinutes(30),
                EndDate = 12.July(2013).AddHours(14).AddMinutes(45)
            };

            appointment.AddParticipant(new Participant { UUID = Guid.NewGuid(), Name = "Bruce Wayne" });
            appointment.AddParticipant(new Participant { UUID = Guid.NewGuid(), Name = "Dick Grayson" });

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Delete(x => true);
                uow.Repository<Appointment>().Delete(x => true);
                uow.Repository<Appointment>().Create(appointment);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneAppointmentInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetOneAppointmentInfoByIdQuery query, CancellationToken ct) =>
                {
                    _outputHelper.WriteLine($"Executing query : {SerializeObject(query)}");
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Expression<Func<Appointment, AppointmentInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Appointment, AppointmentInfo>();

                        _outputHelper.WriteLine($"selector : {selector}");

                        AppointmentInfo appointmentInfo = await uow.Repository<Appointment>()
                            .SingleAsync(selector, x => x.UUID == query.Data, ct)
                            .ConfigureAwait(false);

                        return Option.Some(appointmentInfo);
                    }

                });


            // Act
            IActionResult actionResult = await _sut.Get(appointmentId, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetOneAppointmentInfoByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            BrowsableResource<AppointmentInfo> browsableResource = actionResult.Should()
                .BeAssignableTo<OkObjectResult>().Which
                .Value.Should()
                .BeOfType<BrowsableResource<AppointmentInfo>>().Which;

            AppointmentInfo resource = browsableResource.Resource;
            resource.Id.Should()
                .Be(appointmentId);
            resource.StartDate.Should()
                .Be(appointment.StartDate);
            resource.EndDate.Should()
                .Be(appointment.EndDate);
            resource.Participants.Should()
                .HaveCount(appointment.Participants.Count());

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .HaveCount(2).And
                .ContainSingle(link => link.Relation == LinkRelation.Self).And
                .ContainSingle(link => link.Relation == "delete");

            Link selfLink = links.Single(link => link.Relation == LinkRelation.Self);
            selfLink.Method.Should().Be("GET");
            selfLink.Href.Should().BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AppointmentsController.EndpointName}&id={appointmentId}");

            Link deleteLink = links.Single(link => link.Relation == "delete");
            deleteLink.Method.Should().Be("DELETE");
            deleteLink.Href.Should().BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AppointmentsController.EndpointName}&id={appointmentId}");

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
                            (pageSize, page), // request,
                            (defaultPageSize : 30, maxPageSize : 200),
                            0,    //expected total
                            (
                                first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={AppointmentsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                next :(Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={AppointmentsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase))
                            )  // expected link to last page
                        };
                    }
                }

                {
                    IEnumerable<Appointment> items = A.ListOf<Appointment>(400);
                    items.ForEach((item, pos) =>
                    {
                        item.Id = default;

                        Random random = new Random(pos);
                        int nbParticipants = random.Next(1, 5);
                        for (int i = 0; i < nbParticipants; i++)
                        {
                            Guid participantId = Guid.NewGuid();
                            item.AddParticipant(new Participant { UUID = participantId, Name = $"participant-{participantId}" });
                        }

                    });
                    yield return new object[]
                    {
                        items,
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)))
                        )  // expected link to last page
                    };
                }
                {
                    IEnumerable<Appointment> items = A.ListOf<Appointment>(400);
                    items.ForEach((item, pos) =>
                    {
                        item.Id = default;

                        Random random = new Random(pos);
                        int nbParticipants = random.Next(1, 5);
                        for (int i = 0; i < nbParticipants; i++)
                        {
                            Guid participantId = Guid.NewGuid();
                            item.AddParticipant(new Participant { UUID = participantId, Name = $"participant-{participantId}" });
                        }

                    });
                    yield return new object[]
                    {
                        items,
                        (pageSize : 10, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First
                                && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                        )
                    };
                }
                {
                    Appointment appointment = new Appointment
                    {
                        Location = "Wayne Tower",
                        Subject = "Confidential",
                        StartDate = 12.July(2013).AddHours(14).AddMinutes(30),
                        EndDate = 12.July(2013).AddHours(14).AddMinutes(45),
                    };
                    appointment.AddParticipant(new Participant { Name = "Bruce Wayne" });
                    appointment.AddParticipant(new Participant { Name = "Dick Grayson" });
                    yield return new object[]
                    {
                        new [] { appointment },
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        1,    //expected total
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={AppointmentsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={PaginationConfiguration.DefaultPageSize}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous :((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={AppointmentsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)))
                        ), // expected link to last page
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Appointment> items, (int pageSize, int page) request,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(AppointmentsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {request.pageSize}");
            _outputHelper.WriteLine($"Page : {request.page}");
            _outputHelper.WriteLine($"items count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Delete(x => true);
                uow.Repository<Appointment>().Delete(x => true);
                uow.Repository<Appointment>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfAppointmentInfoQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetPageOfAppointmentInfoQuery query, CancellationToken cancellationToken) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Expression<Func<Appointment, AppointmentInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Appointment, AppointmentInfo>();
                        Page<AppointmentInfo> result = await uow.Repository<Appointment>()
                            .ReadPageAsync(
                                selector,
                                query.Data.PageSize,
                                query.Data.Page,
                                new[] { OrderClause<AppointmentInfo>.Create(x => x.UpdatedDate) },
                                cancellationToken)
                            .ConfigureAwait(false);

                        return result;
                    }
                });

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new AgendaApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            IActionResult actionResult = await _sut.Get(page: request.page, pageSize: request.pageSize)
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"{nameof(AppointmentsController)}.{nameof(AppointmentsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(AgendaApiOptions.MaxPageSize)} value");
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfAppointmentInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            GenericPagedGetResponse<BrowsableResource<AppointmentInfo>> response = okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<GenericPagedGetResponse<BrowsableResource<AppointmentInfo>>>().Which;

            _outputHelper.WriteLine($"response : {response}");

            response.Items.Should()
                .NotBeNull();

            if (response.Items.Any())
            {
                response.Items.Should()
                    .NotContainNulls().And
                    .OnlyContain(x => x.Resource != null, "resource must not be null").And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == LinkRelation.Self), "links must contain only self relation");
            }

            response.Count.Should()
                    .Be(expectedCount, $@"the ""{nameof(GenericPagedGetResponse<AppointmentInfo>)}.{nameof(GenericPagedGetResponse<AppointmentInfo>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(linksExpectation.lastPageUrlExpectation);

        }


        public static IEnumerable<object[]> SearchCases
        {
            get
            {
                yield return new object[]
                {
                    Page<AppointmentInfo>.Default,
                    new SearchAppointmentInfo { Page = 1, PageSize = 10},
                    (defaultPageSize : 20, maxPageSize : 50),
                    (
                        firstPageUrlExpectation : ((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.First
                            && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                        )),
                        previousPageUrl : ((Expression<Func<Link, bool>>)(link => link == null)),
                        nextPageUrl : ((Expression<Func<Link, bool>>)(link => link == null)),
                        lastPageUrlExpectation :((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.Last
                            && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                        ))
                    )
                };

                {
                    A.Configure<AppointmentInfo>()
                        .Fill(x => x.Id)
                        .Fill(x => x.Location).AsCity()
                        .Fill(x => x.Subject).AsLoremIpsumWords(numberOfWords: 5)
                        .Fill(x => x.StartDate)
                        .Fill(x => x.EndDate)
                        .Fill(x => x.UpdatedDate);

                    yield return new object[]
                    {
                        new Page<AppointmentInfo>(A.ListOf<AppointmentInfo>(10), total : 50, size :10),
                        new SearchAppointmentInfo { Page = 1, PageSize = 10},
                        (defaultPageSize : 20, maxPageSize : 50),
                        (
                            firstPageUrlExpectation : ((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.First
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                            )),
                            previousPageUrl : ((Expression<Func<Link, bool>>)(link => link == null)),
                            nextPageUrl : ((Expression<Func<Link, bool>>)(link => link != null && link.Method == "GET" && link.Relation == LinkRelation.Next
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=2&pageSize=10".Equals(link.Href, OrdinalIgnoreCase))),
                            lastPageUrlExpectation :((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.Last
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=5&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                            ))
                        )
                    };
                }

                {
                    A.Configure<AppointmentInfo>()
                        .Fill(x => x.Id)
                        .Fill(x => x.Location).AsCity()
                        .Fill(x => x.Subject).AsLoremIpsumWords(numberOfWords: 5)
                        .Fill(x => x.StartDate)
                        .Fill(x => x.EndDate)
                        .Fill(x => x.UpdatedDate);

                    yield return new object[]
                    {
                        new Page<AppointmentInfo>(A.ListOf<AppointmentInfo>(10), total : 50, size :10),
                        new SearchAppointmentInfo { Page = 2, PageSize = 10},
                        (defaultPageSize : 20, maxPageSize : 50),
                        (
                            firstPageUrlExpectation : ((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.First
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                            )),
                            previousPageUrl : ((Expression<Func<Link, bool>>)(link => link != null && link.Method == "GET" && link.Relation == LinkRelation.Previous
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10".Equals(link.Href, OrdinalIgnoreCase))),
                            nextPageUrl : ((Expression<Func<Link, bool>>)(link => link != null && link.Method == "GET" && link.Relation == LinkRelation.Next
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=3&pageSize=10".Equals(link.Href, OrdinalIgnoreCase))),
                            lastPageUrlExpectation :((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.Last
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=5&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                            ))
                        )
                    };
                }

                {
                    A.Configure<AppointmentInfo>()
                        .Fill(x => x.Id)
                        .Fill(x => x.Location).AsCity()
                        .Fill(x => x.Subject).AsLoremIpsumWords(numberOfWords: 5)
                        .Fill(x => x.StartDate)
                        .Fill(x => x.EndDate)
                        .Fill(x => x.UpdatedDate);

                    yield return new object[]
                    {
                        new Page<AppointmentInfo>(A.ListOf<AppointmentInfo>(10), total : 50, size :10),
                        new SearchAppointmentInfo { Page = 5, PageSize = 10},
                        (defaultPageSize : 20, maxPageSize : 50),
                        (
                            firstPageUrlExpectation : ((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.First
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                            )),
                            previousPageUrl : ((Expression<Func<Link, bool>>)(link => link != null && link.Method == "GET" && link.Relation == LinkRelation.Previous
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=4&pageSize=10".Equals(link.Href, OrdinalIgnoreCase))),
                            nextPageUrl : ((Expression<Func<Link, bool>>)(link => link == null )),
                            lastPageUrlExpectation :((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.Last
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=5&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                            ))
                        )
                    };
                }

                {
                    A.Configure<AppointmentInfo>()
                        .Fill(x => x.Id)
                        .Fill(x => x.Location).AsCity()
                        .Fill(x => x.Subject).AsLoremIpsumWords(numberOfWords: 5)
                        .Fill(x => x.StartDate)
                        .Fill(x => x.EndDate)
                        .Fill(x => x.UpdatedDate);

                    yield return new object[]
                    {
                        new Page<AppointmentInfo>(A.ListOf<AppointmentInfo>(5), total : 45, size :10),
                        new SearchAppointmentInfo { Page = 5, PageSize = 10},
                        (defaultPageSize : 20, maxPageSize : 50),
                        (
                            firstPageUrlExpectation : ((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.First
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=1&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                            )),
                            previousPageUrl : ((Expression<Func<Link, bool>>)(link => link != null && link.Method == "GET" && link.Relation == LinkRelation.Previous
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=4&pageSize=10".Equals(link.Href, OrdinalIgnoreCase))),
                            nextPageUrl : ((Expression<Func<Link, bool>>)(link => link == null )),
                            lastPageUrlExpectation :((Expression<Func<Link, bool>>)(link => link.Method == "GET" && link.Relation == LinkRelation.Last
                                && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AppointmentsController.EndpointName}&page=5&pageSize=10".Equals(link.Href, OrdinalIgnoreCase)
                            ))
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task GivenMediatorReturnsData_Search_ReturnsOk(Page<AppointmentInfo> page, SearchAppointmentInfo search,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            _outputHelper.WriteLine($"Mediator response: {SerializeObject(page, Indented)}");

            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchAppointmentInfoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new AgendaApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            IActionResult actionResult = await _sut.Search(search, ct : default)
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"{nameof(AppointmentsController)}.{nameof(AppointmentsController.Search)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(AgendaApiOptions.MaxPageSize)} value");
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<SearchAppointmentInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            GenericPagedGetResponse<BrowsableResource<AppointmentInfo>> response = okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<GenericPagedGetResponse<BrowsableResource<AppointmentInfo>>>().Which;

            _outputHelper.WriteLine($"response : {response}");

            response.Items.Should()
                .NotBeNull();

            if (response.Items.Any())
            {
                response.Items.Should()
                    .NotContainNulls().And
                    .OnlyContain(x => x.Resource != null, "resource must not be null").And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == LinkRelation.Self), "links must contain only self relation");
            }

            response.Count.Should()
                    .Be(page.Total, $@"the ""{nameof(GenericPagedGetResponse<AppointmentInfo>)}.{nameof(GenericPagedGetResponse<AppointmentInfo>.Count)}"" property indicates the number of elements");

            response.Links.First.Should()
                .NotBeNull().And
                .Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should()
                .NotBeNull().And
                .Match(linksExpectation.lastPageUrlExpectation);

        }

    }
}
