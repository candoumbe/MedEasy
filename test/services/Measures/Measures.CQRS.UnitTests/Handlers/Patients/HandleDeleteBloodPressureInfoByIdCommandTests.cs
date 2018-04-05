using AutoMapper.QueryableExtensions;
using FluentAssertions;
using Measures.Context;
using Measures.CQRS.Commands.Patients;
using Measures.CQRS.Events;
using Measures.CQRS.Handlers.Patients;
using Measures.Mapping;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Context;
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

namespace Measures.CQRS.UnitTests.Handlers.Patients
{
    [UnitTest]
    public class HandleDeletePatientInfoByIdCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private IExpressionBuilder _expressionBuilder;
        private Mock<IMediator> _mediatorMock;
        private HandleDeletePatientInfoByIdCommand _sut;

        public HandleDeletePatientInfoByIdCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
            builder.UseSqlite(database.Connection);

            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, (options) => {
                MeasuresContext context = new MeasuresContext(options);
                context.Database.EnsureCreated();
                return context;
            });
            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;
            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new HandleDeletePatientInfoByIdCommand(_uowFactory, _expressionBuilder, _mediatorMock.Object);
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
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {(unitOfWorkFactory == null)}");
            _outputHelper.WriteLine($"{nameof(expressionBuilder)} is null : {(expressionBuilder == null)}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {(mediator == null)}");
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
            Guid idToDelete = Guid.NewGuid();
            Patient measure = new Patient
            {
                UUID = idToDelete,
                Firstname = "victor",
                Lastname = "zsasz",
            };
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(measure);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            DeletePatientInfoByIdCommand cmd = new DeletePatientInfoByIdCommand(idToDelete);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            DeleteCommandResult result = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<PatientDeleted>(), default), Times.Once, $"{nameof(HandleDeletePatientInfoByIdCommand)} must notify suscribers that a resource was deleted");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool deleteSuccessfull = !await uow.Repository<Patient>()
                     .AnyAsync(x => x.UUID == idToDelete)
                     .ConfigureAwait(false);

                deleteSuccessfull.Should().BeTrue("element should not be present after handling the delete command");
            }
        }


        public static IEnumerable<object[]> PatientWithMeasuresCases
        {
            get
            {

                {
                    Guid idPatient = Guid.NewGuid();
                    yield return new object[]
                    {
                        new Patient
                        {
                            UUID = Guid.NewGuid(),
                            Firstname = "Solomon",

                        },

                        new PhysiologicalMeasurement[]
                        {
                            new BloodPressure
                            {
                                SystolicPressure = 120,
                                DiastolicPressure = 80
                            }
                        }
                    };
                }
                
            }
        }

        [Theory]
        [MemberData(nameof(PatientWithMeasuresCases))]
        public async Task DeletePatient_When_AnyMeasure_Exists_ShouldReturns_Conflict(Patient patient, IEnumerable<PhysiologicalMeasurement> measures)
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

                uow.Repository<PhysiologicalMeasurement>().Create(measures);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            DeleteCommandResult commandResult = await _sut.Handle(new DeletePatientInfoByIdCommand(patient.UUID), default)
                .ConfigureAwait(false);


            // Assert
            commandResult.Should()
                .Be(DeleteCommandResult.Failed_Conflict, "the element to delete still has measures rattached to it");
        }
    }
}
