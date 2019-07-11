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
using MedEasy.IntegrationTests.Core;
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
    public class HandleGetOneAccountByUsernameAndPasswordQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private EFUnitOfWorkFactory<IdentityContext> _uowFactory;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private HandleGetOneAccountInfoByUsernameAndPasswordQuery _sut;

        public HandleGetOneAccountByUsernameAndPasswordQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture databaseFixture)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
            string dbName = $"{Guid.NewGuid()}";
            builder.UseInMemoryDatabase($"{dbName}");

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) => new IdentityContext(options));

            _dateTimeServiceMock = new Mock<IDateTimeService>(Strict);

            _sut = new HandleGetOneAccountInfoByUsernameAndPasswordQuery(_uowFactory, expressionBuilder: AutoMapperConfig.Build().ExpressionBuilder, _dateTimeServiceMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _sut = null;
            _uowFactory = null;
            _dateTimeServiceMock = null;
        }

        [Fact]
        public async Task GivenNoUser_Handler_Returns_None()
        {
            // Arrange
            LoginInfo info = new LoginInfo { Username = "Bruce", Password = "CapedCrusader" };
            GetOneAccountByUsernameAndPasswordQuery query = new GetOneAccountByUsernameAndPasswordQuery(info);
            _dateTimeServiceMock.Setup(mock => mock.UtcNowOffset()).Returns(new DateTimeOffset(2008, 5, 10, 15, 0, 0, TimeSpan.Zero));

            // Act
            Option<AccountInfo> optionalUser = await _sut.Handle(query, default)
                .ConfigureAwait(false);

            // Assert
            _dateTimeServiceMock.Verify();
            optionalUser.HasValue.Should()
                .BeFalse("No user in the store");
        }

        public static IEnumerable<object[]> GetOneUserByUsernameAndPasswordQueryCases
        {
            get
            {
                {
                    Account bruceWayne = new Account
                    (
                        name: "Bruce Wayne",
                        id: Guid.NewGuid(),
                        email: "Bruce@wayne-entreprise.com",
                        username: "Batman",
                        passwordHash: "CapedCrusader",
                        salt: "the_kryptonian"
                    );

                    yield return new object[]
                    {
                        bruceWayne,
                        1.October(2011).AddHours(12).AddMinutes(30),
                        new LoginInfo {Username = bruceWayne.Username, Password = bruceWayne.PasswordHash},
                        (Expression<Func<AccountInfo, bool>>)(info => info.Username == bruceWayne.Username
                            && info.Name == bruceWayne.Name
                            && !info.Claims.Any()
                        )
                    };
                }

                {
                    DateTimeOffset utcNow = 1.October(2011).AddHours(12).AddMinutes(30);

                    Account clarkKent = new Account
                    (
                        id: Guid.NewGuid(),
                        email: "clark.kent@smallville.com",
                        username: "Superman",
                        passwordHash: "StrongestManAlive",
                        salt: "the_kryptonian"
                    );
                    clarkKent.AddOrUpdateClaim(type: "superstrength", value: "150", utcNow);
                    yield return new object[]
                    {
                        clarkKent,
                        utcNow,
                        new LoginInfo {Username = clarkKent.Username, Password = clarkKent.PasswordHash},
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
        public async Task GivenUserExists_Handlers_Returns_Info(Account account, DateTimeOffset utcNow, LoginInfo loginInfo, Expression<Func<AccountInfo, bool>> resultExpectation)
        {
            // Arrange

            _dateTimeServiceMock.Setup(mock => mock.UtcNowOffset()).Returns(utcNow);
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(account);
                foreach (KeyValuePair<string, (string value, DateTimeOffset start, DateTimeOffset? end)> kv in account.Claims)
                {
                    Claim claim = uow.Repository<Claim>().Create(new Claim(Guid.NewGuid(), kv.Key, kv.Value.value));
                    uow.Repository<AccountClaim>().Create(new AccountClaim(Guid.NewGuid(), account.Id, claim.Id, kv.Value.value, DateTimeOffset.UtcNow, null));
                }
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            // Act
            Option<AccountInfo> optionalUser = await _sut.Handle(new GetOneAccountByUsernameAndPasswordQuery(loginInfo), default)
                .ConfigureAwait(false);

            // Assert
            _dateTimeServiceMock.Verify(mock => mock.UtcNowOffset(), Times.Once);
            optionalUser.Match(
                some: accountInfo => accountInfo.Should().Match(resultExpectation),
                none: () => throw new NotImplementedException("This case should not happen"));
        }
    }
}
