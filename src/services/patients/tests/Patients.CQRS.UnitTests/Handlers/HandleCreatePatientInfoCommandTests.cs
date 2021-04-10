using AutoMapper.QueryableExtensions;

using FluentAssertions;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Moq;

using NodaTime;
using NodaTime.Testing;

using Patients.Context;
using Patients.CQRS.Commands;
using Patients.CQRS.Events;
using Patients.CQRS.Handlers.Patients;
using Patients.DTO;
using Patients.Mapping;
using Patients.Objects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

using static Moq.MockBehavior;

namespace Patients.CQRS.UnitTests.Handlers
{
    [UnitTest]
    [Feature("Patients")]
    [Feature("Handlers")]
    public class HandleCreatePatientInfoCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private IExpressionBuilder _expressionBuilder;
        private Mock<IMediator> _mediatorMock;
        private HandleCreatePatientInfoCommand _sut;

        public HandleCreatePatientInfoCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<PatientsContext> builder = new DbContextOptionsBuilder<PatientsContext>();
            builder.UseInMemoryDatabase($"{Guid.NewGuid()}");

            _uowFactory = new EFUnitOfWorkFactory<PatientsContext>(builder.Options, (options) => {
                PatientsContext context = new PatientsContext(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;
            _mediatorMock = new Mock<IMediator>(Strict);
            
            _sut = new HandleCreatePatientInfoCommand(_uowFactory, _expressionBuilder, _mediatorMock.Object);
        }

        public void Dispose()
        {
            _uowFactory = null;
            _expressionBuilder = null;
            _sut = null;
            _mediatorMock = null;
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IExpressionBuilder[] expressionBuilderCases = { null, Mock.Of<IExpressionBuilder>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };

                return uowFactorieCases
                    .CrossJoin(expressionBuilderCases, (uowFactory, expressionBuilder) => (uowFactory, expressionBuilder))
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder })
                    .CrossJoin(mediatorCases, (a, mediator) => (a.uowFactory, a.expressionBuilder, mediator))
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder, tuple.mediator })
                    .Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder == null || tuple.mediator == null)
                    .Select(tuple => new object[] { tuple.uowFactory, tuple.expressionBuilder, tuple.mediator });
            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder, IMediator mediator)
        {
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {(unitOfWorkFactory == null)}");
            _outputHelper.WriteLine($"{nameof(expressionBuilder)} is null : {(expressionBuilder == null)}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {(mediator == null)}");
            // Act
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new HandleCreatePatientInfoCommand(unitOfWorkFactory, expressionBuilder, mediator);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task CreatePatientWithNoIdProvided()
        {
            // Arrange
            CreatePatientInfo resourceInfo = new CreatePatientInfo
            {
                Firstname = "victor",
                Lastname = "zsasz",
            };

            CreatePatientInfoCommand cmd = new CreatePatientInfoCommand(resourceInfo);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            //DateTimeOffset now = 28.February(2015);
            //_dateTimeServiceMock.Setup(mock => mock.UtcNowOffset())
            //    .Returns(now);

            // Act
            PatientInfo createdResource = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            //_dateTimeServiceMock.Verify(mock => mock.UtcNowOffset(), Times.Once);
            //_dateTimeServiceMock.VerifyNoOtherCalls();

            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<PatientCreated>(), default), Times.Once, $"{nameof(HandleCreatePatientInfoCommand)} must notify suscribers that a resource was created");
            _mediatorMock.Verify(mock => mock.Publish(It.Is<PatientCreated>(evt => evt.Data.Id == createdResource.Id), default), Times.Once, $"{nameof(HandleCreatePatientInfoCommand)} must notify suscribers that a resource was created");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            createdResource.Should()
                .NotBeNull();
            createdResource.Id.Should()
                .NotBeEmpty();
            createdResource.Firstname.Should()
                .Be(resourceInfo.Firstname?.ToTitleCase());
            createdResource.Lastname.Should()
                .Be(resourceInfo.Lastname?.ToUpperInvariant());
            createdResource.BirthDate.Should()
                .Be(resourceInfo.BirthDate);
            //createdResource.CreatedDate.Should()
            //    .Be(now);
            //createdResource.UpdatedDate.Should()
            //    .Be(createdResource.CreatedDate);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool createSuccessful = await uow.Repository<Patient>()
                    .AnyAsync(x => x.Id == createdResource.Id)
                    .ConfigureAwait(false);

                createSuccessful.Should().BeTrue("element should be present after handling the create command");
            }
        }

        [Fact]
        public async Task CreatePatientWithIdProvided()
        {
            // Arrange
            Guid desiredId = Guid.NewGuid();
            CreatePatientInfo resourceInfo = new CreatePatientInfo
            {
                Firstname = "victor",
                Lastname = "zsasz",
                Id = desiredId
            };

            //DateTimeOffset now = 28.February(2015);
            //_dateTimeServiceMock.Setup(mock => mock.UtcNowOffset())
            //    .Returns(now);


            CreatePatientInfoCommand cmd = new CreatePatientInfoCommand(resourceInfo);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            PatientInfo createdResource = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            //_dateTimeServiceMock.Verify(mock => mock.UtcNowOffset(), Times.Once);
            //_dateTimeServiceMock.VerifyNoOtherCalls();

            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<PatientCreated>(), default), Times.Once, $"{nameof(HandleCreatePatientInfoCommand)} must notify suscribers that a resource was created");
            _mediatorMock.Verify(mock => mock.Publish(It.Is<PatientCreated>(evt => evt.Data.Id == createdResource.Id), default), Times.Once, $"{nameof(HandleCreatePatientInfoCommand)} must notify suscribers that a resource was created");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            createdResource.Should()
                .NotBeNull();
            createdResource.Id.Should()
                .Be(desiredId, $"handler must use value of {nameof(CreatePatientInfo)}.{nameof(CreatePatientInfo.Id)} when that value is not null");
            createdResource.Firstname.Should()
                .Be(resourceInfo.Firstname?.ToTitleCase());
            createdResource.Lastname.Should()
                .Be(resourceInfo.Lastname?.ToUpperInvariant());
            createdResource.BirthDate.Should()
                .Be(resourceInfo.BirthDate);
            //createdResource.CreatedDate.Should()
            //    .Be(now);
            //createdResource.UpdatedDate.Should()
            //    .Be(createdResource.CreatedDate);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool createSuccessful = await uow.Repository<Patient>()
                    .AnyAsync(x => x.Id == createdResource.Id)
                    .ConfigureAwait(false);

                createSuccessful.Should().BeTrue("element should be present after handling the create command");
            }
        }
    }
}
