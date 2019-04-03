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
    /// Handles commands that removes a <see cref="Attendee"/> from an <see cref="Appointment"/>
    /// </summary>
    public class HandleRemoveAttendeeFromAppointmentByIdCommand : IRequestHandler<RemoveAttendeeFromAppointmentByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        /// Builds a new <see cref="HandleRemoveAttendeeFromAppointmentByIdCommand"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory"></param>
        public HandleRemoveAttendeeFromAppointmentByIdCommand(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<DeleteCommandResult> Handle(RemoveAttendeeFromAppointmentByIdCommand request, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                var optionalAppointment = await uow.Repository<AppointmentAttendee>()
                                 .SingleOrDefaultAsync(
                                    selector: ap => new { ap.AppointmentId, ap.AttendeeId  },
                                    predicate : ap => ap.Appointment.UUID == request.Data.appointmentId && ap.Attendee.UUID == request.Data.attendeeId, cancellationToken)
                                 .ConfigureAwait(false);

                return await optionalAppointment.Match(
                    some: async (appointment) =>
                    {
                        uow.Repository<AppointmentAttendee>().Delete(ap => ap.AttendeeId == appointment.AttendeeId && ap.AppointmentId == ap.AppointmentId);
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

