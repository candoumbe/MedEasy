namespace Identity.API.UnitTests.Features.v1.Accounts
{
    using Bogus;
    using FluentAssertions;
    using Identity.API.Routing;
    using Identity.CQRS.Commands.Accounts;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.Mapping;
    using Identity.Objects;
    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;
    using MediatR;
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
    using static System.StringComparison;
    using static MedEasy.RestObjects.LinkRelation;
    using static System.Uri;
    using Identity.API.Features.v1.Accounts;
    using Microsoft.AspNetCore.Http;
    using NodaTime.Testing;
    using NodaTime;
    using Identity.Ids;
    using MedEasy.Ids;
    using MedEasy.ValueObjects;

    /// <summary>
    /// Unit tests for <see cref="AccountsController"/>
    /// </summary>
    [UnitTest]
    [Feature("Accounts")]
    [Feature("Identity")]
    public class AccountsControllerTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<IdentityDataStore>>
    {
        private readonly ITestOutputHelper _outputHelper;

        private readonly IUnitOfWorkFactory _uowFactory;
        private static readonly IdentityApiOptions ApiOptions = new() { DefaultPageSize = 30, MaxPageSize = 200 };
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<LinkGenerator> _urlHelperMock;
        private readonly Mock<IOptionsSnapshot<IdentityApiOptions>> _apiOptionsMock;
        private readonly AccountsController _sut;
        private const string BaseUrl = "http://host/api";

        public AccountsControllerTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityDataStore> database)
        {
            _outputHelper = outputHelper;

            _urlHelperMock = new Mock<LinkGenerator>(Strict);
            _urlHelperMock.Setup(mock => mock.GetPathByAddress(It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<PathString>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
                .Returns((string routename, RouteValueDictionary routeValues, PathString _, FragmentString _, LinkOptions _)
                => $"{BaseUrl}/{routename}/?{routeValues.ToQueryString()}");

            _apiOptionsMock = new Mock<IOptionsSnapshot<IdentityApiOptions>>(Strict);

            _uowFactory = new EFUnitOfWorkFactory<IdentityDataStore>(database.OptionsBuilder.Options, (options) =>
            {
                IdentityDataStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new AccountsController(urlHelper: _urlHelperMock.Object, apiOptions: _apiOptionsMock.Object, mediator: _mediatorMock.Object);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Clear();
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
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
                                firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First  && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=1&pageSize={Math.Min(pageSize, ApiOptions.MaxPageSize) }".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=1&pageSize={Math.Min(pageSize, ApiOptions.MaxPageSize)}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        };
                    }
                }

                Faker<Account> accountFaker = new Faker<Account>()
                    .CustomInstantiator(faker => new Account(id: AccountId.New(),
                        username: UserName.From(faker.Internet.UserName()),
                        email: Email.From(faker.Internet.Email()),
                        passwordHash: Password.From(faker.Internet.Password()),
                        salt: string.Empty
                    ));
                {
                    IEnumerable<Account> items = accountFaker.Generate(400);
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == First
                                && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
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
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First  && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={AccountsController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
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

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(ApiOptions);

            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfAccountsQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetPageOfAccountsQuery query, CancellationToken _) =>
                {
                    PaginationConfiguration pagination = query.Data;
                    Expression<Func<Account, AccountInfo>> expression = x => new AccountInfo { Id = x.Id, Email = x.Email, Username = x.Username };
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
            IActionResult actionResult = await _sut.Get(new PaginationConfiguration { PageSize = pageSize, Page = page })
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfAccountsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfAccountsQuery>(cmd => cmd.Data.Page == page && cmd.Data.PageSize == Math.Min(pageSize, ApiOptions.MaxPageSize)), It.IsAny<CancellationToken>()), Times.Once,
                "Controller must cap pageSize of the query before sending it to the mediator");

            GenericPagedGetResponse<Browsable<AccountInfo>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                        .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<GenericPagedGetResponse<Browsable<AccountInfo>>>().Which;

            response.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            response.Total.Should()
                    .Be(expectedCount, $@"because the ""{nameof(GenericPagedGetResponse<Browsable<AccountInfo>>)}.{nameof(GenericPagedGetResponse<Browsable<AccountInfo>>.Total)}"" property indicates the number of elements");

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
            AccountId idToDelete = AccountId.New();
            IActionResult actionResult = await _sut.Delete(idToDelete, ct: default)
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
            AccountId accountId = AccountId.New();
            Account account = new Account
                            (
                                id: accountId,
                                username: UserName.From("thebatman"),
                                passwordHash: Password.From("a_super_secret_password"),
                                email: Email.From("bruce@wayne-entreprise.com"),
                                salt: "salt_and_pepper_for_password"

                            );
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(account);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneAccountByIdQuery>(), It.IsAny<CancellationToken>()))
                         .Returns(async (GetOneAccountByIdQuery query, CancellationToken ct) =>
                         {
                             using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                             return await uow.Repository<Account>()
                                             .SingleOrDefaultAsync(x => new AccountInfo { Id = x.Id, Email = x.Email, Username = x.Username },
                                                                   (Account x) => x.Id == query.Data,
                                                                   ct)
                                             .ConfigureAwait(false);
                         });

            // Act
            ActionResult<Browsable<AccountInfo>> actionResult = await _sut.Get(accountId, ct: default)
                                                                          .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneAccountByIdQuery>(q => q.Data == accountId), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Value.Should()
                              .NotBeNull();

            Browsable<AccountInfo> browsableResource = actionResult.Value;

            browsableResource.Links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href)).And
                .ContainSingle(x => x.Relation == Self).And
                .ContainSingle(x => x.Relation == "delete");

            Link self = browsableResource.Links.Single(x => x.Relation == Self);
            self.Method.Should()
                .Be("GET");

            AccountInfo resource = browsableResource.Resource;
            self.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AccountsController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            Link delete = browsableResource.Links.Single(x => x.Relation == "delete");
            delete.Method.Should()
                .Be("DELETE");
            delete.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AccountsController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            resource.Id.Should().Be(accountId);
            resource.Username.Should().Be(account.Username);
            resource.Email.Should().Be(account.Email);
        }

        [Fact]
        public async Task Get_Returns_The_Element_With_Tenant()
        {
            // Arrange
            AccountId accountId = AccountId.New();

            Account tenant = new(id: AccountId.New(),
                                  username: UserName.From("thebatman"),
                                  passwordHash: Password.From("a_super_secret_password"),
                                  email: Email.From("bruce@wayne-entreprise.com"),
                                  salt: "salt_and_pepper_for_password",
                                  tenantId: TenantId.New()
            );
            Account newAccount = new(
                id: accountId,
                username: UserName.From("robin"),
                passwordHash: Password.From("a_super_secret_password"),
                email: Email.From("dick.grayson@wayne-entreprise.com"),
                salt: "salt_and_pepper_for_password",
                tenantId: new(tenant.Id.Value)
            );

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(new[] { newAccount, tenant });

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneAccountByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetOneAccountByIdQuery query, CancellationToken ct) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    return await uow.Repository<Account>()
                                    .SingleOrDefaultAsync(
                                        x => new AccountInfo { Id = x.Id, Email = x.Email, Username = x.Username, TenantId = x.TenantId },
                                        (Account x) => x.Id == query.Data,
                                        ct)
                                    .ConfigureAwait(false);
                });

            // Act
            ActionResult<Browsable<AccountInfo>> actionResult = await _sut.Get(accountId, ct: default)
                                                                          .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneAccountByIdQuery>(q => q.Data == accountId), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Value.Should()
                              .NotBeNull();

            Browsable<AccountInfo> browsableResource = actionResult.Value;

            browsableResource.Links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href)).And
                .ContainSingle(x => x.Relation == Self).And
                .ContainSingle(x => x.Relation == "tenant").And
                .ContainSingle(x => x.Relation == "delete");

            Link self = browsableResource.Links.Single(x => x.Relation == Self);
            self.Method.Should()
                .Be("GET");

            AccountInfo resource = browsableResource.Resource;
            self.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AccountsController.EndpointName}&{nameof(AccountInfo.Id)}={resource.Id}");

            Link tenantLink = browsableResource.Links.Single(x => x.Relation == "tenant");
            tenantLink.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AccountsController.EndpointName}&{nameof(AccountInfo.Id)}={resource.TenantId}");

            Link delete = browsableResource.Links.Single(x => x.Relation == "delete");
            delete.Method.Should()
                .Be("DELETE");
            delete.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AccountsController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            resource.Id.Should().Be(accountId);
            resource.Username.Should().Be(newAccount.Username);
            resource.Email.Should().Be(newAccount.Email);
            resource.TenantId.Should().Be(newAccount.TenantId);
        }

        [Fact]
        public async Task Get_UnknonwnId_Returns_NotFound()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneAccountByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<AccountInfo>());

            // Act
            ActionResult<Browsable<AccountInfo>> actionResult = await _sut.Get(id: AccountId.New(), ct: default)
                                                                          .ConfigureAwait(false);

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
            AccountId idToDelete = AccountId.New();
            IActionResult actionResult = await _sut.Delete(idToDelete, ct: default)
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
            AccountId idToDelete = AccountId.New();
            IActionResult actionResult = await _sut.Delete(idToDelete, ct: default)
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
            JsonPatchDocument<AccountInfo> changes = new();
            changes.Replace(x => x.Email, Email.From("bruce.wayne@gorham.com"));

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<AccountId, AccountInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Failed_NotFound);

            // Act
            IActionResult actionResult = await _sut.Patch(id: AccountId.New(), changes, ct: default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_Valid_Resource_Returns_NoContentResult()
        {
            // Arrange
            JsonPatchDocument<AccountInfo> changes = new();
            changes.Replace(x => x.Email, Email.From("bruce.wayne@gorham.com"));

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<AccountId, AccountInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Done);

            // Act
            IActionResult actionResult = await _sut.Patch(AccountId.New(), changes)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<PatchCommand<AccountId, AccountInfo>>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async Task GivenMediatorReturnsConflict_PostReturns_ConflictedResult()
        {
            // Arrange
            NewAccountInfo newAccount = new()
            {
                Username = UserName.From("thebatman"),
                Email = Email.From("b.wayne@gotham.com")
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreateAccountInfoCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Option.None<AccountInfo, CreateCommandFailure>(CreateCommandFailure.Conflict));

            // Act
            IActionResult actionResult = await _sut.Post(newAccount, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<CreateAccountInfoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<CreateAccountInfoCommand>(cmd => cmd.Data == newAccount), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<StatusCodeResult>().Which
                .StatusCode.Should()
                    .Be(Status409Conflict);
        }

        [Fact]
        public async Task GivenMediatorReturnAccountCreated_PostReturns_OkObjectResult()
        {
            // Arrange
            NewAccountInfo newAccount = new()
            {
                Username = UserName.From("thebatman"),
                Email = Email.From("b.wayne@gotham.com")
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreateAccountInfoCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((CreateAccountInfoCommand cmd, CancellationToken _) => Option.Some<AccountInfo, CreateCommandFailure>(new AccountInfo { Username = cmd.Data.Username, Id = AccountId.New() }));

            // Act
            IActionResult actionResult = await _sut.Post(newAccount, ct: default)
                                                   .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<CreateAccountInfoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<CreateAccountInfoCommand>(cmd => cmd.Data == newAccount), It.IsAny<CancellationToken>()), Times.Once);

            CreatedAtRouteResult createdAtRouteResult = actionResult.Should()
                                                                    .BeAssignableTo<CreatedAtRouteResult>().Which;

            Browsable<AccountInfo> browsableResource = createdAtRouteResult.Value.Should()
                                                                                 .BeAssignableTo<Browsable<AccountInfo>>().Which;

            AccountInfo createdResource = browsableResource.Resource;

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Href)).And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Method)).And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Relation)).And
                .Contain(link => link.Relation == Self);

            Link linkSelf = links.Single(link => link.Relation == Self);
            linkSelf.Method.Should()
                           .Be("GET");
            linkSelf.Href.Should()
                         .Be($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={AccountsController.EndpointName}&{nameof(AccountInfo.Id)}={createdResource.Id.Value}");

            createdResource.Username.Should()
                                    .Be(newAccount.Username);

            createdAtRouteResult.RouteName.Should()
                                          .Be(RouteNames.DefaultGetOneByIdApi);
            RouteValueDictionary routeValues = createdAtRouteResult.RouteValues;
            routeValues.Should()
                .ContainKey("controller").WhoseValue.Should().Be(AccountsController.EndpointName);
            routeValues.Should()
                       .ContainKey("id").WhoseValue.Should()
                            .BeOfType<AccountId>().Which.Should()
                            .NotBe(AccountId.Empty);
        }

        public static IEnumerable<object[]> SearchTestCases
        {
            get
            {
                Faker<Account> accountFaker = new Faker<Account>()
                    .CustomInstantiator(faker => new Account(id: AccountId.New(),
                                                             name: $"{faker.PickRandom("Bruce", "Clark", "Oliver", "Martha")} Wayne",
                                                             email: Email.From(faker.Internet.ExampleEmail()),
                                                             passwordHash: Password.From(faker.Internet.Password()),
                                                             username: UserName.From(faker.Internet.UserName()),
                                                             salt: faker.Lorem.Word()))
                    ;
                {
                    IEnumerable<Account> items = accountFaker.Generate(40);

                    yield return new object[]
                    {
                        items,
                        new SearchAccountInfo
                        {
                            Name = "*Wayne",
                            Page = 1, PageSize = 10,
                            Sort = nameof(AccountInfo.Name)
                        },
                        (maxPageSize : 200, defaultPageSize : 30),
                        (
                            count : 40,
                            items : (Expression<Func<IEnumerable<Browsable<SearchAccountInfoResult>>, bool>>)(resources => resources.All(x => x.Resource.Name.Like("*Wayne"))),
                            links :
                            (
                                firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                    && x.Relation == First
                                    && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AccountsController.EndpointName}&name={EscapeDataString("*Wayne")}&page=1&pageSize=10&sort=Name".Equals(x.Href, CurrentCultureIgnoreCase)), // expected link to first page
                                previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Next && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AccountsController.EndpointName}&name={EscapeDataString("*Wayne")}&page=2&pageSize=10&Sort=Name".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                                lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={AccountsController.EndpointName}&name={EscapeDataString("*Wayne")}&page=4&pageSize=10&Sort=Name".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchTestCases))]
        [Feature("Search")]
        public async Task Search(IEnumerable<Account> items, SearchAccountInfo searchQuery,
            (int maxPageSize, int defaultPageSize) apiOptions,
            (
                int count,
                Expression<Func<IEnumerable<Browsable<SearchAccountInfoResult>>, bool>> items,
                (
                    Expression<Func<Link, bool>> firstPageUrlExpectation,
                    Expression<Func<Link, bool>> previousPageUrlExpectation,
                    Expression<Func<Link, bool>> nextPageUrlExpectation,
                    Expression<Func<Link, bool>> lastPageUrlExpectation
                ) links
            ) pageExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(AccountsController.Search)}({nameof(SearchAccountInfo)})");
            _outputHelper.WriteLine($"Search : {searchQuery.Jsonify()}");
            _outputHelper.WriteLine($"store items: {items.Jsonify()}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new IdentityApiOptions { DefaultPageSize = apiOptions.defaultPageSize, MaxPageSize = apiOptions.maxPageSize });

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<SearchAccountInfoResult>>(), It.IsAny<CancellationToken>()))
                .Returns((SearchQuery<SearchAccountInfoResult> query, CancellationToken ct) =>
                {
                    return new HandleSearchQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder)
                        .Search<Account, SearchAccountInfoResult>(query, ct);
                });

            // Act
            IActionResult actionResult = await _sut.Search(searchQuery)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<SearchQuery<SearchAccountInfoResult>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<SearchQuery<SearchAccountInfoResult>>(query => query.Data.Page == searchQuery.Page && query.Data.PageSize == Math.Min(searchQuery.PageSize, apiOptions.maxPageSize)), It.IsAny<CancellationToken>()), Times.Once);
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.AtLeastOnce, $"because {nameof(AccountsController)}.{nameof(AccountsController.Search)} must always check that " +
                $"{nameof(SearchAccountInfo.PageSize)} don't exceed {nameof(IdentityApiOptions.MaxPageSize)} value");

            GenericPagedGetResponse<Browsable<SearchAccountInfoResult>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<GenericPagedGetResponse<Browsable<SearchAccountInfoResult>>>().Which;

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
                    .Be(pageExpectation.count, $@"the ""{nameof(GenericPagedGetResponse<Browsable<SearchAccountInfoResult>>)}.{nameof(GenericPagedGetResponse<Browsable<SearchAccountInfoResult>>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageExpectation.links.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageExpectation.links.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageExpectation.links.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageExpectation.links.lastPageUrlExpectation);
        }
    }
}
