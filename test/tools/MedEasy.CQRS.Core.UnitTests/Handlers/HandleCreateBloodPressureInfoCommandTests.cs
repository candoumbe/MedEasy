using Measures.CQRS.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using Moq;
using Microsoft.EntityFrameworkCore;
using Measures.Context;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Context;
using Xunit;
using MedEasy.DAL.Interfaces;
using FluentAssertions;
using Measures.DTO;
using FluentAssertions.Extensions;
using Measures.CQRS.Commands;
using Measures.Objects;
using Optional;
using MediatR;
using Measures.CQRS.Events;
using System.Reflection;
using Measures.Mapping;
using System.Threading;

namespace MedEasy.CQRS.Core.UnitTests.Handlers
{
    public class HandleCreateBloodPressureInfoCommandTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private Mock<IMediator> _mediatorMock;
        private HandleCreateBloodPressureInfoCommand _sut;

        public HandleCreateBloodPressureInfoCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
            builder.UseInMemoryDatabase($"InMemoryDb_{Guid.NewGuid()}");
            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, (options) => new MeasuresContext(options));
            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);
            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new HandleCreateBloodPressureInfoCommand(_uowFactory, _expressionBuilderMock.Object, _mediatorMock.Object);
        }

        public void Dispose()
        {
            _uowFactory = null;
            _expressionBuilderMock = null;
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
                    //.Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder == null)
                    .Select(tuple => new { tuple.uowFactory, tuple.expressionBuilder })
                    .CrossJoin(mediatorCases, (a, mediator) => (a.uowFactory, a.expressionBuilder, mediator))
                    //.Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder != null || tuple.mediator != null)
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
            Action action = () => new HandleCreateBloodPressureInfoCommand(unitOfWorkFactory, expressionBuilder, mediator);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task CreateBloodPressure()
        {
            // Arrange
            CreateBloodPressureInfo newResourceInfo = new CreateBloodPressureInfo
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 23.August(2003).Add(15.Hours().Add(30.Minutes())),
                Patient = new PatientInfo
                {
                    Firstname = "victor",
                    Lastname = "zsasz",
                }
            };

            CreateBloodPressureInfoCommand cmd = new CreateBloodPressureInfoCommand(newResourceInfo);

            _expressionBuilderMock.Setup(mock => mock.GetMapExpression(It.IsAny<Type>(), It.IsAny<Type>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
                .Returns((Type source, Type destination, IDictionary<string, object> parameters, MemberInfo[] members) => AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression(source, destination, parameters, members));

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<PatientCreated>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<BloodPressureCreated>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);


            // Act
            BloodPressureInfo createdResource = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            using (IUnitOfWork uow = _uowFactory.New())
            {
                Option<BloodPressure> optionalCreatedMeasure = await uow.Repository<BloodPressure>()
                    .SingleOrDefaultAsync(x => x.UUID == createdResource.Id)
                    .ConfigureAwait(false);

                optionalCreatedMeasure.HasValue.Should()
                    .BeTrue();
                optionalCreatedMeasure.MatchSome((measure) =>
                {
                    measure.UUID.Should()
                        .Be(createdResource.Id);
                    measure.SystolicPressure.Should()
                        .Be(newResourceInfo.SystolicPressure);
                    measure.DiastolicPressure.Should()
                        .Be(newResourceInfo.DiastolicPressure);
                    measure.DateOfMeasure.Should()
                        .Be(newResourceInfo.DateOfMeasure);

                });

                Option<Patient> optionalCreatedPatient = await uow.Repository<Patient>()
                    .SingleOrDefaultAsync(x => x.UUID == createdResource.PatientId)
                    .ConfigureAwait(false);
                optionalCreatedPatient.HasValue.Should()
                    .BeTrue();
                optionalCreatedPatient.MatchSome((createdPatient) =>
                {
                    createdPatient.UUID.Should()
                        .Be(createdResource.PatientId);
                    createdPatient.Firstname.Should()
                        .Be(newResourceInfo.Patient.Firstname.ToTitleCase());
                    createdPatient.Lastname.Should()
                        .Be(newResourceInfo.Patient.Lastname.ToUpper());
                });


            }

            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<BloodPressureCreated>(), default), Times.Once, $"{nameof(HandleCreateBloodPressureInfoCommand)} must notify suscriber that a new measure resource was created");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<PatientCreated>(), default), Times.Once, $"{nameof(HandleCreateBloodPressureInfoCommand)} must notify suscriber that a new patient resource was created");

        }
    }
}
