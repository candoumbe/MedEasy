using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.Objects;
using AutoMapper;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.CQRS.Core.Exceptions;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MedEasy.CQRS.Core.Commands.Results.ModifyCommandResult;

namespace Agenda.CQRS.Features.Appointments.Handlers
{
    /// <summary>
    /// Handles <see cref="ChangeAppointmentDateCommand"/>
    /// </summary>
    public class HandleChangeAppointmentDateCommand : IRequestHandler<ChangeAppointmentDateCommand, ModifyCommandResult>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IMapper _mapper;

        public HandleChangeAppointmentDateCommand(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _mapper = mapper;
        }

        public async Task<ModifyCommandResult> Handle(ChangeAppointmentDateCommand request, CancellationToken cancellationToken)
        {
            (Guid appointmentId, DateTimeOffset start, DateTimeOffset end) = request.Data;

            if (appointmentId == default)
            {
                throw new CommandNotValidException<Guid>(request.Id, new ErrorInfo[]
                {
                    new ErrorInfo(nameof(appointmentId), "Appointment ID cannot be empty", ErrorLevel.Error)
                });
            }
            if (start == default)
            {
                throw new CommandNotValidException<Guid>(request.Id, new ErrorInfo[]
                {
                    new ErrorInfo(nameof(appointmentId), "Start date cannot be empty", ErrorLevel.Error)
                });
            }
            if (end == default)
            {
                throw new CommandNotValidException<Guid>(request.Id, new ErrorInfo[]
                {
                    new ErrorInfo(nameof(appointmentId), "End date cannot be empty", ErrorLevel.Error)
                });
            }

            if (start > end)
            {
                throw new CommandNotValidException<Guid>(request.Id, new ErrorInfo[]{
                    new ErrorInfo("start", "Start date greater than end date", ErrorLevel.Error)
                });
            }

            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                Option<Appointment> optionalAppointment = await uow.Repository<Appointment>()
                                                                   .SingleOrDefaultAsync(app => app.Id == appointmentId, cancellationToken)
                                                                   .ConfigureAwait(false);

                return await optionalAppointment.Match(
                    some : async (appointment) =>
                    {
                        ModifyCommandResult result;

                        bool willOverlapAnotherAppointment = await uow.Repository<Appointment>()
                            .AnyAsync(app => app.Id != appointmentId
                                && ((start.CompareTo(app.StartDate) <= 0 && app.StartDate.CompareTo(end) <= 0) || (start.CompareTo(app.EndDate) <= 0 && app.EndDate.CompareTo(end) <= 0))   // another appointment starts before and end after
                            )
                            .ConfigureAwait(false);

                        if (willOverlapAnotherAppointment)
                        {
                            result = Failed_Conflict;
                        } else {
                            appointment.Reschedule(request.Data.start, request.Data.end);

                            await uow.SaveChangesAsync(cancellationToken)
                                .ConfigureAwait(false);

                            result = Done;
                        }
                        return result;
                    },
                    none: () => Task.FromResult(Failed_NotFound)
                )
                    .ConfigureAwait(false);
            }
        }
    }
}
