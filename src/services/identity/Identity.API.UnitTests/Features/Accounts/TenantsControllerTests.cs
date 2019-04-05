using FluentAssertions;
using Identity.API.Features.Accounts;
using Identity.API.Routing;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using Identity.Objects;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Optional;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace Identity.API.Tests.Features.Accounts
{
    /// <summary>
    /// Unit tests for <see cref="TenantsController"/>
    /// </summary>
    [UnitTest]
    [Feature("Tenants")]
    [Feature("Identity")]
    public class TenantsControllerTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;

        private IUnitOfWorkFactory _uowFactory;
        private static readonly IdentityApiOptions _apiOptions = new IdentityApiOptions { DefaultPageSize = 30, MaxPageSize = 200 };
        private Mock<IMediator> _mediatorMock;
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<IOptionsSnapshot<IdentityApiOptions>> _apiOptionsMock;
        private TenantsController _sut;
        private const string _baseUrl = "http://host/api";

        public TenantsControllerTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
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

            _sut = new TenantsController(urlHelper: _urlHelperMock.Object, apiOptions: _apiOptionsMock.Object, mediator: _mediatorMock.Object);
        }

        public async void Dispose()
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
            Guid tenantId = Guid.NewGuid();
            Account tenant = new Account
            {
                UUID = tenantId,
                UserName = "thebatman",
                PasswordHash = "a_super_secret_password",
                Email = "bruce@wayne-entreprise.com",
                Salt = "salt_and_pepper_for_password",
                TenantId = Guid.NewGuid()
            };
            Account newAccount = new Account
            {
                UUID = Guid.NewGuid(),
                UserName = "robin",
                PasswordHash = "a_super_secret_password",
                Email = "dick.grayson@wayne-entreprise.com",
                Salt = "salt_and_pepper_for_password",
                TenantId = tenant.UUID
            };

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(new[] { tenant, newAccount });

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<IsTenantQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (IsTenantQuery query, CancellationToken ct) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        return await uow.Repository<Account>()
                            .AnyAsync((Account x) => x.TenantId == query.Data, ct)
                            .ConfigureAwait(false);
                    }
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
                    .Be(tenantId);
        }

        [Fact]
        public async Task When_Id_DoNotStands_For_Tenant_Get_Returns_NotFoundResult()
        {
            // Arrange
            Guid accountId = Guid.NewGuid();
            Account newAccount = new Account
            {
                UUID = accountId,
                UserName = "robin",
                PasswordHash = "a_super_secret_password",
                Email = "dick.grayson@wayne-entreprise.com",
                Salt = "salt_and_pepper_for_password"
            };

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(newAccount);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<IsTenantQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (IsTenantQuery query, CancellationToken ct) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        return await uow.Repository<Account>()
                            .AnyAsync((Account x) => x.TenantId == query.Data,ct)
                            .ConfigureAwait(false);
                    }
                });

            // Act
            IActionResult actionResult = await _sut.Get(accountId, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<IsTenantQuery>(q => q.Data == accountId), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<IsTenantQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>($"account <{accountId}> is not a tenant");
        }
    }
}
