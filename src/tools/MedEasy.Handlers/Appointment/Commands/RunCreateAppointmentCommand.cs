using MedEasy.DAL.Interfaces;
using MedEasy.Commands.Appointment;
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Handlers.Core.Exceptions;
using System;
using MedEasy.DTO;
using System.Threading.Tasks;
using AutoMapper;
using System.Threading;

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


        public async Task<AppointmentInfo> RunAsync(ICreateAppointmentCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            CreateAppointmentInfo info = command.Data;

            using (IUnitOfWork uow = UowFactory.New())
            {
                var doctor = await uow.Repository<Objects.Doctor>()
                    .SingleOrDefaultAsync(x => new { x.Id, x.UUID },
                        (Objects.Doctor x) => x.UUID == info.DoctorId);

                if (doctor == null)
                {
                    throw new NotFoundException($"{nameof(Objects.Doctor)} <{info.DoctorId}> not found");
                }

                var patient = await uow.Repository<Objects.Patient>()
                    .SingleOrDefaultAsync(
                        x => new { x.Id, x.UUID },
                        (Objects.Patient x) => x.UUID == info.PatientId);

                if (patient == null)
                {
                    throw new NotFoundException($"{nameof(Objects.Patient)} <{info.PatientId}> not found");
                }


                DateTimeOffset now = DateTimeOffset.UtcNow;
                Objects.Appointment itemToCreate = new Objects.Appointment()
                {
                    StartDate = info.StartDate,
                    Duration = info.Duration,
                    PatientId = patient.Id,
                    DoctorId = doctor.Id,
                    UpdatedDate = now,
                    CreatedDate = now,
                    UUID = Guid.NewGuid()
                };

                uow.Repository<Objects.Appointment>().Create(itemToCreate);
                await uow.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                AppointmentInfo output = Mapper.Map<AppointmentInfo>(itemToCreate);
                output.PatientId = info.PatientId;
                output.DoctorId = info.DoctorId;
                return output;
            }
        }
    }
}
