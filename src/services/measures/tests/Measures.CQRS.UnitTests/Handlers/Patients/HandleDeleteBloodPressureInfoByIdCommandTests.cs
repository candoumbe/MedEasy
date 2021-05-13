using AutoMapper.QueryableExtensions;

using FluentAssertions;
using FluentAssertions.Extensions;

using Measures.DataStores;
using Measures.CQRS.Commands.Patients;
using Measures.CQRS.Events;
using Measures.CQRS.Handlers.Patients;
using Measures.Ids;
using Measures.Mapping;
using Measures.Objects;

using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Moq;

using NodaTime;
using NodaTime.Extensions;
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

namespace Measures.CQRS.UnitTests.Handlers.Patients
{
    [UnitTest]
    public class HandleDeletePatientInfoByIdCommandTests : IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly HandleDeletePatientInfoByIdCommand _sut;

        public HandleDeletePatientInfoByIdCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<MeasuresStore> database)
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

            _sut = new HandleDeletePatientInfoByIdCommand(_uowFactory, _expressionBuilder, _mediatorMock.Object);
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
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {unitOfWorkFactory == null}");
            _outputHelper.WriteLine($"{nameof(expressionBuilder)} is null : {expressionBuilder == null}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {mediator == null}");
            // Act
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new HandleDeletePatientInfoByIdCommand(unitOfWorkFactory, expressionBuilder, mediator);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task DeletePatient()
        {
            // Arrange
            PatientId idToDelete = PatientId.New();
            Patient patient = new(idToDelete, "victor zsasz");
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(patient);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            DeletePatientInfoByIdCommand cmd = new(idToDelete);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            DeleteCommandResult result = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            result.Should()
                .Be(DeleteCommandResult.Done);
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<PatientDeleted>(), default), Times.Once, $"{nameof(HandleDeletePatientInfoByIdCommand)} must notify suscribers that a resource was deleted");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool deleteSuccessfull = !await uow.Repository<Patient>()
                     .AnyAsync(x => x.Id == idToDelete)
                     .ConfigureAwait(false);

                deleteSuccessfull.Should().BeTrue("element should not be present after handling the delete command");
            }
        }

        public static IEnumerable<object[]> PatientWithMeasuresCases
        {
            get
            {
                {
                    PatientId idPatient = PatientId.New();
                    yield return new object[]
                    {
                        new Patient(idPatient, "Solomon"),

                        new []
                        {
                            new BloodPressure(idPatient,
                                BloodPressureId.New(),
                                dateOfMeasure: 30.September(2007).AsUtc().ToInstant(),
                                systolicPressure : 120,
                                diastolicPressure : 80
                            )
                        }
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(PatientWithMeasuresCases))]
        public async Task DeletePatient_When_AnyMeasure_Exists_ShouldReturns_Conflict(Patient patient, IEnumerable<BloodPressure> measures)
        {
            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(patient);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                await measures.ForEachAsync(measure =>
                {
                    measure.PatientId = patient.Id;
                    return Task.CompletedTask;
                })
                .ConfigureAwait(false);

                uow.Repository<BloodPressure>().Create(measures);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            DeleteCommandResult commandResult = await _sut.Handle(new DeletePatientInfoByIdCommand(patient.Id), default)
                .ConfigureAwait(false);

            // Assert
            commandResult.Should()
                .Be(DeleteCommandResult.Failed_Conflict, "the element to delete still has measures rattached to it");
        }
    }
}
