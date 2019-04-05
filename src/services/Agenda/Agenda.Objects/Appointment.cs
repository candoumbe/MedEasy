using MedEasy.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Optional;

namespace Agenda.Objects
{
    /// <summary>
    /// An meeting wih a location and a subject
    /// </summary>
    public class Appointment : AuditableEntity<int, Appointment>
    {
        private readonly IList<AppointmentAttendee> _attendees;

        /// <summary>
        /// Location of the appointment
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Subject of the <see cref="Appointment"/>
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Start date of the appointment
        /// </summary>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// End date of the <see cref="Appointment"/>
        /// </summary>
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Participants of the <see cref="Appointment"/>
        /// </summary>
        public IEnumerable<AppointmentAttendee> Attendees => _attendees;

        public Appointment() => _attendees = new List<AppointmentAttendee>();

        /// <summary>
        /// Adds an attendee to the current <see cref=""/>
        /// </summary>
        /// <param name="attendee">The participant to add</param>
        /// <returns><c>true</c> if <see cref="participant"/> was successfully added and <c>false</c> otherwise</returns>
        public void AddAttendee(Attendee attendee)
        {
            if (attendee.UUID == default)
            {
                attendee.UUID = Guid.NewGuid();
            }
            _attendees.Add(new AppointmentAttendee { Attendee = attendee , Appointment = this});
        }

        /// <summary>
        /// Removes the <see cref="Attendee"/> with the specified <see cref="Attendee.UUID"/>
        /// </summary>
        /// <param name="attendeeId">ID of the attendee to remove</param>
        public void RemoveAttendee(Guid attendeeId)
        {
            Option<AppointmentAttendee> optionalAttendeee = _attendees.SingleOrDefault(x => x.Attendee.UUID == attendeeId)
                .SomeNotNull();

            optionalAttendeee.MatchSome((participant) => _attendees.Remove(participant));
        }
    }
}
