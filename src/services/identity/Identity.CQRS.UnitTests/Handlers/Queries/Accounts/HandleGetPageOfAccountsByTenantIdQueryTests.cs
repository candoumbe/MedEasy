using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.CQRS.Handlers.Queries.Accounts;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using Identity.Mapping;
using Identity.Objects;
using MedEasy.Abstractions;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.EntityFrameworkCore;
using Moq;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace Identity.CQRS.UnitTests.Handlers.Queries.Accounts
{
    [UnitTest]
    [Feature("Identity")]
    public class HandleGetPageOfAccountsByTenantIdQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private EFUnitOfWorkFactory<IdentityContext> _uowFactory;
        private HandleGetPageOfAccountByTenantIdQuery _sut;

        public HandleGetPageOfAccountsByTenantIdQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture databaseFixture)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
            builder.UseSqlite(databaseFixture.Connection);

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options);
                context.Database.EnsureCreated();
                return context;
            });

            _sut = new HandleGetPageOfAccountByTenantIdQuery(_uowFactory, expressionBuilder: AutoMapperConfig.Build().ExpressionBuilder);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _sut = null;
            _uowFactory = null;
        }

        [Fact]
        public async Task GivenNoUser_Handler_Returns_None()
        {
            // Arrange
            GetPageOfAccountInfoByTenantIdInfo data = new GetPageOfAccountInfoByTenantIdInfo
            {
                PageSize = 10,
                Page = 1,
                TenantId = Guid.NewGuid()
            };
            GetPageOfAccountsByTenantIdQuery query = new GetPageOfAccountsByTenantIdQuery(data);

            // Act
            Page<AccountInfo> pageOfAccounts = await _sut.Handle(query, default)
                .ConfigureAwait(false);

            // Assert
            pageOfAccounts.Should()
                .NotBeNull();
            pageOfAccounts.Total.Should()
                .Be(0);
            pageOfAccounts.Count.Should().Be(1);
            pageOfAccounts.Entries.Should()
                .NotBeNull().And
                .BeEmpty("Account store is empty element");
        }

        [Fact]
        public async Task GivenAccounts_Handler_Returns_AccountsThatBelongToTenant()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            IEnumerable<Account> accounts = new Faker<Account>()
                .RuleFor(x => x.TenantId, tenantId)
                .RuleFor(x => x.UserName, f => f.Person.UserName)
                .RuleFor(x => x.PasswordHash, string.Empty)
                .RuleFor(x => x.Salt, string.Empty)
                .Generate(10);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(accounts);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            GetPageOfAccountInfoByTenantIdInfo data = new GetPageOfAccountInfoByTenantIdInfo
            {
                PageSize = 10,
                Page = 1,
                TenantId = tenantId
            };
            GetPageOfAccountsByTenantIdQuery query = new GetPageOfAccountsByTenantIdQuery(data);

            // Act
            Page<AccountInfo> pageOfAccounts = await _sut.Handle(query, default)
                .ConfigureAwait(false);

            // Assert
            pageOfAccounts.Should()
                .NotBeNull();
            pageOfAccounts.Count.Should().Be(1);
            pageOfAccounts.Entries.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .HaveCount(10).And
                .OnlyContain(x => x.TenantId == query.Data.TenantId);
        }
    }
}
