namespace Identity.CQRS.UnitTests.Handlers.Commands
{
    using Bogus;

    using FluentAssertions;

    using Identity.CQRS.Commands;
    using Identity.CQRS.Handlers.Commands;
    using Identity.DataStores;
    using Identity.Ids;
    using Identity.Objects;
    using Identity.ValueObjects;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

    using Microsoft.IdentityModel.Tokens;

    using Moq;

    using NodaTime;
    using NodaTime.Testing;

    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;

    using Xunit;

    using static Moq.MockBehavior;

    using Claim = System.Security.Claims.Claim;

    public class HandleInvalidateAccessTokenByUsernameCommandTests : IAsyncDisposable, IClassFixture<SqliteEfCoreDatabaseFixture<IdentityDataStore>>
    {
        private Mock<IClock> _datetimeServiceMock;
        private IUnitOfWorkFactory _uowFactory;
        private HandleInvalidateAccessTokenByUsernameCommand _sut;

        public HandleInvalidateAccessTokenByUsernameCommandTests(SqliteEfCoreDatabaseFixture<IdentityDataStore> databaseFixture)
        {
            _datetimeServiceMock = new Mock<IClock>(Strict);
            _uowFactory = new EFUnitOfWorkFactory<IdentityDataStore>(databaseFixture.OptionsBuilder.Options, (options) =>
            {
                IdentityDataStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleInvalidateAccessTokenByUsernameCommand(clock: _datetimeServiceMock.Object, uowFactory: _uowFactory);
        }

        public async ValueTask DisposeAsync()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Clear();
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _uowFactory = null;
            _datetimeServiceMock = null;
            _sut = null;
        }

        [Fact]
        public async Task InvalidateUnknownUsername_Returns_NotFound()
        {
            // Arrange
            InvalidateAccessTokenByUsernameCommand cmd = new(UserName.From("thejoker"));

            // Act
            InvalidateAccessCommandResult cmdResult = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(InvalidateAccessCommandResult.Failed_NotFound, "Accounts datastore is empty");
        }

        [Fact]
        public async Task InvalidateUnknownUsername_Returns_Ok_When_User_Try_Invalidate_Itself()
        {
            // Arrange
            Faker faker = new();
            UserName username = UserName.From("administrator");
            SecurityToken securityToken = new JwtSecurityToken(
                issuer: "server",
                audience: "api",
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, username.Value)
                }
            );
            Account account = new(
                username: username,
                passwordHash: faker.Lorem.Word(),
                salt: faker.Lorem.Word(),
                name: "Victor Jones",
                id: AccountId.New(),
                email: Email.From("victor.jones@home.dc")
            );
            account.ChangeRefreshToken(securityToken.ToString());

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(account);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            InvalidateAccessTokenByUsernameCommand cmd = new(username);

            // Act
            InvalidateAccessCommandResult cmdResult = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(InvalidateAccessCommandResult.Done, "A user is allowed to invalidate its own account access");

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                string refreshTokenInStore = await uow.Repository<Account>()
                    .SingleAsync(x => x.RefreshToken, x => x.Id == account.Id)
                    .ConfigureAwait(false);

                refreshTokenInStore.Should()
                    .BeNull("The refresh token must be reset");
            }
        }

        [Fact]
        public void IsCommandHandler() => typeof(HandleInvalidateAccessTokenByUsernameCommand).Should()
            .Implement<IRequestHandler<InvalidateAccessTokenByUsernameCommand, InvalidateAccessCommandResult>>();
    }
}
