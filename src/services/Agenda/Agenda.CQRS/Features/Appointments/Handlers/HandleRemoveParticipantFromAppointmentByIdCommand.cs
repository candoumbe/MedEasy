using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Appointments.Handlers
{
    /// <summary>
    /// Handles commands that removes a <see cref="Participant"/> from an <see cref="Appointment"/>
    /// </summary>
    public class HandleRemoveParticipantFromAppointmentByIdCommand : IRequestHandler<RemoveParticipantFromAppointmentByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        /// Builds a new <see cref="HandleRemoveParticipantFromAppointmentByIdCommand"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory"></param>
        public HandleRemoveParticipantFromAppointmentByIdCommand(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<DeleteCommandResult> Handle(RemoveParticipantFromAppointmentByIdCommand request, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                var optionalAppointment = await uow.Repository<AppointmentParticipant>()
                                 .SingleOrDefaultAsync(
                                    ap => new { ap.AppointmentId, ap.ParticipantId  }, 
                                    ap => ap.Appointment.UUID == request.Data.appointmentId && ap.Participant.UUID == request.Data.participantId, cancellationToken)
                                 .ConfigureAwait(false);

                return await optionalAppointment.Match(
                    some: async (appointment) =>
                    {
                        uow.Repository<AppointmentParticipant>().Delete(ap => ap.ParticipantId == appointment.ParticipantId && ap.AppointmentId == ap.AppointmentId);
                        await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        return DeleteCommandResult.Done;
                    },
                    none: () => Task.FromResult(DeleteCommandResult.Failed_NotFound)
                 )
                 .ConfigureAwait(false);
            }
        }
    }
}

