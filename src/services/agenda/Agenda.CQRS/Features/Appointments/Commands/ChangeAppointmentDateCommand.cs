using MedEasy.Abstractions;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    public class ChangeAppointmentDateCommand : CommandBase<Guid, (Guid appointmentId, DateTimeOffset start, DateTimeOffset end), ModifyCommandResult>
    {
        private readonly IDateTimeService _datetimeService;

        public ChangeAppointmentDateCommand((Guid appointmentId, DateTimeOffset start, DateTimeOffset end) data) : base (Guid.NewGuid(), data)
        { 
        }
    }
}
