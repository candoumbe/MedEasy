using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DTO;
using Agenda.Objects;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Linq.Expressions;
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
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {

                Appointment appointment = _mapper.Map<NewAppointmentInfo, Appointment>(request.Data);
                foreach (ParticipantInfo participantInfo in request.Data.Participants)
                {
                    Participant participant = _mapper.Map<ParticipantInfo, Participant>(participantInfo);
                    appointment.AddParticipant(participant);
                }
                uow.Repository<Appointment>().Create(appointment);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                return _mapper.Map<Appointment, AppointmentInfo>(appointment);

            }


        }
    }
}
