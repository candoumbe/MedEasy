namespace Agenda.Models.v1.Attendees
{
    using Agenda.Ids;

    using MedEasy.Models;

    public class AttendeeModel : ModelBase<AttendeeId>
    {
        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }
    }
}