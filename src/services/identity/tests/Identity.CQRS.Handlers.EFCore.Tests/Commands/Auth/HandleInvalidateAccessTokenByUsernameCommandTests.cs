using Bogus;
using FluentAssertions;
using Identity.CQRS.Commands;
using Identity.CQRS.Handlers.Commands;
using Identity.DataStores.SqlServer;
using Identity.Objects;
using MedEasy.Abstractions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Xunit;
using static Moq.MockBehavior;
using Claim = System.Security.Claims.Claim;

namespace Identity.CQRS.UnitTests.Handlers.Commands
{
    public class HandleInvalidateAccessTokenByUsernameCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private Mock<IDateTimeService> _datetimeServiceMock;
        private IUnitOfWorkFactory _uowFactory;
        private HandleInvalidateAccessTokenByUsernameCommand _sut;

        public HandleInvalidateAccessTokenByUsernameCommandTests(SqliteDatabaseFixture databaseFixture)
        {
            _datetimeServiceMock = new Mock<IDateTimeService>(Strict);
            DbContextOptionsBuilder<IdentityContext> dbContextBuilderOptionsBuilder = new DbContextOptionsBuilder<IdentityContext>()
                .UseSqlite(databaseFixture.Connection);

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(dbContextBuilderOptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options);
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleInvalidateAccessTokenByUsernameCommand(datetimeService: _datetimeServiceMock.Object, uowFactory: _uowFactory);
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
            _datetimeServiceMock = null;
            _sut = null;
        }

        [Fact]
        public async Task InvalidateUnknownUsername_Returns_NotFound()
        {
            // Arrange
            SecurityToken securityToken = new JwtSecurityToken(
                issuer: "server",
                audience: "api",
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, "administrator")
                }
            );

            InvalidateAccessTokenByUsernameCommand cmd = new InvalidateAccessTokenByUsernameCommand("thejoker");

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
            Faker faker = new Faker();
            const string username = "administrator";
            SecurityToken securityToken = new JwtSecurityToken(
                issuer: "server",
                audience: "api",
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, username)
                }
            );
            Account account = new Account
            (
                username: username,
                passwordHash : faker.Lorem.Word(),
                salt : faker.Lorem.Word(),
                name : "Victor Jones",
                id: Guid.NewGuid(),
                email: "victor.jones@home.dc"
            );
            account.ChangeRefreshToken(securityToken.ToString());

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(account);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            InvalidateAccessTokenByUsernameCommand cmd = new InvalidateAccessTokenByUsernameCommand(username);

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
