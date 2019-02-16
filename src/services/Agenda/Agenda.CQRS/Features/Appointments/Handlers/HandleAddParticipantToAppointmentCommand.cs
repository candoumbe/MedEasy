using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MedEasy.Objects;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Appointments.Handlers
{
    public class HandleAddParticipantToAppointmentCommand : IRequestHandler<AddParticipantToAppointmentCommand, ModifyCommandResult>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public HandleAddParticipantToAppointmentCommand(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<ModifyCommandResult> Handle(AddParticipantToAppointmentCommand request, CancellationToken ct)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                var optionalParticipant = await uow.Repository<Participant>()
                                .SingleOrDefaultAsync(x => new { x.Id }, x => x.UUID == request.Data.participantId)
                                .ConfigureAwait(false);

                return await optionalParticipant.Match(
                    some: async (participant) =>
                    {
                        var optionalAppointment = await uow.Repository<Appointment>()
                                 .SingleOrDefaultAsync(x => new { x.Id }, x => x.UUID == request.Data.appointmentId)
                                 .ConfigureAwait(false);

                        return await optionalAppointment.Match(
                            some: async (appointment) =>
                            {
                                ModifyCommandResult cmdResult;

                                bool associationAlreadyExists = await uow.Repository<AppointmentParticipant>()
                                     .AnyAsync(ap => ap.ParticipantId == participant.Id && ap.AppointmentId == appointment.Id)
                                     .ConfigureAwait(false);

                                if (!associationAlreadyExists)
                                {
                                    uow.Repository<AppointmentParticipant>().Create(new AppointmentParticipant { AppointmentId = appointment.Id, ParticipantId = participant.Id });
                                    await uow.SaveChangesAsync().ConfigureAwait(false);
                                    cmdResult = ModifyCommandResult.Done;
                                }
                                else
                                {
                                    cmdResult = ModifyCommandResult.Failed_Conflict;
                                }

                                return cmdResult;
                            },
                            none: () => Task.FromResult(ModifyCommandResult.Failed_NotFound)
                         );
                    },
                    none: () => Task.FromResult(ModifyCommandResult.Failed_NotFound)
                );
            }
        }
    }
}

