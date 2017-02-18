using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics;
using AutoMapper;
using MedEasy.Commands.Appointment;
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Handlers.Core.Exceptions;

namespace MedEasy.Handlers.Appointment.Commands
{


    /// <summary>
    /// An instance of this class process process <see cref="IRunCreateAppointmentCommand"/>
    /// </summary>
    public class RunCreateAppointmentCommand : IRunCreateAppointmentCommand
    {
        private IMapper Mapper { get; }
        private IUnitOfWorkFactory UowFactory { get; }

        /// <summary>
        /// Builds a new <see cref="RunCreateAppointmentCommand"/> instance.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="mapper"></param>
        public RunCreateAppointmentCommand(IUnitOfWorkFactory factory, IMapper mapper)
        {
            UowFactory = factory;
            Mapper = mapper;
        }


        public async Task<AppointmentInfo> RunAsync(ICreateAppointmentCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            CreateAppointmentInfo info = command.Data;

            using (var uow = UowFactory.New())
            {
                var doctor = await uow.Repository<Objects.Doctor>()
                    .SingleOrDefaultAsync(
                        x => new { x.Id, x.UUID },
                        x => x.Id == info.DoctorId);

                if (doctor == null)
                {
                    throw new NotFoundException($"{nameof(Objects.Doctor)} <{info.DoctorId}> not found");
                }

                var patient = await uow.Repository<Objects.Patient>()
                    .SingleOrDefaultAsync(
                        x => new { x.Id, x.UUID }, 
                        x => x.Id == info.PatientId);

                if (patient == null)
                {
                    throw new NotFoundException($"{nameof(Objects.Patient)} <{info.PatientId}> not found");
                }


                var now = DateTimeOffset.UtcNow;
                Objects.Appointment itemToCreate = new Objects.Appointment()
                {
                    StartDate = info.StartDate,
                    Duration = info.Duration,
                    PatientId = info.PatientId,
                    DoctorId = info.DoctorId,
                    UpdatedDate = now,
                    CreatedDate = now,
                    UUID = Guid.NewGuid()
                };

                uow.Repository<Objects.Appointment>().Create(itemToCreate);
                await uow.SaveChangesAsync();

                return Mapper.Map<AppointmentInfo>(itemToCreate);

            }
        }
    }
}
