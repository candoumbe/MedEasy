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
    using MedEasy.Ids;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

    using Microsoft.EntityFrameworkCore.Diagnostics;

    using Moq;

    using NodaTime;
    using NodaTime.Testing;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;

    public class HandleListRolesForAccountQueryTests : IClassFixture<SqliteEfCoreDatabaseFixture<IdentityContext>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;

        private readonly Faker<Account> _accountFaker;

        private readonly HandleListRolesForAccountQuery _sut;


        private class LogQueryInterceptor : DbCommandInterceptor
        {
            public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
            {
                Debug.WriteLine(result.CommandText);

                return base.CommandCreated(eventData, result);
            }
        }

        public HandleListRolesForAccountQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityContext> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(database.OptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _accountFaker = new Faker<Account>()
                    .CustomInstantiator((faker) => new Account(
                        id: AccountId.New(),
                        username: faker.Internet.UserName(),
                        email: faker.Internet.Email(),
                        passwordHash: faker.Internet.Password(),
                        locked: faker.PickRandom(new[] { true, false }),
                        isActive: faker.PickRandom(new[] { true, false }),
                        salt: faker.Lorem.Word(),
                        tenantId: faker.PickRandom(new[] { TenantId.New(), default }),
                        refreshToken: faker.Lorem.Word()
                    ));

            _sut = new HandleListRolesForAccountQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
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
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new HandleListRolesForAccountQuery(unitOfWorkFactory, expressionBuilder);
#pragma warning restore IDE0039 // Utiliser une fonction locale

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
            Option<IEnumerable<RoleInfo>> optionalResource = await _sut.Handle(new ListRolesForAccountQuery(AccountId.New()), default)
                .ConfigureAwait(false);

            // Assert
            optionalResource.HasValue.Should()
                            .BeFalse();
        }

        [Fact]
        public async Task Handle_returns_empty_when_account_does_not_exist()
        {
            // Arrange
            Account account = _accountFaker.Generate();

            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Account>().Create(account);

            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);

            ListRolesForAccountQuery query = new(account.Id);

            // Act
            Option<IEnumerable<RoleInfo>> optionRoles = await _sut.Handle(query, default)
                                                                  .ConfigureAwait(false);

            // Assert
            optionRoles.HasValue.Should()
                                .BeTrue("the account exists and was found");

            optionRoles.MatchSome(roles => roles.Should().BeEmpty("account is not attached to any role"));
        }

        [Fact]
        public async Task Handle_returns_roles_when_account_does_exist()
        {
            // Arrange
            Account account = _accountFaker.Generate();

            Role role = new(RoleId.New(), "administrator");
            role.AddOrUpdateClaim("documents", "read");

            account.AddRole(role);

            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Role>().Create(role);
            uow.Repository<Account>().Create(account);

            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);

            ListRolesForAccountQuery query = new(account.Id);

            // Act
            Option<IEnumerable<RoleInfo>> optionRoles = await _sut.Handle(query, default)
                                                                  .ConfigureAwait(false);

            // Assert
            optionRoles.HasValue.Should()
                                .BeTrue("the account exists and was found");

            optionRoles.MatchSome(roles =>
            {
                roles.Should()
                     .HaveCount(1).And
                     .Contain(r => r.Name == "administrator", "account is part of the administrator");
            });
        }
    }
}
