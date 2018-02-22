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
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
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
    public class HandlePatchBloodPressureInfoCommandTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;
        private Mock<IMediator> _mediatorMock;
        private HandlePatchBloodPressureInfoCommand _sut;

        public HandlePatchBloodPressureInfoCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
            builder.UseInMemoryDatabase($"InMemoryDb_{Guid.NewGuid()}");
            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, (options) => new MeasuresContext(options));
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
            BloodPressure measure = new BloodPressure
            {
                UUID = idToPatch,
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 23.August(2003).Add(15.Hours().Add(30.Minutes())),
                Patient = new Patient
                {
                    Firstname = "victor",
                    Lastname = "zsasz",
                }

            };
            using (IUnitOfWork uow = _uowFactory.New())
            {
                uow.Repository<BloodPressure>().Create(measure);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            JsonPatchDocument<BloodPressureInfo> patchDocument = new JsonPatchDocument<BloodPressureInfo>();
            patchDocument.Replace(x => x.SystolicPressure, 130);
            
            PatchInfo<Guid, BloodPressureInfo> patchInfo = new PatchInfo<Guid, BloodPressureInfo>
            {
                Id = idToPatch,
                PatchDocument = patchDocument
            };
            PatchCommand<Guid, BloodPressureInfo> cmd = new PatchCommand<Guid, BloodPressureInfo>(patchInfo);

            _mediatorMock.Setup(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<BloodPressureUpdated>(), default), Times.Once, $"{nameof(HandlePatchBloodPressureInfoCommand)} must notify suscribers that a resource was patched.");
            _mediatorMock.Verify(mock => mock.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);

            using (IUnitOfWork uow = _uowFactory.New())
            {
                BloodPressure actualMeasure = await uow.Repository<BloodPressure>()
                     .SingleAsync(x => x.UUID == idToPatch)
                     .ConfigureAwait(false);

                actualMeasure.SystolicPressure.Should().Be(130);
                actualMeasure.DiastolicPressure.Should().Be(measure.DiastolicPressure);
            }
        }
    }
}
