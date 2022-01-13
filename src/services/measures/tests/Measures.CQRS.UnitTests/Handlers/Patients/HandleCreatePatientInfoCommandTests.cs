namespace Measures.CQRS.UnitTests.Handlers.Patients
{
    using AutoMapper.QueryableExtensions;

    using FluentAssertions;

    using Measures.DataStores;
    using Measures.CQRS.Commands.Patients;
    using Measures.CQRS.Events;
    using Measures.CQRS.Handlers.Subjects;
    using Measures.DTO;
    using Measures.Ids;
    using Measures.Mapping;
    using Measures.Objects;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

    using Moq;

    using NodaTime;
    using NodaTime.Testing;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Moq.MockBehavior;

    [UnitTest]
    public class HandleCreatePatientInfoCommandTests : IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly HandleCreateSubjectInfoCommand _sut;

        public HandleCreatePatientInfoCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<MeasuresStore> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<MeasuresStore>(database.OptionsBuilder.Options, (options) =>
            {
                MeasuresStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;
            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new HandleCreateSubjectInfoCommand(_uowFactory, _expressionBuilder, _mediatorMock.Object);
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IExpressionBuilder[] expressionBuilderCases = { null, Mock.Of<IExpressionBuilder>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };

                IEnumerable<object[]> cases = uowFactorieCases
                    .CrossJoin(expressionBuilderCases, (uowFactory, expressionBuilder) => (uowFactory, expressionBuilder))
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder })
                    .CrossJoin(mediatorCases, (a, mediator) => (a.uowFactory, a.expressionBuilder, mediator))
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder, tuple.mediator })
                    .Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder == null || tuple.mediator == null)
                    .Select(tuple => new object[] { tuple.uowFactory, tuple.expressionBuilder, tuple.mediator });

                return cases;
            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder, IMediator mediator)
        {
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {unitOfWorkFactory == null}");
            _outputHelper.WriteLine($"{nameof(expressionBuilder)} is null : {expressionBuilder == null}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {mediator == null}");
            // Act
            Action action = () => new HandleCreateSubjectInfoCommand(unitOfWorkFactory, expressionBuilder, mediator);

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
            NewSubjectInfo resourceInfo = new()
            {
                Name = "victor zsasz",
            };

            CreateSubjectInfoCommand cmd = new(resourceInfo);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            SubjectInfo createdResource = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<PatientCreated>(), default), Times.Once, $"{nameof(HandleCreateSubjectInfoCommand)} must notify suscribers that a resource was created");
            _mediatorMock.Verify(mock => mock.Publish(It.Is<PatientCreated>(evt => evt.Data.Id == createdResource.Id), default), Times.Once, $"{nameof(HandleCreateSubjectInfoCommand)} must notify suscribers that a resource was created");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);

            createdResource.Should()
                .NotBeNull();
            createdResource.Id.Should()
                .NotBe(SubjectId.Empty).And
                .NotBeNull();
            createdResource.Name.Should()
                .Be(resourceInfo.Name?.ToTitleCase());
            createdResource.BirthDate.Should()
                .Be(resourceInfo.BirthDate);

            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            bool createSuccessful = await uow.Repository<Subject>()
                .AnyAsync(x => x.Id == createdResource.Id)
                .ConfigureAwait(false);

            createSuccessful.Should().BeTrue("element should be present after handling the create command");
        }

        [Fact]
        public async Task CreatePatientWithIdProvided()
        {
            // Arrange
            SubjectId desiredId = SubjectId.New();
            NewSubjectInfo resourceInfo = new()
            {
                Name = "victor zsasz",
                Id = desiredId
            };

            CreateSubjectInfoCommand cmd = new(resourceInfo);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            SubjectInfo createdResource = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<PatientCreated>(), default), Times.Once, $"{nameof(HandleCreateSubjectInfoCommand)} must notify suscribers that a resource was created");
            _mediatorMock.Verify(mock => mock.Publish(It.Is<PatientCreated>(evt => evt.Data.Id == createdResource.Id), default), Times.Once, $"{nameof(HandleCreateSubjectInfoCommand)} must notify suscribers that a resource was created");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);

            createdResource.Should()
                .NotBeNull();
            createdResource.Id.Should()
                .Be(desiredId, $"handler must use value of {nameof(NewSubjectInfo)}.{nameof(NewSubjectInfo.Id)} when that value is not null");
            createdResource.Name.Should()
                .Be(resourceInfo.Name?.ToTitleCase());
            createdResource.BirthDate.Should()
                .Be(resourceInfo.BirthDate);

            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            bool createSuccessful = await uow.Repository<Subject>()
                .AnyAsync(x => x.Id == createdResource.Id)
                .ConfigureAwait(false);

            createSuccessful.Should().BeTrue("element should be present after handling the create command");
        }

        [Fact]
        public void Given_command_with_empty_id_Handles_throws_InvalidOperationException()
        {
            // Arrange
            NewSubjectInfo data = new()
            {
                Name = "Grundy",
                Id = SubjectId.Empty
            };

            CreateSubjectInfoCommand cmd = new(data);

            // Act
            Func<Task> action = async () => await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            action.Should()
                .NotThrowAsync<InvalidOperationException>($"{nameof(CreateSubjectInfoCommand.Data)} will provide a new id");
        }
    }
}
