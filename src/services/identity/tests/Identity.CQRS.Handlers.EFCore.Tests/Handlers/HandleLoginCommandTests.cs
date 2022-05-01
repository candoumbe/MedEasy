namespace Identity.CQRS.Handlers.EFCore.Tests.Handlers;

using Bogus;

using FluentAssertions;

using Identity.CQRS.Commands.v1;
using Identity.CQRS.Handlers.Commands.v1;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using Identity.DTO.v1;
using MedEasy.ValueObjects;

using MediatR;

using Moq;

using Optional;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Categories;

using static Moq.It;
using static Moq.MockBehavior;

[UnitTest]
public class HandleLoginCommandTests
{
    public static readonly Faker Faker = new();
    private readonly Mock<IMediator> _mediatorMock;
    private readonly HandleLoginCommand _sut;

    public HandleLoginCommandTests()
    {
        _mediatorMock = new Mock<IMediator>(Strict);
        _sut = new HandleLoginCommand(_mediatorMock.Object);
    }

    /// <summary>
    /// <para>
    /// Given a valid login command <br/>
    /// Given user and/or password do not match <br/>
    /// </para>
    /// <para>Handler should return none</para>
    /// </summary>
    [Fact]
    public async Task Given_LoginCommand_is_valid_and_user_exists_when_password_does_not_match_Then_Return_None()
    {
        // Arrange
        LoginInfo loginInfo = new()
        {
            UserName = UserName.From(Faker.Internet.UserName()),
            Password = Password.From(Faker.Internet.Password())
        };

        JwtInfos jwtInfos = new()
        {
            Issuer = Faker.Internet.DomainName(),
            Audiences = Enumerable.Range(1, Faker.Random.Int(1, 10))
                                  .Select(_ => Faker.Internet.DomainName()),
            AccessTokenLifetime = Faker.Random.Int(1, 5),
            RefreshTokenLifetime = Faker.Random.Int(1, 5),
        };

        LoginCommand command = new((loginInfo, jwtInfos, string.Empty));

        _mediatorMock.Setup(mock => mock.Send(Is<GetOneAccountByUsernameAndPasswordQuery>(cmd => cmd.Data.UserName == loginInfo.UserName && cmd.Data.Password == loginInfo.Password), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Option.None<AccountInfo>());


        // Act
        Option<BearerTokenInfo> result = await _sut.Handle(command, default)
                                                   .ConfigureAwait(false);

        // Assert
        result.HasValue.Should().BeFalse();
    }
}
