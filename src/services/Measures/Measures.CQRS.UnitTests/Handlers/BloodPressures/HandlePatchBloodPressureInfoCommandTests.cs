using AutoMapper;
using FluentAssertions;
using FluentAssertions.Extensions;
using Measures.Context;
using Measures.CQRS.Events.BloodPressures;
using Measures.CQRS.Handlers.BloodPressures;
using Measures.DTO;
using Measures.Mapping;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.IntegrationTests.Core;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
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
    public class HandlePatchBloodPressureInfoCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;
        private Mock<IMediator> _mediatorMock;
        private HandlePatchBloodPressureInfoCommand _sut;

        public HandlePatchBloodPressureInfoCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
            builder.UseSqlite(database.Connection);

            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, (options) => {
                MeasuresContext context = new MeasuresContext(options);
                context.Database.EnsureCreated();
                return context;
            });
            _mapper = AutoMapperConfig.Build().CreateMapper();
            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new HandlePatchBloodPressureInfoCommand(_uowFactory, _mapper, _mediatorMock.Object);
        }

        public void Dispose()
        {
            _uowFactory = null;
            _mapper = null;
            _sut = null;
            _mediatorMock = null;
        }


        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>()};
                IMapper[] mapperCases = { null, Mock.Of<IMapper>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };

                IEnumerable<object[]> cases = uowFactorieCases
                    .CrossJoin(mapperCases, (uowFactory, mapper) => ((uowFactory, mapper)))
                    .Select(((IUnitOfWorkFactory uowFactory, IMapper mapper) tuple) => new { tuple.uowFactory, tuple.mapper})
                    .CrossJoin(mediatorCases, (a, mediator) => ((a.uowFactory, a.mapper, mediator)))
                    .Select(((IUnitOfWorkFactory uowFactory, IMapper mapper, IMediator mediator) tuple) => new { tuple.uowFactory, tuple.mapper, tuple.mediator})
                    .Where(tuple  => tuple.uowFactory == null || tuple.mapper == null || tuple.mediator == null)
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
            Action action = () => new HandlePatchBloodPressureInfoCommand(unitOfWorkFactory, mapper, mediator);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task PatchBloodPressure()
        {
            // Arrange
            Guid idToPatch = Guid.NewGuid();

            Patient patient = new Patient(Guid.NewGuid())
                .ChangeNameTo("victor zsasz");
            const float systolic = 120;
            const float diastolic = 80;
            patient.AddBloodPressure(
                measureId : idToPatch,
                dateOfMeasure: 23.August(2003).Add(15.Hours().Add(30.Minutes())),
                systolic, diastolic
            );

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(patient);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            float systolicNewValue = 130;
            JsonPatchDocument<BloodPressureInfo> patchDocument = new JsonPatchDocument<BloodPressureInfo>();
            patchDocument.Replace(x => x.SystolicPressure, systolicNewValue);

            PatchInfo<Guid, BloodPressureInfo> patchInfo = new PatchInfo<Guid, BloodPressureInfo>
            {
                Id = idToPatch,
                PatchDocument = patchDocument
            };
            PatchCommand<Guid, BloodPressureInfo> cmd = new PatchCommand<Guid, BloodPressureInfo>(patchInfo);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            ModifyCommandResult result = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            result.Should()
                .Be(ModifyCommandResult.Done);

            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<BloodPressureUpdated>(), default), Times.Once, $"{nameof(HandlePatchBloodPressureInfoCommand)} must notify suscribers that a resource was patched.");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                BloodPressure actualMeasure = await uow.Repository<BloodPressure>()
                     .SingleAsync(x => x.Id == idToPatch)
                     .ConfigureAwait(false);

                actualMeasure.SystolicPressure.Should().Be(systolicNewValue);
                actualMeasure.DiastolicPressure.Should().Be(diastolic);
            }
        }
    }
}
