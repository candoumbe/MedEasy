using AutoMapper.QueryableExtensions;
using FluentAssertions;
using FluentAssertions.Extensions;
using Measures.Context;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Events.BloodPressures;
using Measures.CQRS.Handlers.BloodPressures;
using Measures.Mapping;
using Measures.Objects;
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

namespace Measures.CQRS.UnitTests.Handlers.BloodPressures
{
    [UnitTest]
    public class HandleDeleteBloodPressureInfoByIdCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private IExpressionBuilder _expressionBuilder;
        private Mock<IMediator> _mediatorMock;
        private HandleDeleteBloodPressureInfoByIdCommand _sut;

        public HandleDeleteBloodPressureInfoByIdCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
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

            _sut = new HandleDeleteBloodPressureInfoByIdCommand(_uowFactory, _expressionBuilder, _mediatorMock.Object);
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
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>()};
                IExpressionBuilder[] expressionBuilderCases = { null, Mock.Of<IExpressionBuilder>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };

                IEnumerable<object[]> cases = uowFactorieCases
                    .CrossJoin(expressionBuilderCases, (uowFactory, expressionBuilder) => (uowFactory, expressionBuilder))
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder })
                    .CrossJoin(mediatorCases, (a, mediator) => (a.uowFactory, a.expressionBuilder, mediator))
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder, tuple.mediator})
                    .Where(tuple  => tuple.uowFactory == null || tuple.expressionBuilder == null || tuple.mediator == null)
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
            Action action = () => new HandleDeleteBloodPressureInfoByIdCommand(unitOfWorkFactory, expressionBuilder, mediator);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task DeleteBloodPressure()
        {
            // Arrange
            Guid idToDelete = Guid.NewGuid();
            Patient patient = new Patient(Guid.NewGuid(), "Bruce Wayne");

            BloodPressure measure = new BloodPressure(
                idToDelete,
                patientId: patient.Id,
                systolicPressure: 120,
                diastolicPressure: 80,
                dateOfMeasure: 23.August(2003).Add(15.Hours().Add(30.Minutes()))
            );

            patient.AddBloodPressure(measure.Id, measure.DateOfMeasure, measure.SystolicPressure, measure.DiastolicPressure);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(patient);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            DeleteBloodPressureInfoByIdCommand cmd = new DeleteBloodPressureInfoByIdCommand(idToDelete);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            DeleteCommandResult result = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            result.Should()
                .Be(DeleteCommandResult.Done);
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<BloodPressureDeleted>(), default), Times.Once, $"{nameof(HandleCreateBloodPressureInfoCommand)} must notify suscribers that blood pressure resource was deleted");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool deleteSuccessfull = ! await uow.Repository<BloodPressure>()
                     .AnyAsync(x => x.Id == idToDelete)
                     .ConfigureAwait(false);

                deleteSuccessfull.Should().BeTrue("element should not be present after handling the delete command");
            }
        }
    }
}
