namespace Agenda.CQRS.Features.Appointments.Handlers
{
    using Agenda.CQRS.Features.Appointments.Commands;
    using Agenda.Objects;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;

    using MediatR;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="AddAttendeeToAppointmentCommand"/> commands.
    /// </summary>
    public class HandleAddParticipantToAppointmentCommand : IRequestHandler<AddAttendeeToAppointmentCommand, ModifyCommandResult>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        /// Builds a new <see cref="HandleAddParticipantToAppointmentCommand"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HandleAddParticipantToAppointmentCommand(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        ///<inheritdoc/>
        public async Task<ModifyCommandResult> Handle(AddAttendeeToAppointmentCommand request, CancellationToken ct)
        {
            using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();
            Option<Attendee> optionalParticipant = await uow.Repository<Attendee>()
                                                            .SingleOrDefaultAsync(x => x.Id == request.Data.attendeeId, ct)
                                                            .ConfigureAwait(false);

            return await optionalParticipant.Match(
                some: async (participant) =>
                {
                    Option<Appointment> optionalAppointment = await uow.Repository<Appointment>()
                                                                       .SingleOrDefaultAsync(x => x.Id == request.Data.appointmentId,
                                                                                             includedProperties: new IncludeClause<Appointment>[] { IncludeClause<Appointment>.Create<IEnumerable<Attendee>>(x => x.Attendees) },
                                                                                             ct)
                                                                       .ConfigureAwait(false);

                    return await optionalAppointment.Match(
                        some: async (appointment) =>
                        {
                            ModifyCommandResult cmdResult;

                            bool associationAlreadyExists = appointment.Attendees.AtLeastOnce(x => x.Id == request.Data.attendeeId);

                            if (!associationAlreadyExists)
                            {
                                appointment.AddAttendee(participant);

                                await uow.SaveChangesAsync(ct).ConfigureAwait(false);
                                cmdResult = ModifyCommandResult.Done;
                            }
                            else
                            {
                                cmdResult = ModifyCommandResult.Failed_Conflict;
                            }

                            return cmdResult;
                        },
                        none: () => Task.FromResult(ModifyCommandResult.Failed_NotFound)
                     )
                    .ConfigureAwait(false);
                },
                none: () => Task.FromResult(ModifyCommandResult.Failed_NotFound)
            ).ConfigureAwait(false);
        }
    }
}

