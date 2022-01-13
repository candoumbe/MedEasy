namespace Patients.CQRS.UnitTests.Handlers
{
    using AutoMapper.QueryableExtensions;

    using FluentAssertions;

    using MassTransit;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

    using Moq;

    using NodaTime;
    using NodaTime.Testing;

    using Optional;

    using Patients.Context;
    using Patients.CQRS.Commands;
    using Patients.CQRS.Handlers.Patients;
    using Patients.DTO;
    using Patients.Events;
    using Patients.Ids;
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

    [UnitTest]
    [Feature("Patients")]
    [Feature("Handlers")]
    public class HandleCreatePatientInfoCommandTests : IClassFixture<SqliteEfCoreDatabaseFixture<PatientsDataStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly HandleCreatePatientInfoCommand _sut;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;

        public HandleCreatePatientInfoCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<PatientsDataStore> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<PatientsDataStore>(database.OptionsBuilder.Options, (options) =>
            {
                PatientsDataStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;
            _mediatorMock = new Mock<IMediator>(Strict);
            _publishEndpointMock = new(Strict);

            _sut = new HandleCreatePatientInfoCommand(_uowFactory, _expressionBuilder, _mediatorMock.Object, _publishEndpointMock.Object);
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IExpressionBuilder[] expressionBuilderCases = { null, Mock.Of<IExpressionBuilder>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };
                IPublishEndpoint[] publishEndpointsCases = { null, Mock.Of<IPublishEndpoint>() };

                return uowFactorieCases
                    .CrossJoin(expressionBuilderCases, (uowFactory, expressionBuilder) => (uowFactory, expressionBuilder))
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder })
                    .CrossJoin(mediatorCases, (a, mediator) => (a.uowFactory, a.expressionBuilder, mediator))
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder, tuple.mediator })
                    .CrossJoin(publishEndpointsCases, (a, publishEndpoint) => (a.uowFactory, a.expressionBuilder, a.mediator, publishEndpoint))
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder, tuple.mediator, tuple.publishEndpoint })
                    .Where(tuple => tuple.uowFactory is null || tuple.expressionBuilder is null || tuple.mediator is null || tuple.publishEndpoint is null)
                    .Select(tuple => new object[] { tuple.uowFactory, tuple.expressionBuilder, tuple.mediator, tuple.publishEndpoint });
            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder, IMediator mediator, IPublishEndpoint publishEndpoint)
        {
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {unitOfWorkFactory is null}");
            _outputHelper.WriteLine($"{nameof(expressionBuilder)} is null : {expressionBuilder is null}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {mediator is null}");
            _outputHelper.WriteLine($"{nameof(publishEndpoint)} is null : {publishEndpoint is null}");
            // Act
            Action action = () => new HandleCreatePatientInfoCommand(unitOfWorkFactory, expressionBuilder, mediator, publishEndpoint);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Given_id_was_not_provided_Handle_should_generate_one()
        {
            // Arrange
            CreatePatientInfo resourceInfo = new()
            {
                Firstname = "victor",
                Lastname = "zsasz",
            };

            CreatePatientInfoCommand cmd = new(resourceInfo);

            _publishEndpointMock.Setup(mock => mock.Publish(It.IsNotNull<PatientCaseCreated>(), It.IsAny<CancellationToken>()))
                                .Returns(Task.CompletedTask);

            Option<PatientInfo, CreateCommandFailure> optionalCreatedResource = await _sut.Handle(cmd, default)
                                                                                          .ConfigureAwait(false);

            // Assert
            optionalCreatedResource.HasValue.Should().BeTrue();
            optionalCreatedResource.MatchSome(
                async createdResource =>
                {
                    _publishEndpointMock.Verify(mock => mock.Publish(It.IsAny<PatientCaseCreated>(), default),
                                        Times.Once,
                                        $"{nameof(HandleCreatePatientInfoCommand)} must notify suscribers that a resource was created");
                    _publishEndpointMock.Verify(mock => mock.Publish(It.Is<PatientCaseCreated>(evt => evt.Id == createdResource.Id), default),
                                                Times.Once,
                                                $"{nameof(HandleCreatePatientInfoCommand)} must notify suscribers that a resource was created");
                    _publishEndpointMock.VerifyNoOtherCalls();

                    createdResource.Should()
                        .NotBeNull();
                    createdResource.Id.Should()
                        .NotBe(PatientId.Empty).And
                        .NotBeNull();
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

                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    bool createSuccessful = await uow.Repository<Patient>()
                        .AnyAsync(x => x.Id == createdResource.Id)
                        .ConfigureAwait(false);

                    createSuccessful.Should().BeTrue("element should be present after handling the create command");
                });
        }

        [Fact]
        public async Task Given_id_was_provided_Handle_should_use_the_id_provided()
        {
            // Arrange
            PatientId desiredId = PatientId.New();
            CreatePatientInfo resourceInfo = new()
            {
                Firstname = "victor",
                Lastname = "zsasz",
                Id = desiredId
            };

            CreatePatientInfoCommand cmd = new(resourceInfo);

            _publishEndpointMock.Setup(mock => mock.Publish(It.IsAny<PatientCaseCreated>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Option<PatientInfo, CreateCommandFailure> optionalCreatedResource = await _sut.Handle(cmd, default)
                                                                                          .ConfigureAwait(false);

            // Assert
            optionalCreatedResource.HasValue.Should().BeTrue();
            optionalCreatedResource.MatchSome(
                async createdResource =>
                {
                    _publishEndpointMock.Verify(mock => mock.Publish(It.IsAny<PatientCaseCreated>(), default), Times.Once, $"{nameof(HandleCreatePatientInfoCommand)} must notify suscribers that a resource was created");
                    _publishEndpointMock.Verify(mock => mock.Publish(It.Is<PatientCaseCreated>(evt => evt.Id == createdResource.Id), default), Times.Once, $"{nameof(HandleCreatePatientInfoCommand)} must notify suscribers that a resource was created");
                    _publishEndpointMock.VerifyNoOtherCalls();

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

                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    bool createSuccessful = await uow.Repository<Patient>()
                        .AnyAsync(x => x.Id == createdResource.Id)
                        .ConfigureAwait(false);

                    createSuccessful.Should().BeTrue("element should be present after handling the create command");
                });
        }
    }
}
