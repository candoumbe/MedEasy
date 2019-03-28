using FluentAssertions;
using Identity.CQRS.Commands.Accounts;
using Identity.CQRS.Events.Accounts;
using Identity.CQRS.Handlers.Commands.Accounts;
using Identity.DataStores.SqlServer;
using Identity.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
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
    public class HandleDeleteAccountInfoByIdCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<IMediator> _mediatorMock;
        private HandleDeleteAccountInfoByIdCommand _sut;

        public HandleDeleteAccountInfoByIdCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
            builder.UseSqlite(database.Connection);

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) => {
                IdentityContext context = new IdentityContext(options);
                context.Database.EnsureCreated();
                return context;
            });

            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new HandleDeleteAccountInfoByIdCommand(_uowFactory, _mediatorMock.Object);
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
            _sut = null;
            _mediatorMock = null;
        }


        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };

                IEnumerable<object[]> cases = uowFactorieCases
                    .CrossJoin(mediatorCases, (uowFactory, mediator) => (uowFactory, mediator))
                    .Select(tuple => new { tuple.uowFactory, tuple.mediator })
                    .Where(tuple => tuple.uowFactory == null || tuple.mediator == null)
                    .Select(tuple => new object[] { tuple.uowFactory, tuple.mediator });

                return cases;
            }
        }


        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null(IUnitOfWorkFactory unitOfWorkFactory, IMediator mediator)
        {
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {unitOfWorkFactory == null}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {mediator == null}");
            // Act
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new HandleDeleteAccountInfoByIdCommand(unitOfWorkFactory, mediator);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GivenAccountExists_Delete_Succeed()
        {
            // Arrange
            Guid idToDelete = Guid.NewGuid();
            Account entity = new Account
            {
                UUID = idToDelete,
                Name = "victor zsasz",
                UserName = "victorzsasz",
                Salt = "knife",
                PasswordHash = "cut_up"
            };
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(entity);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }


            DeleteAccountInfoByIdCommand cmd = new DeleteAccountInfoByIdCommand(idToDelete);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            DeleteCommandResult result = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            result.Should()
                .Be(DeleteCommandResult.Done, "The resource was deleted successfully");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<AccountDeleted>(), It.IsAny<CancellationToken>()), Times.Once, $"{nameof(HandleDeleteAccountInfoByIdCommand)} must notify suscribers that account resource was deleted");
            _mediatorMock.Verify(mock => mock.Publish(It.Is<AccountDeleted>(deleted => deleted.AccountId == idToDelete), It.IsAny<CancellationToken>()), Times.Once, $"{nameof(HandleDeleteAccountInfoByIdCommand)} must notify suscribers that account resource was deleted");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool deleteSuccessfull = !await uow.Repository<Account>()
                     .AnyAsync(x => x.UUID == idToDelete)
                     .ConfigureAwait(false);

                deleteSuccessfull.Should().BeTrue("element should not be present after handling the delete command");
            }
        }

        [Fact]
        public void Type_Is_A_Handler() =>
            // Assert
            typeof(HandleDeleteAccountInfoByIdCommand).Should()
                .Implement<IRequestHandler<DeleteAccountInfoByIdCommand, DeleteCommandResult>>();

        [Fact]
        public async Task GivenAccountIsTenant_Delete_Returns_Conflict()
        {
            // Arrange
            Guid idToDelete = Guid.NewGuid();
            Account tenant = new Account
            {
                UUID = Guid.NewGuid(),
                Name = "victor zsasz",
                UserName = "victorzsasz",
                Salt = "knife",
                PasswordHash = "cut_up"
            };

            Account account = new Account
            {
                UUID = idToDelete,
                Name = "victor zsasz's minion",
                UserName = "victorzsasz-minion",
                Salt = "knife",
                PasswordHash = "cut_up",
                TenantId = tenant.UUID
            };
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(new[] { tenant, account });
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }


            DeleteAccountInfoByIdCommand cmd = new DeleteAccountInfoByIdCommand(tenant.UUID);

            
            // Act
            DeleteCommandResult result = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            result.Should()
                .Be(DeleteCommandResult.Failed_Conflict, "The resource to delete is a tenant");
            
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool deleteSuccessfull = !await uow.Repository<Account>()
                     .AnyAsync(x => x.UUID == idToDelete)
                     .ConfigureAwait(false);

                deleteSuccessfull.Should().BeFalse("acouunt to delete is a tenant");
            }
        }
    }
}
