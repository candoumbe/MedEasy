using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.DTO;
using Agenda.Objects;
using AutoMapper;
using MedEasy.DAL.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Appointments.Handlers
{
    public class HandleCreateAppointmentInfoCommand : IRequestHandler<CreateAppointmentInfoCommand, AppointmentInfo>
    {
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;

        /// <summary>
        /// Builds a <see cref="HandleCreateAppointmentInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="mapper"></param>
        public HandleCreateAppointmentInfoCommand(IUnitOfWorkFactory uowFactory, IMapper mapper)
        {
            _uowFactory = uowFactory;
            _mapper = mapper;
        }

        public async Task<AppointmentInfo> Handle(CreateAppointmentInfoCommand request, CancellationToken ct)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

            Appointment appointment = _mapper.Map<NewAppointmentInfo, Appointment>(request.Data);
            foreach (AttendeeInfo participantInfo in request.Data.Attendees)
            {
                Attendee participant = _mapper.Map<AttendeeInfo, Attendee>(participantInfo);
                appointment.AddAttendee(participant);
            }
            uow.Repository<Appointment>().Create(appointment);
            await uow.SaveChangesAsync()
                .ConfigureAwait(false);

            return _mapper.Map<Appointment, AppointmentInfo>(appointment);
        }
    }
}
