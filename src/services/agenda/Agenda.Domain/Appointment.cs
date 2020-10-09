using Agenda.Domain.Events;

using MedEasy.Domain.Core;

using System;
using System.Collections.Generic;

namespace Agenda.Domain
{
    public class Appointment : Aggregate<Guid>
    {
        private readonly ISet<Attendee> _attendees;

        /// <summary>
        /// Location of the appointment
        /// </summary>
        private string Location { get; }

        /// <summary>
        /// Subject of the <see cref="Appointment"/>
        /// </summary>
        private string Subject { get; set; }

        /// <summary>
        /// Start date of the appointment
        /// </summary>
        private DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the <see cref="Appointment"/>
        /// </summary>
        private DateTime EndDate { get;  set; }

        /// <summary>
        /// Participants of the <see cref="Appointment"/>
        /// </summary>
        public IEnumerable<Attendee> Attendees => _attendees;

        public Appointment(Guid id) : base(id)
        {
            _attendees = new HashSet<Attendee>();

            On<AppointmentCreated>(e =>
            {
                StartDate = e.Start;
                EndDate = e.End;
                Subject = e.Title;
                foreach (Attendee attendee in e.Attendees)
                {
                    _attendees.Add(attendee);
                }
            });

            On<AppointmentRescheduled>(evt =>
            {
                StartDate = evt.Start;
                EndDate = evt.End;
            });
        }

        /// <summary>
        /// Initialize a new appointment and raises <see cref="AppointmentCreated"/> event
        /// </summary>
        /// <param name="start">when the appointment starts</param>
        /// <param name="end">when the appointment ends</param>
        /// <param name="title">a title</param>
        /// <param name="attendees">participants of the appointments.</param>
        public void Initialize(DateTime start, DateTime end, string title, ISet<Attendee> attendees)
        {
            Raise(new AppointmentCreated(Id, start, end, title, attendees));
        }

        public Status Status { get; }

        public void Reschedule(DateTime newStart, DateTime newEnd)
        {
            Raise(new AppointmentRescheduled(Id, newStart, newEnd));
        }

        /// <summary>
        /// Cancels the current appointment
        /// </summary>
        /// <param name="by">id of the appointment cancelor</param>
        public void Cancel(Guid by)
        {
            Raise(new AppointmentCancelled(Id, by));
        }

        public override string ToString() => this.Jsonify();
    }
}
