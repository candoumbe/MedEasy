using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics;
using AutoMapper;
using MedEasy.Commands.Appointment;
using MedEasy.Handlers.Core.Appointment.Commands;

namespace MedEasy.Handlers.Appointment.Commands
{


    /// <summary>
    /// An instance of this class process process <see cref="IRunCreateAppointmentCommand"/>
    /// </summary>
    public class RunCreateAppointmentCommand : IRunCreateAppointmentCommand
    {
        private readonly IMapper _mapper;

        public RunCreateAppointmentCommand(IUnitOfWorkFactory factory, IMapper mapper)
        {
            UowFactory = factory;
            _mapper = mapper;
        }

        private IUnitOfWorkFactory UowFactory { get; }

        public async Task<AppointmentInfo> RunAsync(ICreateAppointmentCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            CreateAppointmentInfo info = command.Data;
            
            using (var uow = UowFactory.New())
            {
                var now = DateTimeOffset.UtcNow;
                Objects.Appointment itemToCreate = new Objects.Appointment()
                {
                    StartDate = info.StartDate,
                    Duration = info.Duration,
                    PatientId = info.PatientId,
                    DoctorId = info.DoctorId,
                    UpdatedDate = now,
                    CreatedDate = now,
                };

                uow.Repository<Objects.Appointment>().Create(itemToCreate);
                await uow.SaveChangesAsync();

                return _mapper.Map<AppointmentInfo>(itemToCreate); 

            }
        }
    }
}
