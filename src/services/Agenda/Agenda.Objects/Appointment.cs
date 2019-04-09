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
        private readonly HashSet<AppointmentAttendee> _attendees;

        /// <summary>
        /// Location of the appointment
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Subject of the <see cref="Appointment"/>
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// Start date of the appointment
        /// </summary>
        public DateTimeOffset StartDate { get; private set; }

        /// <summary>
        /// End date of the <see cref="Appointment"/>
        /// </summary>
        public DateTimeOffset EndDate { get; private set; }

        /// <summary>
        /// Participants of the <see cref="Appointment"/>
        /// </summary>
        public IEnumerable<AppointmentAttendee> Attendees => _attendees;

        public AppointmentStatus Status { get; }

        
        public Appointment(Guid uuid, string subject, string location, DateTimeOffset startDate, DateTimeOffset endDate) : base(uuid)
        {
            Subject = subject;
            Location = location ?? string.Empty;
            StartDate = startDate;
            EndDate = endDate;
            _attendees = new HashSet<AppointmentAttendee>();
        }

        /// <summary>
        /// Adds an attendee to the current <see cref=""/>
        /// </summary>
        /// <param name="attendee">The participant to add</param>
        public void AddAttendee(Attendee attendee)
        {
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

            optionalAttendeee.MatchSome((attendee) => _attendees.Remove(attendee));
        }

        /// <summary>
        /// Update the <see cref="Subject"/> of the <see cref="Appointment"/>
        /// </summary>
        /// <param name="newSubject">The new subject</param>
        /// <exception cref="ArgumentNullException">if <paramref name="newSubject"/> is <c>null</c></exception>
        public void ChangeSubjectTo(string newSubject) => Subject = newSubject ?? throw new ArgumentNullException(nameof(newSubject));

        /// <summary>
        /// Changes the <see cref="StartDate"/> and <see cref="EndDate"/> of the <see cref="Appointment"/>
        /// </summary>
        /// <param name="newStartDate"></param>
        /// <param name="newEndDate"></param>
        public void Reschedule(DateTimeOffset newStartDate, DateTimeOffset newEndDate)
        {
            StartDate = newStartDate;
            EndDate = newEndDate;
        }
    }
}
