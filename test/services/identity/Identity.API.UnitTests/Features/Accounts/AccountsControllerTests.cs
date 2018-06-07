using AutoMapper.QueryableExtensions;
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.API.Features.Accounts;
using Identity.API.Routing;
using Identity.CQRS.Commands.Accounts;
using Identity.CQRS.Handlers.Queries.Accounts;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using Identity.Mapping;
using Identity.Objects;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
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

namespace Identity.API.Tests.Features.Accounts
{
    /// <summary>
    /// Unit tests for <see cref="AccountsController"/>
    /// </summary>
    [UnitTest]
    [Feature("Accounts")]
    [Feature("Identity")]
    public class AccountsControllerTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;

        private IUnitOfWorkFactory _uowFactory;
        private static readonly IdentityApiOptions _apiOptions = new IdentityApiOptions { DefaultPageSize = 30, MaxPageSize = 200 };
        private Mock<IMediator> _mediatorMock;
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<IOptionsSnapshot<IdentityApiOptions>> _apiOptionsMock;
        private AccountsController _controller;
        private const string _baseUrl = "http://host/api";


        public AccountsControllerTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string routename, object routeValues) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            _apiOptionsMock = new Mock<IOptionsSnapshot<IdentityApiOptions>>(Strict);

            DbContextOptionsBuilder<IdentityContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            dbContextOptionsBuilder.UseSqlite(database.Connection)
                .EnableSensitiveDataLogging();

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(dbContextOptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options);
                context.Database.EnsureCreated();
                return context;
            });

            _mediatorMock = new Mock<IMediator>(Strict);

            _controller = new AccountsController(urlHelper : _urlHelperMock.Object, apiOptions : _apiOptionsMock.Object, mediator: _mediatorMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _uowFactory = null;
            _urlHelperMock = null;
            _apiOptionsMock = null;
            _mediatorMock = null;

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
                            Enumerable.Empty<Account>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            (
                                firstPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First  && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=1&pageSize={Math.Min(pageSize, _apiOptions.MaxPageSize) }".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                                previousPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                                nextPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                                lastPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=1&pageSize={Math.Min(pageSize, _apiOptions.MaxPageSize)}".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                            )
                        };
                    }
                }

                Faker<Account> accountFaker = new Faker<Account>()
                    .RuleFor(x =>x.Id, 0)
                    .RuleFor(x => x.UUID, () => Guid.NewGuid())
                    .RuleFor(x => x.UserName, faker => faker.Internet.UserName())
                    .RuleFor(x => x.Email, faker => faker.Person.Email);

                
                {

                    IEnumerable<Account> items = accountFaker.Generate(400);
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previousPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            nextPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            lastPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                        )
                    };
                }
                {
                    IEnumerable<Account> items = accountFaker.Generate(400);
                    

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First  && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previousPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            nextPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            lastPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Account> items, int pageSize, int page,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) pageLinksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(AccountsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(_apiOptions);

            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfAccountInfoQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetPageOfAccountInfoQuery query, CancellationToken cancellationToken) =>
                {
                    PaginationConfiguration pagination = query.Data;
                    Expression<Func<Account, AccountInfo>> expression = x => new AccountInfo { Id = x.UUID, Email = x.Email, Username = x.UserName };
                    Func<Account, AccountInfo> selector = expression.Compile();
                    _outputHelper.WriteLine($"Selector : {selector}");

                    IEnumerable<AccountInfo> results = items.Select(selector)
                        .ToArray();


                    results = results.Skip(pagination.PageSize * (pagination.Page == 1 ? 0 : pagination.Page - 1))
                         .Take(pagination.PageSize)
                         .ToArray();

                    return Task.FromResult(new Page<AccountInfo>(results, items.Count(), pagination.PageSize));

                });

            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { PageSize = pageSize, Page = page })
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfAccountInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfAccountInfoQuery>(cmd => cmd.Data.Page == page && cmd.Data.PageSize == Math.Min(pageSize, _apiOptions.MaxPageSize)), It.IsAny<CancellationToken>()), Times.Once,
                "Controller must cap pageSize of the query before sending it to the mediator");

            GenericPagedGetResponse<BrowsableResource<AccountInfo>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                        .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<GenericPagedGetResponse<BrowsableResource<AccountInfo>>>().Which;

            response.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(GenericPagedGetResponse<BrowsableResource<AccountInfo>>)}.{nameof(GenericPagedGetResponse<BrowsableResource<AccountInfo>>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageLinksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageLinksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageLinksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageLinksExpectation.lastPageUrlExpectation);
        }

        

        

        [Fact]
        public async Task Delete_Returns_NoContent()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<DeleteAccountInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            Guid idToDelete = Guid.NewGuid();
            IActionResult actionResult = await _controller.Delete(idToDelete, ct: default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NoContentResult>();

            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<DeleteAccountInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteAccountInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task Get_Returns_The_Element()
        {
            // Arrange
            Guid accountId = Guid.NewGuid();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(new Account
                {
                    UUID = accountId,
                    UserName = "thebatman",
                    PasswordHash = "a_super_secret_password",
                    Email = "bruce@wayne-entreprise.com"
                    
                });

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetAccountInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetAccountInfoByIdQuery query, CancellationToken ct) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Option<AccountInfo> result = await uow.Repository<Account>()
                            .SingleOrDefaultAsync(
                                x => new AccountInfo { Id = x.UUID, Email = x.Email, Username = x.UserName },
                                (Account x) => x.UUID == query.Data,
                                ct)
                            .ConfigureAwait(false);

                        return result;
                    }
                });

            // Act
            IActionResult actionResult = await _controller.Get(accountId, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetAccountInfoByIdQuery>(q => q.Data == accountId), It.IsAny<CancellationToken>()), Times.Once);

            BrowsableResource<AccountInfo> browsableResource = actionResult.Should()
                .BeAssignableTo<OkObjectResult>().Which
                .Value.Should()
                .BeAssignableTo<BrowsableResource<AccountInfo>>().Which;

            browsableResource.Links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href)).And
                .ContainSingle(x => x.Relation == LinkRelation.Self).And
                .ContainSingle(x => x.Relation == "delete");


            Link self = browsableResource.Links.Single(x => x.Relation == LinkRelation.Self);
            self.Method.Should()
                .Be("GET");

            AccountInfo resource = browsableResource.Resource;
            self.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AccountsController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            Link delete = browsableResource.Links.Single(x => x.Relation == "delete");
            delete.Method.Should()
                .Be("DELETE");
            delete.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AccountsController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            

            resource.Id.Should().Be(accountId);
            resource.Username.Should().Be("thebatman");
            resource.Email.Should().Be("bruce@wayne-entreprise.com");
            
        }

        [Fact]
        public async Task Get_UnknonwnId_Returns_NotFound()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetAccountInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<AccountInfo>());

            // Act
            IActionResult actionResult = await _controller.Get(id : Guid.NewGuid(), ct : default);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();

        }




        [Fact]
        public async Task DeleteResource()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteAccountInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            Guid idToDelete = Guid.NewGuid();
            IActionResult actionResult = await _controller.Delete(idToDelete, ct : default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeleteAccountInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteAccountInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();


        }

        [Fact]
        public async Task Delete_Unknown_Resource_Returns_Not_Found()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteAccountInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Failed_NotFound);


            // Act
            Guid idToDelete = Guid.NewGuid();
            IActionResult actionResult = await _controller.Delete(idToDelete, ct : default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeleteAccountInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteAccountInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();


        }



        [Fact]
        public async Task Patch_UnknownEntity_Returns_NotFound()
        {
            JsonPatchDocument<AccountInfo> changes = new JsonPatchDocument<AccountInfo>();
            changes.Replace(x => x.Email, "bruce.wayne@gorham.com");

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, AccountInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Failed_NotFound);

            // Act
            IActionResult actionResult = await _controller.Patch(id : Guid.NewGuid(), changes, ct: default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_Valid_Resource_Returns_NoContentResult()
        {
            // Arrange
            JsonPatchDocument<AccountInfo> changes = new JsonPatchDocument<AccountInfo>();
            changes.Replace(x => x.Email, "bruce.wayne@gorham.com");

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, AccountInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Done);

            // Act
            IActionResult actionResult = await _controller.Patch(Guid.NewGuid(), changes)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<PatchCommand<Guid, AccountInfo>>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();

        }

    }
}
