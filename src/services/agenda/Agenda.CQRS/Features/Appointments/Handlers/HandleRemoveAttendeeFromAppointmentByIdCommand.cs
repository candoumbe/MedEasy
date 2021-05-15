namespace Agenda.CQRS.Features.Appointments.Handlers
{
    using Agenda.CQRS.Features.Appointments.Commands;
    using Agenda.Objects;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;

    using MediatR;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

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
            using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();
            var optionalAppointment = await uow.Repository<Appointment>()
                                               .SingleOrDefaultAsync(appointment => appointment.Id == request.Data.appointmentId,
                                                                     includedProperties: new[] { IncludeClause<Appointment>.Create<IEnumerable<Attendee>>(x => x.Attendees) },
                                                                     cancellationToken)
                                               .ConfigureAwait(false);

            return await optionalAppointment.Match(
                some: async (appointment) =>
                {
                    DeleteCommandResult result = DeleteCommandResult.Failed_NotFound;
                    if (appointment.Attendees.AtLeastOnce(x => x.Id == request.Data.attendeeId))
                    {
                        appointment.RemoveAttendee(request.Data.attendeeId);
                        await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        result = DeleteCommandResult.Done;
                    }
                    return result;
                },
                none: () => Task.FromResult(DeleteCommandResult.Failed_NotFound)
             )
             .ConfigureAwait(false);
        }
    }
}

