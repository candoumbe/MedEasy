using AutoMapper;

using FluentAssertions;
using FluentAssertions.Extensions;

using Measures.DataStores;
using Measures.CQRS.Events;
using Measures.CQRS.Handlers.Patients;
using Measures.DTO;
using Measures.Ids;
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

using Moq;

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
    public class HandlePatchPatientInfoCommandTests : IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IMapper _mapper;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly HandlePatchPatientInfoCommand _sut;

        public HandlePatchPatientInfoCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<MeasuresStore> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<MeasuresStore>(database.OptionsBuilder.Options, (options) =>
            {
                MeasuresStore context = new(options, new FakeClock(new DateTime().AsUtc().ToInstant()));
                context.Database.EnsureCreated();
                return context;
            });
            _mapper = AutoMapperConfig.Build().CreateMapper();
            _mediatorMock = new Mock<IMediator>(Strict);

            _sut = new HandlePatchPatientInfoCommand(_uowFactory, _mapper, _mediatorMock.Object);
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IMapper[] mapperCases = { null, Mock.Of<IMapper>() };
                IMediator[] mediatorCases = { null, Mock.Of<IMediator>() };

                IEnumerable<object[]> cases = uowFactorieCases
                    .CrossJoin(mapperCases, (uowFactory, mapper) => (uowFactory, mapper))
                    .Select(((IUnitOfWorkFactory uowFactory, IMapper mapper) tuple) => new { tuple.uowFactory, tuple.mapper })
                    .CrossJoin(mediatorCases, (a, mediator) => (a.uowFactory, a.mapper, mediator))
                    .Select(((IUnitOfWorkFactory uowFactory, IMapper mapper, IMediator mediator) tuple) => new { tuple.uowFactory, tuple.mapper, tuple.mediator })
                    .Where(tuple => tuple.uowFactory == null || tuple.mapper == null || tuple.mediator == null)
                    .Select(tuple => (new object[] { tuple.uowFactory, tuple.mapper, tuple.mediator }));

                return cases;
            }
        }


        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper, IMediator mediator)
        {
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {unitOfWorkFactory == null}");
            _outputHelper.WriteLine($"{nameof(mapper)} is null : {mapper == null}");
            _outputHelper.WriteLine($"{nameof(mediator)} is null : {mediator == null}");
            // Act
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new HandlePatchPatientInfoCommand(unitOfWorkFactory, mapper, mediator);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task PatchPatient()
        {
            // Arrange
            PatientId idToPatch = PatientId.New();
            Patient entity = new(idToPatch, "victor zsasz");

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(entity);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            JsonPatchDocument<PatientInfo> patchDocument = new();
            patchDocument.Replace(x => x.Name, "Darkseid");

            PatchInfo<PatientId, PatientInfo> patchInfo = new()
            {
                Id = idToPatch,
                PatchDocument = patchDocument
            };
            PatchCommand<PatientId, PatientInfo> cmd = new(patchInfo);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            ModifyCommandResult patchResult = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            patchResult.Should()
                .Be(ModifyCommandResult.Done);

            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<PatientUpdated>(), default), Times.Once, $"{nameof(HandlePatchPatientInfoCommand)} must notify suscribers that a resource was patched.");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Patient actualMeasure = await uow.Repository<Patient>()
                     .SingleAsync(x => x.Id == idToPatch)
                     .ConfigureAwait(false);

                actualMeasure.Name.Should()
                    .Be("Darkseid");
            }
        }
    }
}
