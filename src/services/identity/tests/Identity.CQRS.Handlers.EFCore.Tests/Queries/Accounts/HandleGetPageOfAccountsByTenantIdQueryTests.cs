using Bogus;
using NodaTime.Testing;

using FluentAssertions;

using Identity.CQRS.Handlers.Queries.Accounts;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores;
using Identity.DTO;
using Identity.Mapping;
using Identity.Objects;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using NodaTime;

namespace Identity.CQRS.UnitTests.Handlers.Queries.Accounts
{
    [UnitTest]
    [Feature("Identity")]
    public class HandleGetPageOfAccountsByTenantIdQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private EFUnitOfWorkFactory<IdentityContext> _uowFactory;
        private HandleGetPageOfAccountByTenantIdQuery _sut;
        private FakeClock _clock;

        public HandleGetPageOfAccountsByTenantIdQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture databaseFixture)
        {
            _outputHelper = outputHelper;
            _clock = new FakeClock(new Instant());
            DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
            builder.UseInMemoryDatabase($"{Guid.NewGuid()}");

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options, _clock);
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
                .CustomInstantiator(faker => new Account(id: Guid.NewGuid(),
                                                         tenantId: tenantId,
                                                         email: faker.Internet.Email(),
                                                         username: faker.Person.UserName,
                                                         passwordHash: string.Empty,
                                                         salt: string.Empty)
                {
                    CreatedDate = faker.Noda().Instant.Recent()
                })
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
