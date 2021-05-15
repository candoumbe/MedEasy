namespace Identity.CQRS.UnitTests.Handlers.Queries.Accounts
{
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
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using NodaTime;
    using MedEasy.Ids;
    using Identity.Ids;

    [UnitTest]
    [Feature("Identity")]
    public class HandleGetPageOfAccountsByTenantIdQueryTests : IClassFixture<SqliteEfCoreDatabaseFixture<IdentityContext>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly EFUnitOfWorkFactory<IdentityContext> _uowFactory;
        private readonly HandleGetPageOfAccountByTenantIdQuery _sut;
        private readonly FakeClock _clock;

        public HandleGetPageOfAccountsByTenantIdQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityContext> databaseFixture)
        {
            _outputHelper = outputHelper;
            _clock = new FakeClock(new Instant());
            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(databaseFixture.OptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new(options, _clock);
                context.Database.EnsureCreated();
                return context;
            });

            _sut = new HandleGetPageOfAccountByTenantIdQuery(_uowFactory, expressionBuilder: AutoMapperConfig.Build().ExpressionBuilder);
        }

        [Fact]
        public async Task GivenNoUser_Handler_Returns_None()
        {
            // Arrange
            GetPageOfAccountInfoByTenantIdInfo data = new()
            {
                PageSize = 10,
                Page = 1,
                TenantId = TenantId.New()
            };
            GetPageOfAccountsByTenantIdQuery query = new(data);

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
            TenantId tenantId = TenantId.New();
            IEnumerable<Account> accounts = new Faker<Account>()
                .CustomInstantiator(faker => new Account(id: AccountId.New(),
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

            GetPageOfAccountInfoByTenantIdInfo data = new()
            {
                PageSize = 10,
                Page = 1,
                TenantId = tenantId
            };
            GetPageOfAccountsByTenantIdQuery query = new(data);

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
