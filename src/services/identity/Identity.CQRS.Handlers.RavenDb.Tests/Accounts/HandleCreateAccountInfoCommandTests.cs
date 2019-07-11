using AutoMapper;
using FluentAssertions;
using Identity.CQRS.Commands.Accounts;
using Identity.CQRS.Events.Accounts;
using Identity.CQRS.Handlers.RavenDb.Accounts;
using Identity.CQRS.Queries;
using Identity.DTO;
using Identity.Mapping;
using Identity.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MediatR;
using Moq;
using Optional;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using Raven.TestDriver;
using MedEasy.IntegrationTests.Core;
using Raven.Client.Documents.Session;

namespace Identity.CQRS.Handlers.RavenDb.Tests.Accounts
{
    public class HandleCreateAccountInfoCommandTests : IDisposable, IClassFixture<RavenDbFixture>
    {
        private IDocumentStore _documentStore;
        private Mock<IMediator> _mediatorMock;
        private Mock<IMapper> _mapperMock;
        private HandleCreateAccountInfoCommand _sut;
        private readonly ITestOutputHelper _outputHelper;

        public HandleCreateAccountInfoCommandTests(ITestOutputHelper outputHelper, RavenDbFixture ravenDb)
        {
            _outputHelper = outputHelper;
            _documentStore = ravenDb.CreateStore();

            _mediatorMock = new Mock<IMediator>(Strict);

           _sut = new HandleCreateAccountInfoCommand(_documentStore, _mediatorMock.Object, AutoMapperConfig.Build().CreateMapper());
        }

        public void Dispose()
        {
            using (IDocumentSession session = _documentStore.OpenSession())
            {
                IEnumerable<string> ids = session.Query<Account>()
                    .Select(x => x.Id.ToString());

                ids.ForEach(id => session.Delete(id));
            }

            _mediatorMock = null;
            _mapperMock = null;
            _documentStore = null;
            _sut = null;
        }


        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IDocumentStore[] documentStores = { null, Mock.Of<IDocumentStore>() };
                IMapper[] mapperCases = { null, Mock.Of<IMapper>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };

                IEnumerable<object[]> cases = documentStores
                    .CrossJoin(mapperCases, (documentStore, mapper) => ((documentStore, mapper)))
                    //.Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder == null)
                    .Select(((IDocumentStore documentStore, IMapper mapper) tuple) => new { tuple.documentStore, tuple.mapper })
                    .CrossJoin(mediatorCases, (a, mediator) => ((a.documentStore, a.mapper, mediator)))
                    //.Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder != null || tuple.mediator != null)
                    .Select(((IDocumentStore documentStore, IMapper mapper, IMediator mediator) tuple) => new { tuple.documentStore, tuple.mapper, tuple.mediator })
                    .Where(tuple => tuple.documentStore == null || tuple.mapper == null || tuple.mediator == null)
                    .Select(tuple => (new object[] { tuple.documentStore, tuple.mapper, tuple.mediator }));

                return cases;
            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null(IDocumentStore documentStore, IMapper mapper, IMediator mediator)
        {
            _outputHelper.WriteLine($"{nameof(documentStore)} is null : {documentStore == null}");
            _outputHelper.WriteLine($"{nameof(mapper)} is null : {mapper == null}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {mediator == null}");
            // Act
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new HandleCreateAccountInfoCommand(documentStore, mediator, mapper);
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
            (
                id: Guid.NewGuid(),
                username: "thebatman",
                email: "thecaped@crusader.com",
                passwordHash: "fjeiozfjzfdcvqcnjifozjffkjioj",
                salt: "some_salt_and_pepper"
            );

            using (IDocumentSession session = _documentStore.OpenSession())
            {
                session.Store(existingAccount, existingAccount.Id.ToString());
                session.SaveChanges();
            }
           
            NewAccountInfo newResourceInfo = new NewAccountInfo
            {
                Username = existingAccount.Username,
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader",
                Email = "b.wayne@gotham.com"
            };

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

            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<AccountCreated>(), It.IsAny<CancellationToken>()), Times.Never,
                "No resource created");
        }

        [Fact]
        public async Task GivenPasswordAndConfirmPasswordDoesNotMatch_Handler_Returns_Conflict()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send<(string, string)>(It.IsAny<HashPasswordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HashPasswordQuery request, CancellationToken cancellation) => (request.Data, "salt"));

            
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

            
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<AccountCreated>(), It.IsAny<CancellationToken>()), Times.Never,
                "No resource created");
        }

        [Fact]
        public async Task GivenCorrectData_Handler_Create_Account()
        {
            // Arrange
            NewAccountInfo newAccount = new NewAccountInfo
            {
                Name = "Bruce Wayne",
                Username = "thebatman",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader",
                Email = "b.wayne@gotham.com"
            };

            //_mapperMock.Setup(mock => mock.Map<NewAccountInfo, Account>(It.IsAny<NewAccountInfo>()))
            //    .Returns((NewAccountInfo newResource) => AutoMapperConfig.Build().CreateMapper().Map<NewAccountInfo, Account>(newResource));

            //_mapperMock.Setup(mock => mock.Map<Account, AccountInfo>(It.IsAny<Account>()))
            //    .Returns((Account newEntity) => AutoMapperConfig.Build().CreateMapper().Map<Account, AccountInfo>(newEntity));

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<AccountCreated>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<HashPasswordQuery>(), It.IsAny<CancellationToken>()))
                .Returns((HashPasswordQuery query, CancellationToken _) => Task.FromResult((salt: query.Data, passwordHash: new string(query.Data.Reverse().ToArray()))));

            CreateAccountInfoCommand cmd = new CreateAccountInfoCommand(newAccount);

            // Act
            Option<AccountInfo, CreateCommandResult> optionalResult = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            optionalResult.HasValue.Should()
                    .BeTrue("Data provided to create an account are correct");

            optionalResult.MatchSome(resource =>
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

                _mediatorMock.Verify(mock => mock.Send(It.IsAny<HashPasswordQuery>(), It.IsAny<CancellationToken>()), Times.Once);
                _mediatorMock.Verify(mock => mock.Send(It.Is<HashPasswordQuery>(query => query.Data == newAccount.Password), It.IsAny<CancellationToken>()), Times.Once);
                _mediatorMock.Verify(mock => mock.Publish(It.IsAny<AccountCreated>(), It.IsAny<CancellationToken>()), Times.Once);
                _mediatorMock.Verify(mock => mock.Publish(
                    It.Is<AccountCreated>(evt => evt.Data.Id == resource.Id),
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            });

            //_mapperMock.Verify(mock => mock.Map<NewAccountInfo, Account>(It.IsAny<NewAccountInfo>()), Times.Once);
        }
    }
}
