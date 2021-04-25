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
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IMapper _mapper;

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
            await uow.SaveChangesAsync(ct)
                .ConfigureAwait(false);

            AppointmentInfo info = _mapper.Map<Appointment, AppointmentInfo>(appointment);
            info.StartDate = appointment.StartDate;
            info.EndDate = appointment.EndDate;

            return info;
        }
    }
}
