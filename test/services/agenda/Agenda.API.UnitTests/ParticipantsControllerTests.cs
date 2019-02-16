using Agenda.API.Resources;
using Agenda.API.Routing;
using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.Mapping;
using Agenda.Objects;
using AutoMapper.QueryableExtensions;
using Bogus;
using FluentAssertions;
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
                uow.Repository<Participant>().Delete(x => true);

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
                                first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={ParticipantsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                next :(Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={ParticipantsController.EndpointName}" +
                                    $"&page=1" +
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
                        first : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=1&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        previous : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        next : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=2&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=4&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)))
                    )  // expected link to last page
                };

                yield return new object[]
                {
                    items,
                    (pageSize : 5, page : 4), // request
                    (defaultPageSize : 30, maxPageSize : 200),
                    20,    //expected total
                    (
                        first : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=1&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        previous : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Previous && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=3&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        next : ((Expression<Func<Link, bool>>) (x => x == null )), // expected link to next page
                        last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=4&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)))
                    )  // expected link to last page
                };

                yield return new object[]
               {
                    items,
                    (pageSize : 5, page : 2), // request
                    (defaultPageSize : 30, maxPageSize : 200),
                    20,    //expected total
                    (
                        first : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=1&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        previous : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Previous && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=1&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        next : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=3&pageSize=5".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={ParticipantsController.EndpointName}&page=4&pageSize=5".Equals(x.Href, OrdinalIgnoreCase)))
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
                                new[] { OrderClause<ParticipantInfo>.Create(x => x.UpdatedDate) },
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
                    .OnlyContain(x => x.Links.Once(link => link.Relation == LinkRelation.Self), "links must contain only self relation");
            }

            response.Count.Should()
                    .Be(expectedCount, $@"the ""{nameof(GenericPagedGetResponse<ParticipantInfo>)}.{nameof(GenericPagedGetResponse<ParticipantInfo>.Count)}"" property indicates the number of elements");

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
        }
    }
}
