using AutoMapper.QueryableExtensions;

using Bogus;

using FluentAssertions;

using Identity.CQRS.Handlers.Queries.Accounts;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores;
using Identity.Objects;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using MediatR;

using Microsoft.EntityFrameworkCore;

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

namespace Identity.CQRS.UnitTests.Handlers.Accounts
{
    [UnitTest]
    [Feature("Handlers")]
    [Feature("Accounts")]
    public class HandleIsTenantQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private HandleIsTenantQuery _sut;

        public HandleIsTenantQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
            builder.UseInMemoryDatabase($"{Guid.NewGuid()}");

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleIsTenantQuery(_uowFactory);
        }

        public async void Dispose()
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
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new HandleIsTenantQuery(null);
#pragma warning restore IDE0039 // Utiliser une fonction locale

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
                        Guid.NewGuid(),
                        false,
                        "Account data store is empty"
                    };
                }

                {
                    yield return new object[]
                    {
                        Enumerable.Empty<Account>(),
                        Guid.Empty,
                        false,
                        "Account data store is empty"
                    };
                }
                {
                    Guid tenantId = Guid.NewGuid();
                    Faker<Account> accountFaker = new Faker<Account>()
                        .CustomInstantiator(faker => new Account(id: Guid.NewGuid(),
                            name: faker.Person.FullName,
                            username: faker.Person.UserName,
                            email: faker.Internet.Email(),
                            passwordHash: faker.Lorem.Word(),
                            salt: faker.Lorem.Word(),
                            tenantId: tenantId))
                        ;

                    IEnumerable<Account> accounts = accountFaker.GenerateLazy(10);
                    yield return new object[]
                    {
                        accounts,
                        Guid.Empty,
                        false,
                        $"<{Guid.Empty}> is not a tenant id"
                    };

                    yield return new object[]
                    {
                        accounts,
                        Guid.NewGuid(),
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
        public async Task Handle(IEnumerable<Account> accounts, Guid accountId, bool expectedResult, string reason)
        {
            _outputHelper.WriteLine($"Account datastore : {SerializeObject(accounts, Formatting.Indented)}");
            _outputHelper.WriteLine($"Searched tenant id : {accountId}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(accounts);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            IsTenantQuery request = new IsTenantQuery(accountId);
            // Act
            bool isTenant = await _sut.Handle(request, default)
                .ConfigureAwait(false);

            // Assert
            isTenant.Should()
                .Be(expectedResult, reason);
        }
    }
}
