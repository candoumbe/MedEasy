using MedEasy.Models;
using System;

namespace Agenda.Models.v1.Attendees
{
    public class AttendeeModel : ModelBase<Guid>
    {
        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }
    }
}