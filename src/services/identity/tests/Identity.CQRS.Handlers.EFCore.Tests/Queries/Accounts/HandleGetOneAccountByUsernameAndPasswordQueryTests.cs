using Bogus;

using FluentAssertions;
using FluentAssertions.Extensions;

using Identity.CQRS.Handlers.Queries;
using Identity.CQRS.Handlers.Queries.Accounts;
using Identity.CQRS.Queries;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores;
using Identity.DTO;
using Identity.Mapping;
using Identity.Objects;
using MedEasy.Abstractions;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Moq;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace Identity.CQRS.UnitTests.Handlers.Queries.Accounts
{
    [UnitTest]
    [Feature("Identity")]
    public class HandleGetOneAccountByUsernameAndPasswordQueryTests : IAsyncDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private EFUnitOfWorkFactory<IdentityContext> _uowFactory;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IMediator> _mediatorMock;
        private HandleGetOneAccountInfoByUsernameAndPasswordQuery _sut;

        public HandleGetOneAccountByUsernameAndPasswordQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture databaseFixture)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
            string dbName = $"{Guid.NewGuid()}";
            builder.UseInMemoryDatabase($"{dbName}");

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) => new IdentityContext(options));

            _dateTimeServiceMock = new Mock<IDateTimeService>(Strict);

            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new HandleGetOneAccountInfoByUsernameAndPasswordQuery(_uowFactory, expressionBuilder: AutoMapperConfig.Build().ExpressionBuilder, _dateTimeServiceMock.Object, _mediatorMock.Object);
        }

        public async ValueTask DisposeAsync()
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Account>().Clear();
            await uow.SaveChangesAsync().ConfigureAwait(false);

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

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<HashPasswordWithPredefinedSaltAndIterationQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(info.Password);

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
                        new LoginInfo {Username = bruceWayne.Username, Password = bruceWayne.PasswordHash},
                        (Expression<Func<AccountInfo, bool>>)(info => info.Username == bruceWayne.Username
                            && info.Name == bruceWayne.Name
                            && !info.Claims.Any()
                        )
                    };
                }

                {
                    DateTime utcNow = 1.October(2011).AddHours(12).AddMinutes(30);

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
        public async Task GivenUserExists_Handlers_Returns_Info(Account account, LoginInfo loginInfo, Expression<Func<AccountInfo, bool>> resultExpectation)
        {
            // Arrange

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<HashPasswordWithPredefinedSaltAndIterationQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(loginInfo.Password);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(account);

                await uow.SaveChangesAsync()
                         .ConfigureAwait(false);
            }

            // Act
            Option<AccountInfo> optionalUser = await _sut.Handle(new GetOneAccountByUsernameAndPasswordQuery(loginInfo), default)
                                                         .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<HashPasswordWithPredefinedSaltAndIterationQuery>(query => query.Data.password == loginInfo.Password), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            optionalUser.HasValue.Should()
                                 .BeTrue();
            optionalUser.MatchSome(accountInfo => accountInfo.Should().Match(resultExpectation));
        }
    }
}
