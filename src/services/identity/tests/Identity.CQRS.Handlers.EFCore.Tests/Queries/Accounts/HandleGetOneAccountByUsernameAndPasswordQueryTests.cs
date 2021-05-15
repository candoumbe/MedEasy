namespace Identity.CQRS.UnitTests.Handlers.Queries.Accounts
{

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using Identity.CQRS.Handlers.Queries.Accounts;
    using Identity.CQRS.Queries;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.Ids;
    using Identity.Mapping;
    using Identity.Objects;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

    using Microsoft.EntityFrameworkCore;

    using Moq;

    using NodaTime;
    using NodaTime.Extensions;
    using NodaTime.Testing;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Moq.MockBehavior;

    [UnitTest]
    [Feature("Identity")]
    public class HandleGetOneAccountByUsernameAndPasswordQueryTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<IdentityContext>>
    {
        private ITestOutputHelper _outputHelper;
        private EFUnitOfWorkFactory<IdentityContext> _uowFactory;
        private readonly Mock<IMediator> _mediatorMock;
        private HandleGetOneAccountInfoByUsernameAndPasswordQuery _sut;

        public HandleGetOneAccountByUsernameAndPasswordQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityContext> databaseFixture)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(databaseFixture.OptionsBuilder.Options,
                                                                   (options) =>
                                                                   {
                                                                       IdentityContext context = new IdentityContext(options, new FakeClock(new Instant()));
                                                                       context.Database.EnsureCreated();

                                                                       return context;
                                                                   });

            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new HandleGetOneAccountInfoByUsernameAndPasswordQuery(_uowFactory, expressionBuilder: AutoMapperConfig.Build().ExpressionBuilder, mediator: _mediatorMock.Object);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Account>().Clear();
            await uow.SaveChangesAsync().ConfigureAwait(false);

            _outputHelper = null;
            _sut = null;
            _uowFactory = null;
        }

        [Fact]
        public async Task GivenNoUser_Handler_Returns_None()
        {
            // Arrange
            LoginInfo info = new() { Username = "Bruce", Password = "CapedCrusader" };
            GetOneAccountByUsernameAndPasswordQuery query = new(info);

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
                        new LoginInfo {Username = bruceWayne.Username, Password = bruceWayne.PasswordHash},
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
