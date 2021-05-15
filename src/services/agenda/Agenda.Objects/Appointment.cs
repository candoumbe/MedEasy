namespace Agenda.Objects
{
    using MedEasy.Objects;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Optional;
    using NodaTime;
    using Agenda.Ids;

    /// <summary>
    /// An meeting wih a location and a subject
    /// </summary>
    public class Appointment : AuditableEntity<AppointmentId, Appointment>
    {
        private readonly IList<Attendee> _attendees;

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
        public Instant StartDate { get; private set; }

        /// <summary>
        /// End date of the <see cref="Appointment"/>
        /// </summary>
        public Instant EndDate { get; private set; }

        /// <summary>
        /// Participants of the <see cref="Appointment"/>
        /// </summary>
        public IList<Attendee> Attendees => _attendees;

        public AppointmentStatus Status { get; }

        public Appointment(AppointmentId id, string subject, string location, Instant startDate, Instant endDate) : base(id)
        {
            Subject = subject;
            Location = location ?? string.Empty;
            StartDate = startDate;
            EndDate = endDate;
            _attendees = new List<Attendee>();
        }

        /// <summary>
        /// Adds an attendee to the current instance
        /// </summary>
        /// <param name="attendee">The participant to add</param>
        public void AddAttendee(Attendee attendee)
        {
            _attendees.Add(attendee);
        }

        /// <summary>
        /// Removes the <see cref="Attendee"/> with the specified <see cref="Attendee.UUID"/>
        /// </summary>
        /// <param name="attendeeId">ID of the attendee to remove</param>
        public void RemoveAttendee(AttendeeId attendeeId)
        {
            Option<Attendee> optionalAttendeee = _attendees.SingleOrDefault(x => x.Id == attendeeId)
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
        public void Reschedule(ZonedDateTime newStartDate, ZonedDateTime newEndDate)
        {
            StartDate = newStartDate.ToInstant();
            EndDate = newEndDate.ToInstant();
        }
    }
}
