using AutoMapper;
using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.CQRS.Commands.Accounts;
using Identity.CQRS.Events.Accounts;
using Identity.CQRS.Handlers.Commands.Accounts;
using Identity.CQRS.Queries;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using Identity.Mapping;
using Identity.Objects;
using MedEasy.CQRS.Core.Commands.Results;
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
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace Identity.CQRS.UnitTests.Handlers.Accounts
{
    [UnitTest]
    [Feature("Accounts")]
    [Feature("Handlers")]
    public class HandleCreateAccountInfoCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<IMapper> _mapperMock;
        private Mock<IMediator> _mediatorMock;
        private HandleCreateAccountInfoCommand _sut;

        public HandleCreateAccountInfoCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture databaseFixture)
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
            _mapperMock = new Mock<IMapper>(Strict);
            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new HandleCreateAccountInfoCommand(_uowFactory, mapper: _mapperMock.Object, mediator: _mediatorMock.Object);
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Delete(x => true);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _uowFactory = null;
            _sut = null;
            _mapperMock = null;
            _mediatorMock = null;
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IMapper[] mapperCases = { null, Mock.Of<IMapper>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };

                IEnumerable<object[]> cases = uowFactorieCases
                    .CrossJoin(mapperCases, (uowFactory, mapper) => ((uowFactory, mapper)))
                    //.Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder == null)
                    .Select(((IUnitOfWorkFactory uowFactory, IMapper mapper) tuple) => new { tuple.uowFactory, tuple.mapper })
                    .CrossJoin(mediatorCases, (a, mediator) => ((a.uowFactory, a.mapper, mediator)))
                    //.Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder != null || tuple.mediator != null)
                    .Select(((IUnitOfWorkFactory uowFactory, IMapper mapper, IMediator mediator) tuple) => new { tuple.uowFactory, tuple.mapper, tuple.mediator })
                    .Where(tuple => tuple.uowFactory == null || tuple.mapper == null || tuple.mediator == null)
                    .Select(tuple => (new object[] { tuple.uowFactory, tuple.mapper, tuple.mediator }));

                return cases;
            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper, IMediator mediator)
        {
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {(unitOfWorkFactory == null)}");
            _outputHelper.WriteLine($"{nameof(mapper)} is null : {(mapper == null)}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {(mediator == null)}");
            // Act
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new HandleCreateAccountInfoCommand(unitOfWorkFactory, mapper, mediator);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GivenUsernameAlreadyExists_Handler_Returns_Conflict()
        {
            // Arrange
            Guid resourceId = Guid.NewGuid();
            Account existingAccount = new Account
            {
                UserName = "thebatman",
                PasswordHash = "fjeiozfjzfdcvqcnjifozjffkjioj",
                Salt = "some_salt_and_pepper"
            };

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(existingAccount);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            NewAccountInfo newResourceInfo = new NewAccountInfo
            {
                Username = existingAccount.UserName,
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader",
                Email = "b.wayne@gotham.com"
            };

            _mapperMock.Setup(mock => mock.Map<NewAccountInfo, Account>(It.IsAny<NewAccountInfo>()))
                .Returns((NewAccountInfo newResource) => AutoMapperConfig.Build().CreateMapper().Map<NewAccountInfo, Account>(newResource));

            _mapperMock.Setup(mock => mock.Map<Account, AccountInfo>(It.IsAny<Account>()))
                .Returns((Account newEntity) => AutoMapperConfig.Build().CreateMapper().Map<Account, AccountInfo>(newEntity));

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<AccountCreated>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            CreateAccountInfoCommand cmd = new CreateAccountInfoCommand(newResourceInfo);

            // Act
            Option<AccountInfo, CreateCommandResult> optionalCreatedResource = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            optionalCreatedResource.HasValue.Should()
                .BeFalse($"The handler cannot create account with a duplicate {nameof(AccountInfo.Username)}");

            optionalCreatedResource.MatchNone(cmdError =>
            {
                cmdError.Should()
                    .Be(CreateCommandResult.Failed_Conflict);
            });

            _mapperMock.Verify(mock => mock.Map<NewAccountInfo, Account>(It.IsAny<NewAccountInfo>()), Times.Never, "Duplicated Username");

            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<AccountCreated>(), It.IsAny<CancellationToken>()), Times.Never,
                "No resource created");
        }

        [Fact]
        public async Task GivenPasswordAndConfirmPasswordDoesNotMatch_Handler_Returns_Conflict()
        {
            // Arrange

            NewAccountInfo newResourceInfo = new NewAccountInfo
            {
                Username = "thebatman",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecrusader",
                Email = "b.wayne@gotham.com"
            };

            CreateAccountInfoCommand cmd = new CreateAccountInfoCommand(newResourceInfo);

            // Act
            Option<AccountInfo, CreateCommandResult> optionalCreatedResource = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            optionalCreatedResource.HasValue.Should()
                .BeFalse($"{nameof(NewAccountInfo.Password)} and {nameof(NewAccountInfo.ConfirmPassword)} do not match");

            optionalCreatedResource.MatchNone(cmdError =>
            {
                cmdError.Should()
                    .Be(CreateCommandResult.Failed_Conflict);
            });

            _mapperMock.Verify(mock => mock.Map<NewAccountInfo, Account>(It.IsAny<NewAccountInfo>()), Times.Never, "Duplicated Username");

            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<AccountCreated>(), It.IsAny<CancellationToken>()), Times.Never,
                "No resource created");
        }

        [Fact]
        public async Task GivenCorrectData_Handler_Create_Account()
        {
            // Arrange
            NewAccountInfo newAccount = new NewAccountInfo
            {
                Name ="Bruce Wayne",
                Username = "thebatman",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader",
                Email = "b.wayne@gotham.com"
            };

            _mapperMock.Setup(mock => mock.Map<NewAccountInfo, Account>(It.IsAny<NewAccountInfo>()))
                .Returns((NewAccountInfo newResource) => AutoMapperConfig.Build().CreateMapper().Map<NewAccountInfo, Account>(newResource));

            _mapperMock.Setup(mock => mock.Map<Account, AccountInfo>(It.IsAny<Account>()))
                .Returns((Account newEntity) => AutoMapperConfig.Build().CreateMapper().Map<Account, AccountInfo>(newEntity));

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<AccountCreated>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<HashPasswordQuery>(), It.IsAny<CancellationToken>()))
                .Returns((HashPasswordQuery query, CancellationToken ct) => Task.FromResult((salt: query.Data, passwordHash: new string(query.Data.Reverse().ToArray()))));

            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAccountByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetOneAccountByIdQuery query, CancellationToken ct) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        DateTimeOffset now = 10.January(2010).AddHours(10).AddMinutes(37);
                        var optionalUser = await uow.Repository<Account>()
                            .SingleOrDefaultAsync(
                                x => new
                                {
                                    x.UserName,
                                    x.Email,
                                    Roles = x.Roles
                                        .Select(r => r.Id)
                                        .ToList(),
                                    AccountClaims = x.Claims
                                        .Where(uc => uc.Start <= now && (uc.End == null || now <= uc.End))
                                        .Select(uc => new ClaimInfo { Type = uc.Claim.Type, Value = uc.Claim.Value })
                                        .ToList()
                                },
                                (Account x) => x.UUID == query.Data,
                                ct
                            )
                            .ConfigureAwait(false);

                        return await optionalUser.Match(
                            some: async account =>
                            {
                                IEnumerable<ClaimInfo> claimsFromRoles = await uow.Repository<RoleClaim>()
                                    .WhereAsync(
                                        rc => new ClaimInfo { Type = rc.Claim.Type, Value = rc.Claim.Value },
                                        (RoleClaim rc) => account.Roles.Any(roleId => roleId == rc.RoleId) && !account.AccountClaims.Any(uc => uc.Type == rc.Claim.Type),
                                        ct
                                    )
                                    .ConfigureAwait(false);

                                AccountInfo accountInfo = new AccountInfo
                                {
                                    Username = account.UserName,
                                    Email = account.Email,
                                    Claims = account.AccountClaims
                                        .Union(claimsFromRoles)
                                };
                                return Option.Some(accountInfo);
                            },
                            none: () => Task.FromResult(Option.None<AccountInfo>())
                        );
                    }
                });

            CreateAccountInfoCommand cmd = new CreateAccountInfoCommand(newAccount);

            // Act
            Option<AccountInfo, CreateCommandResult> optionalResult = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            optionalResult.HasValue.Should()
                    .BeTrue("Data provided to create an account are correct");

            optionalResult.MatchSome(async resource =>
                {
                    resource.Name.Should()
                        .Be(newAccount.Name);

                    resource.Username.Should()
                        .Be(newAccount.Username);
                    resource.Email.Should()
                            .Be(newAccount.Email);
                    resource.Claims.Should()
                            .NotContainNulls().And
                            .NotContain(claim => claim.Type.EndsWith(".api"));

                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Account newEntity = await uow.Repository<Account>().SingleAsync(x => x.UserName == newAccount.Username)
                            .ConfigureAwait(false);

                        newEntity.PasswordHash.Should()
                            .NotBeNullOrWhiteSpace().And
                            .NotBe(newAccount.Password);
                        newEntity.Salt.Should()
                            .NotBeNullOrWhiteSpace();
                        newEntity.UUID.Should()
                            .NotBeEmpty("UUID must not be empty");
                        newEntity.Email.Should()
                            .Be(resource.Email);
                        newEntity.EmailConfirmed.Should()
                            .BeTrue("Email is confirmed automagically for now");
                        newEntity.IsActive.Should()
                            .BeTrue("Account is active right after registration for now");

                        _mediatorMock.Verify(mock => mock.Send(It.IsAny<HashPasswordQuery>(), It.IsAny<CancellationToken>()), Times.Once);
                        _mediatorMock.Verify(mock => mock.Send(It.Is<HashPasswordQuery>(query => query.Data == newAccount.Password), It.IsAny<CancellationToken>()), Times.Once);
                        _mediatorMock.Verify(mock => mock.Publish(It.IsAny<AccountCreated>(), It.IsAny<CancellationToken>()), Times.Once);
                        _mediatorMock.Verify(mock => mock.Publish(
                            It.Is<AccountCreated>(evt => evt.Data.Id == resource.Id),
                            It.IsAny<CancellationToken>()),
                            Times.Once);
                    }
                });

            _mapperMock.Verify(mock => mock.Map<NewAccountInfo, Account>(It.IsAny<NewAccountInfo>()), Times.Once);
        }
    }
}
