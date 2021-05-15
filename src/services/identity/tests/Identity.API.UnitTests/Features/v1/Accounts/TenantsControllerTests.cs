namespace Identity.API.UnitTests.Features.v1.Accounts
{
    using FluentAssertions;

    using Identity.API.Features.Accounts;
    using Identity.API.Features.v1.Accounts;
    using Identity.API.Routing;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.Ids;
    using Identity.Objects;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.Ids;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    using Moq;

    using NodaTime;
    using NodaTime.Testing;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Moq.MockBehavior;

    /// <summary>
    /// Unit tests for <see cref="TenantsController"/>
    /// </summary>
    [UnitTest]
    [Feature("Tenants")]
    [Feature("Identity")]
    public class TenantsControllerTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<IdentityContext>>
    {
        private ITestOutputHelper _outputHelper;

        private IUnitOfWorkFactory _uowFactory;
        private static readonly IdentityApiOptions _apiOptions = new() { DefaultPageSize = 30, MaxPageSize = 200 };
        private Mock<IMediator> _mediatorMock;
        private Mock<LinkGenerator> _urlHelperMock;
        private Mock<IOptionsSnapshot<IdentityApiOptions>> _apiOptionsMock;
        private readonly TenantsController _sut;
        private const string _baseUrl = "http://host/api";

        public TenantsControllerTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityContext> database)
        {
            _outputHelper = outputHelper;

            _urlHelperMock = new Mock<LinkGenerator>(Strict);
            _urlHelperMock.Setup(mock => mock.GetPathByAddress(It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<PathString>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
                .Returns((string routename, RouteValueDictionary routeValues, PathString _, FragmentString __, LinkOptions ___) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            _apiOptionsMock = new Mock<IOptionsSnapshot<IdentityApiOptions>>(Strict);

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(database.OptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new TenantsController(urlHelper: _urlHelperMock.Object, apiOptions: _apiOptionsMock.Object, mediator: _mediatorMock.Object);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            _outputHelper = null;
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Clear();
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _uowFactory = null;
            _urlHelperMock = null;
            _apiOptionsMock = null;
            _mediatorMock = null;
        }

        [Fact]
        public async Task When_Id_Stands_For_Tenant_Get_Redirect_To_AccountsEndpoint()
        {
            // Arrange
            TenantId tenantId = TenantId.New();
            Account tenant = new(id: new(tenantId.Value),
                                  username: "thebatman",
                                  passwordHash: "a_super_secret_password",
                                  email: "bruce@wayne-entreprise.com",
                                  salt: "salt_and_pepper_for_password",
                                  tenantId: tenantId);

            Account newAccount = new(id: AccountId.New(),
                                      username: "robin",
                                      passwordHash: "a_super_secret_password",
                                      email: "dick.grayson@wayne-entreprise.com",
                                      salt: "salt_and_pepper_for_password",
                                      tenantId: tenantId
            );

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(new[] { tenant, newAccount });

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<IsTenantQuery>(), It.IsAny<CancellationToken>()))
                .Returns((IsTenantQuery query, CancellationToken ct) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    return uow.Repository<Account>()
                              .AnyAsync((x) => x.TenantId == query.Data, ct)
                              .AsTask();
                });

            // Act
            IActionResult actionResult = await _sut.Get(tenantId, ct: default)
                                                   .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<IsTenantQuery>(q => q.Data == tenantId), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IsTenantQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            RedirectToRouteResult redirect = actionResult.Should()
                                                         .BeAssignableTo<RedirectToRouteResult>().Which;

            redirect.Permanent.Should()
                .BeFalse();
            redirect.PreserveMethod.Should()
                .BeTrue();
            redirect.RouteName.Should()
                .Be(RouteNames.DefaultGetOneByIdApi);
            redirect.RouteValues.Should()
                .HaveCount(2).And
                .ContainKeys(new[] { "controller", nameof(AccountInfo.Id) });
            redirect.RouteValues["controller"].Should()
                    .BeOfType<string>().And
                    .BeEquivalentTo(AccountsController.EndpointName);
            redirect.RouteValues[nameof(AccountInfo.Id)].Should()
                    .BeOfType<Guid>().And
                    .Be(tenantId.Value);
        }

        [Fact]
        public async Task When_Id_DoNotStands_For_Tenant_Get_Returns_NotFoundResult()
        {
            // Arrange
            TenantId tenantId = TenantId.New();
            Account newAccount = new(id: new AccountId(tenantId.Value),
                                      username: "robin",
                                      passwordHash: "a_super_secret_password",
                                      email: "dick.grayson@wayne-entreprise.com",
                                      salt: "salt_and_pepper_for_password"
            );

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(newAccount);

                await uow.SaveChangesAsync()
                         .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<IsTenantQuery>(), It.IsAny<CancellationToken>()))
                .Returns((IsTenantQuery query, CancellationToken ct) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    return uow.Repository<Account>()
                              .AnyAsync((x) => x.TenantId == query.Data, ct)
                              .AsTask();
                });

            // Act
            IActionResult actionResult = await _sut.Get(new TenantId(newAccount.Id.Value), ct: default)
                                                   .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<IsTenantQuery>(q => q.Data == tenantId), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IsTenantQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                        .BeAssignableTo<NotFoundResult>($"account <{tenantId}> is not a tenant");
        }
    }
}
