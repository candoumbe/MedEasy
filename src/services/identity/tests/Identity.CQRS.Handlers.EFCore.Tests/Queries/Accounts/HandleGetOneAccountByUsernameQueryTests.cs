namespace Identity.CQRS.UnitTests.Handlers.Queries.Accounts;

using FluentAssertions;
using FluentAssertions.Extensions;

using Identity.CQRS.Handlers.Queries;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores;
using Identity.DTO;
using Identity.Ids;
using Identity.Mapping;
using Identity.Objects;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using Microsoft.EntityFrameworkCore;

using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;

using Optional;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

[UnitTest]
[Feature("Identity")]
public class HandleGetOneAccountByUsernameQueryTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<IdentityContext>>
{
    private ITestOutputHelper _outputHelper;
    private EFUnitOfWorkFactory<IdentityContext> _uowFactory;
    private HandleGetOneAccountInfoByUsernameQuery _sut;

    public HandleGetOneAccountByUsernameQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityContext> databaseFixture)
    {
        _outputHelper = outputHelper;

        _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(databaseFixture.OptionsBuilder.Options,
                                                               (options) =>
                                                               {
                                                                   IdentityContext context = new (options, new FakeClock(new Instant()));
                                                                   context.Database.EnsureCreated();

                                                                   return context;
                                                               });

        _sut = new HandleGetOneAccountInfoByUsernameQuery(_uowFactory, expressionBuilder: AutoMapperConfig.Build().ExpressionBuilder);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
        uow.Repository<Account>().Clear();
        await uow.SaveChangesAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task GivenNoUser_Handler_Returns_None()
    {
        // Arrange
        string username = $"{Guid.NewGuid()}";
        GetOneAccountInfoByUsernameQuery query = new(username);

        // Act
        Option<AccountInfo> optionalUser = await _sut.Handle(query, default)
                                                     .ConfigureAwait(false);

        // Assert
        optionalUser.HasValue.Should()
                             .BeFalse("No user in the store");
    }

    public static IEnumerable<object[]> GetOneUserByUsernameAndPasswordQueryCases
    {
        get
        {
            {
                Account bruceWayne = new(
                    name: "Bruce Wayne",
                    id: AccountId.New(),
                    email: "Bruce@wayne-entreprise.com",
                    username: "Batman",
                    passwordHash: "CapedCrusader",
                    salt: "the_kryptonian"
                );

                yield return new object[]
                {
                    bruceWayne,
                    bruceWayne.Username,
                    (Expression<Func<AccountInfo, bool>>)(info => info.Username == bruceWayne.Username
                        && info.Name == bruceWayne.Name
                        && !info.Claims.Any()
                    )
                };
            }

            {
                Instant utcNow = 1.October(2011).Add(12.Hours().And(30.Minutes())).AsUtc().ToInstant();

                Account clarkKent = new(
                    id: AccountId.New(),
                    email: "clark.kent@smallville.com",
                    username: "Superman",
                    passwordHash: "StrongestManAlive",
                    salt: "the_kryptonian"
                );
                clarkKent.AddOrUpdateClaim(type: "superstrength", value: "150", utcNow);
                yield return new object[]
                {
                    clarkKent,
                    clarkKent.Username,
                    (Expression<Func<AccountInfo, bool>>)(info => info.Username == clarkKent.Username
                        && info.Email == clarkKent.Email
                        && info.Claims.Once()
                        && info.Claims.Once(claim => claim.Type == "superstrength" && claim.Value == "150")
                    )
                };
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetOneUserByUsernameAndPasswordQueryCases))]
    public async Task GivenUserExists_Handlers_Returns_Info(Account account, string username, Expression<Func<AccountInfo, bool>> resultExpectation)
    {
        // Arrange

        using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
        {
            uow.Repository<Account>().Create(account);

            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);
        }

        // Act
        Option<AccountInfo> optionalUser = await _sut.Handle(new (username), default)
                                                     .ConfigureAwait(false);

        // Assert
        optionalUser.HasValue.Should()
                             .BeTrue();
        optionalUser.MatchSome(accountInfo => accountInfo.Should().Match(resultExpectation));
    }
}
