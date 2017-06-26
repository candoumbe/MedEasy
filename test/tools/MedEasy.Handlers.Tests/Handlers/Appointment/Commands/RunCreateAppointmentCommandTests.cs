using AutoMapper;
using FluentAssertions;
using MedEasy.API.Stores;
using MedEasy.Commands.Appointment;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Handlers.Appointment.Commands;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using Xunit;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Mapping;
using Optional;

namespace MedEasy.Handlers.Tests.Handlers.Appointment.Commands
{
    public class RunCreateAppointmentCommandTests : IDisposable
    {
        private RunCreateAppointmentCommand _handler;
        private Mock<IMapper> _mapperMock;
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        public RunCreateAppointmentCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MedEasyContext> builder = new DbContextOptionsBuilder<MedEasyContext>()
                .UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(builder.Options);
            _mapperMock = new Mock<IMapper>(Strict);
            _mapperMock.Setup(mock => mock.Map<AppointmentInfo>(It.IsNotNull<Objects.Appointment>()))
                .Returns((Objects.Appointment source) => AutoMapperConfig.Build().CreateMapper().Map<AppointmentInfo>(source));


            _handler = new RunCreateAppointmentCommand(_unitOfWorkFactory, _mapperMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactory = null;
            _mapperMock = null;
            _handler = null;

        }

        [Fact]
        public async Task Create()
        {
            // Arrange
            Guid doctorUUID = Guid.NewGuid();
            Guid patientUUID = Guid.NewGuid();

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Objects.Patient>().Create(new Objects.Patient { Id = 3, Firstname = "Bruce", Lastname = "Wayne", UUID = patientUUID });
                uow.Repository<Objects.Doctor>().Create(new Objects.Doctor { Id = 1, Firstname = "Hugo", Lastname = "Strange", UUID = doctorUUID });

                await uow.SaveChangesAsync();
            }

            CreateAppointmentInfo data = new CreateAppointmentInfo
            {
                DoctorId = doctorUUID,
                PatientId = patientUUID,
                StartDate = 1.February(2010),
                Duration = 15
            };

            // Act 
            CreateAppointmentCommand cmd = new CreateAppointmentCommand(data);
            Option<AppointmentInfo, CommandException> result = await _handler.RunAsync(cmd);

            // Assert
            result.HasValue.Should().BeTrue();
            result.MatchSome(x =>
            {
                x.Id.Should().NotBeEmpty();
                x.PatientId.Should().Be(data.PatientId);
                x.DoctorId.Should().Be(data.DoctorId);
                x.StartDate.Should().Be(data.StartDate);
                x.Duration.Should().Be(data.Duration);
            });

        }





        /// <summary>
        /// Tests <see cref="RunCreateAppointmentCommand.RunAsync(ICreateAppointmentCommand)"/> throws <see cref="NotFoundException"/>
        /// if <see cref="CreateAppointmentCommand"/> refers to a <see cref="DoctorInfo"/> that doesn't exist.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Run_Throws_NotFoundException_If_Doctor_Not_Found()
        {
            // Arrange
            Guid patientUUID = Guid.NewGuid();

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Objects.Patient>().Create(new Objects.Patient { Id = 3, Firstname = "Bruce", Lastname = "Wayne", UUID = patientUUID });

                await uow.SaveChangesAsync();
            }


            CreateAppointmentInfo data = new CreateAppointmentInfo
            {
                DoctorId = Guid.NewGuid(),
                PatientId = patientUUID,
                StartDate = 1.February(2010),
                Duration = 15
            };

            // Act 
            CreateAppointmentCommand cmd = new CreateAppointmentCommand(data);
            Option<AppointmentInfo, CommandException> result = await _handler.RunAsync(cmd);

            // Assert
            result.HasValue.Should().BeFalse();
            result.MatchNone(exception => exception.Should()
                .BeOfType<CommandEntityNotFoundException>().Which
                .Message.Should()
                .MatchEquivalentOf($"{nameof(Objects.Doctor)} <{data.DoctorId}> not found"));
            
        }

        /// <summary>
        /// Tests <see cref="RunCreateAppointmentCommand.RunAsync(ICreateAppointmentCommand)"/> throws <see cref="NotFoundException"/>
        /// if <see cref="CreateAppointmentCommand"/> refers to a <see cref="DoctorInfo"/> that doesn't exist.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Run_Throws_NotFoundException_If_Patient_Not_Found()
        {
            // Arrange
            Guid doctorUUID = Guid.NewGuid();

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Objects.Doctor>().Create(new Objects.Doctor { Id = 3, Firstname = "Bruce", Lastname = "Wayne", UUID = doctorUUID });

                await uow.SaveChangesAsync();
            }

            CreateAppointmentInfo data = new CreateAppointmentInfo
            {
                DoctorId = doctorUUID,
                PatientId = Guid.NewGuid(),
                StartDate = 1.February(2010),
                Duration = 15
            };

            // Act 
            CreateAppointmentCommand cmd = new CreateAppointmentCommand(data);
            Option<AppointmentInfo, CommandException> result = await _handler.RunAsync(cmd);

            // Assert
            result.HasValue.Should().BeFalse();
            result.MatchNone(exception =>
            {
                exception.Should()
                    .BeOfType<CommandEntityNotFoundException>().Which
                    .Message.Should()
                    .BeEquivalentTo($"{nameof(Objects.Patient)} <{data.PatientId}> not found");

            });


        }
    }
}
