﻿using MedEasy.Objects;

using System;
using System.Collections.Generic;

namespace Agenda.Objects
{
    /// <summary>
    /// Participant of a <see cref="Appointment"/>
    /// </summary>
    public class Attendee : AuditableEntity<Guid, Attendee>
    {
        private string _name;

        /// <summary>
        /// Name of the participant
        /// </summary>
        public string Name
        {
            get => _name;
            private set => _name = value?.ToTitleCase() ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Phone number of the participant
        /// </summary>
        public string PhoneNumber { get; private set; }

        /// <summary>
        /// Email of the participant
        /// </summary>
        public string Email { get; private set; }

        private readonly IList<AppointmentAttendee> _appointments;

        public IEnumerable<AppointmentAttendee> Appointments => _appointments;

        /// <summary>
        /// Builds a new <see cref="Attendee"/> instance
        /// </summary>
        /// <param name="name">Name of the participant</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="id"/> is <c>Guid.Empty</c></exception>
        public Attendee(Guid id, string name, string email = null, string phoneNumber = null) : base(id)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
            _appointments = new List<AppointmentAttendee>();
        }

        /// <summary>
        /// Changes attendee's name
        /// </summary>
        /// <param name="newName"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="newName"/> is <c>null</c></exception>
        public void ChangeNameTo(string newName) => Name = newName;


        /// <summary>
        /// Changes <see cref="Attendee"/>'s <see cref="Email"/>
        /// </summary>
        /// <param name="newEmail">new email</param>
        public void ChangeEmail(string newEmail) => Email = newEmail;
    }
}
