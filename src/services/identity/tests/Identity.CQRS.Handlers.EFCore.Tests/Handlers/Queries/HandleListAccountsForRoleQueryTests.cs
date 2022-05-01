namespace Identity.CQRS.Handlers.EFCore.Tests.Handlers.Queries
{
    using AutoMapper.QueryableExtensions;

    using Bogus;

    using FluentAssertions;

    using Identity.CQRS.Handlers.Queries;
    using Identity.CQRS.Queries.Roles;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.Ids;
    using Identity.Mapping;
    using Identity.Objects;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;
    using MedEasy.ValueObjects;

    using MediatR;

    using Moq;

    using NodaTime;
    using NodaTime.Testing;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;

    public class HandleListAccountsForRoleQueryTests : IClassFixture<SqliteEfCoreDatabaseFixture<IdentityDataStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;

        private readonly Faker<Role> _roleFaker;

        private readonly HandleListAccountsForRoleQuery _sut;

        public HandleListAccountsForRoleQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityDataStore> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<IdentityDataStore>(database.OptionsBuilder.Options, (options) =>
            {
                IdentityDataStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _roleFaker = new Faker<Role>()
                    .CustomInstantiator((faker) => new Role(
                        id: RoleId.New(),
                        code: faker.Lorem.Word()
                    ));

            _sut = new HandleListAccountsForRoleQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IExpressionBuilder[] expressionBuilderCases = { null, Mock.Of<IExpressionBuilder>() };

                return uowFactorieCases
                    .CrossJoin(expressionBuilderCases, (uowFactory, expressionBuilder) => (uowFactory, expressionBuilder))
                    .Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder == null)
                    .Select(tuple => new object[] { tuple.uowFactory, tuple.expressionBuilder });
            }
        }


        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {unitOfWorkFactory == null}");
            _outputHelper.WriteLine($"{nameof(expressionBuilder)} is null : {expressionBuilder == null}");

            // Act
            Action action = () => new HandleListRolesForAccountQuery(unitOfWorkFactory, expressionBuilder);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void IsHandler() => typeof(HandleListRolesForAccountQuery).Should()
                                                                         .Implement<IRequestHandler<ListRolesForAccountQuery, Option<IEnumerable<RoleInfo>>>>();

        [Fact]
        public async Task Handle_returns_none_when_account_does_not_exist()
        {
            // Act
            Option<IEnumerable<AccountInfo>> optionalResource = await _sut.Handle(new ListAccountsForRoleQuery(RoleId.New()), default)
                .ConfigureAwait(false);

            // Assert
            optionalResource.HasValue.Should()
                            .BeFalse();
        }

        [Fact]
        public async Task Handle_returns_empty_when_no_account_attached_to_the_role()
        {
            // Arrange
            Role role = _roleFaker.Generate();

            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Role>().Create(role);

            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);

            ListAccountsForRoleQuery query = new(role.Id);

            // Act
            Option<IEnumerable<AccountInfo>> optionAccounts = await _sut.Handle(query, default)
                                                                     .ConfigureAwait(false);

            // Assert
            optionAccounts.HasValue.Should()
                                .BeTrue("the role exists and was found");

            optionAccounts.MatchSome(accounts => accounts.Should().BeEmpty("no account is attached to the specified role"));
        }

        [Fact]
        public async Task Handle_returns_roles_when_account_does_exist()
        {
            // Arrange
            Role role = _roleFaker.Generate();
            role.AddOrUpdateClaim("documents", "read");

            Faker faker = new();

            Account account = new(AccountId.New(),
                                  UserName.From(faker.Internet.UserName()),
                                  Email.From(faker.Internet.Email()),
                                  Password.From(faker.Internet.Password()),
                                  "a_salt");

            account.AddRole(role);

            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Role>().Create(role);
            uow.Repository<Account>().Create(account);

            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);

            ListAccountsForRoleQuery query = new(role.Id);

            // Act
            Option<IEnumerable<AccountInfo>> optionAccounts = await _sut.Handle(query, default)
                                                                  .ConfigureAwait(false);

            // Assert
            optionAccounts.HasValue.Should()
                                .BeTrue("the role exists and was found");

            optionAccounts.MatchSome(accounts =>
            {
                accounts.Should()
                     .HaveCount(1).And
                     .Contain(acc => acc.Id == account.Id, "account has the specified role");
            });
        }
    }
}
