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
            builder.UseSqlite(databaseFixture.Connection);

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options);
                context.Database.EnsureCreated();
                return context;
            });

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
                    {
                        Name = "Bruce Wayne",
                        Email = "Bruce@wayne-entreprise.com",
                        UserName = "Batman",
                        PasswordHash = "CapedCrusader",
                        Salt = "the_kryptonian"
                    };
                    yield return new object[]
                    {
                        bruceWayne,
                        1.October(2011).AddHours(12).AddMinutes(30),
                        new LoginInfo {Username = bruceWayne.UserName, Password = bruceWayne.PasswordHash},
                        (Expression<Func<AccountInfo, bool>>)(info => info.Username == bruceWayne.UserName
                            && info.Name == bruceWayne.Name
                            && !info.Claims.Any()
                        )
                    };
                }

                {
                    DateTimeOffset utcNow = 1.October(2011).AddHours(12).AddMinutes(30);
                    Role superHero = new Role { Code = "SuperHero" };
                    Account clarkKent = new Account
                    {
                        Email = "clark.kent@smallville.com",
                        UserName = "Superman",
                        PasswordHash = "StrongestManAlive",
                        Salt = "the_kryptonian"
                    };
                    clarkKent.AddOrUpdateClaim(type : "superstrength", value :"150", utcNow);
                    yield return new object[]
                    {
                        clarkKent,
                        utcNow,
                        new LoginInfo {Username = clarkKent.UserName, Password = clarkKent.PasswordHash},
                        (Expression<Func<AccountInfo, bool>>)(info => info.Username == clarkKent.UserName
                            && info.Name == clarkKent.UserName
                            && info.Claims.Once()
                            && info.Claims.Once(claim => claim.Type == "superstrength" && claim.Value == "150")
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetOneUserByUsernameAndPasswordQueryCases))]
        public async Task GivenUserExists_Handlers_Returns_Info(Account user, DateTimeOffset utcNow, LoginInfo loginInfo, Expression<Func<AccountInfo, bool>> resultExpectation)
        {
            // Arrange

            _dateTimeServiceMock.Setup(mock => mock.UtcNowOffset()).Returns(utcNow);
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(user);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            // Act
            Option<AccountInfo> optionalUser = await _sut.Handle(new GetOneAccountByUsernameAndPasswordQuery(loginInfo), default)
                .ConfigureAwait(false);

            // Assert
            _dateTimeServiceMock.Verify(mock => mock.UtcNowOffset(), Times.Once);
            optionalUser.Match(
                some : userInfo => userInfo.Should().Match(resultExpectation),
                none : () => throw new NotImplementedException("This case should not happen"));
        }
    }
}
