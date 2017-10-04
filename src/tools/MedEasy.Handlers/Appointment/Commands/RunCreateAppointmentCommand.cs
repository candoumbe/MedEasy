using MedEasy.DAL.Interfaces;
using MedEasy.Commands.Appointment;
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Handlers.Core.Exceptions;
using System;
using MedEasy.DTO;
using System.Threading.Tasks;
using AutoMapper;
using System.Threading;
using Optional;

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


        public async Task<Option<AppointmentInfo, CommandException>> RunAsync(ICreateAppointmentCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            CreateAppointmentInfo info = command.Data;

            using (IUnitOfWork uow = UowFactory.New())
            {

                Option<AppointmentInfo, CommandException> result = default;
                var doctor = await uow.Repository<Objects.Doctor>()
                    .SingleOrDefaultAsync(x => new { x.Id, x.UUID },
                        (Objects.Doctor x) => x.UUID == info.DoctorId);

                if (!doctor.HasValue)
                {
                    result = Option.None<AppointmentInfo, CommandException>(new CommandEntityNotFoundException($"{nameof(Objects.Doctor)} <{info.DoctorId}> not found"));
                }
                var patient = await uow.Repository<Objects.Patient>()
                    .SingleOrDefaultAsync(
                        x => new { x.Id, x.UUID },
                        (Objects.Patient x) => x.UUID == info.PatientId);
                
                if (!patient.HasValue)
                {
                    result = Option.None<AppointmentInfo, CommandException>(new CommandEntityNotFoundException($"{nameof(Objects.Patient)} <{info.PatientId}> not found"));
                }
                
                foreach (var doctorId in doctor)
                {
                    foreach (var patientId in patient)
                    {
                        DateTimeOffset now = DateTimeOffset.UtcNow;
                        Objects.Appointment itemToCreate = new Objects.Appointment()
                        {
                            StartDate = info.StartDate,
                            EndDate = info.StartDate.AddMinutes(info.Duration),
                            PatientId = patientId.Id,
                            DoctorId = doctorId.Id,
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
                        output.Duration = info.Duration;
                        result = Option.Some<AppointmentInfo, CommandException>(output);
                    }

                }
                return result;
            }
        }
    }
}
