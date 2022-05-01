namespace Identity.CQRS.UnitTests.Handlers.Accounts
{
    using AutoMapper.QueryableExtensions;

    using Bogus;

    using FluentAssertions;

    using Identity.CQRS.Handlers.Queries.Accounts;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DataStores;
    using Identity.Ids;
    using Identity.Objects;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.Ids;
    using MedEasy.IntegrationTests.Core;
    using MedEasy.ValueObjects;

    using MediatR;

    using Moq;

    using Newtonsoft.Json;

    using NodaTime;
    using NodaTime.Testing;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Newtonsoft.Json.JsonConvert;

    [UnitTest]
    [Feature("Handlers")]
    [Feature("Accounts")]
    public class HandleIsTenantQueryTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<IdentityDataStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private HandleIsTenantQuery _sut;

        public HandleIsTenantQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityDataStore> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<IdentityDataStore>(database.OptionsBuilder.Options, (options) =>
            {
                IdentityDataStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleIsTenantQuery(_uowFactory);
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
            _uowFactory = null;
            _sut = null;
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IExpressionBuilder[] expressionBuilderCases = { null, Mock.Of<IExpressionBuilder>() };

                IEnumerable<object[]> cases = uowFactorieCases
                    .CrossJoin(expressionBuilderCases, (uowFactory, expressionBuilder) => (uowFactory, expressionBuilder))
                    .Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder == null)
                    .Select(tuple => new object[] { tuple.uowFactory, tuple.expressionBuilder });

                return cases;
            }
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null()
        {
            // Act
            Action action = () => new HandleIsTenantQuery(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void IsHandler() => typeof(HandleIsTenantQuery)
            .Should().Implement<IRequestHandler<IsTenantQuery, bool>>();

        public static IEnumerable<object[]> HandleCases
        {
            get
            {
                {
                    yield return new object[]
                    {
                        Enumerable.Empty<Account>(),
                        TenantId.New(),
                        false,
                        "Account data store is empty"
                    };
                }

                {
                    yield return new object[]
                    {
                        Enumerable.Empty<Account>(),
                        TenantId.Empty,
                        false,
                        "Account data store is empty"
                    };
                }
                {
                    TenantId tenantId = TenantId.New();
                    Faker<Account> accountFaker = new Faker<Account>()
                        .CustomInstantiator(faker => new Account(id: AccountId.New(),
                                                                 name: faker.Person.FullName,
                                                                 username: UserName.From(faker.Person.UserName),
                                                                 email: Email.From(faker.Internet.Email()),
                                                                 passwordHash: Password.From(faker.Internet.Password()),
                                                                 salt: faker.Lorem.Word(),
                                                                 tenantId: tenantId))
                        ;

                    IEnumerable<Account> accounts = accountFaker.GenerateLazy(10);
                    yield return new object[]
                    {
                        accounts,
                        TenantId.Empty,
                        false,
                        $"<{TenantId.Empty}> is not a tenant id"
                    };

                    yield return new object[]
                    {
                        accounts,
                        TenantId.New(),
                        false,
                        "The searched account id is not a tenant"
                    };

                    yield return new object[]
                    {
                        accounts,
                        tenantId,
                        true,
                        $"Account id <{tenantId}> is a tenant id"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(HandleCases))]
        public async Task Handle(IEnumerable<Account> accounts, TenantId tenantId, bool expectedResult, string reason)
        {
            _outputHelper.WriteLine($"Account datastore : {SerializeObject(accounts, Formatting.Indented)}");
            _outputHelper.WriteLine($"Searched tenant id : {tenantId}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(accounts);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            IsTenantQuery request = new(tenantId);
            // Act
            bool isTenant = await _sut.Handle(request, default)
                .ConfigureAwait(false);

            // Assert
            isTenant.Should()
                .Be(expectedResult, reason);
        }
    }
}
