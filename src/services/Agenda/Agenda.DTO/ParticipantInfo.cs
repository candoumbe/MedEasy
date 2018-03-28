using MedEasy.RestObjects;
using System;

namespace Agenda.DTO
{
    public class ParticipantInfo : Resource<Guid>
    {
        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }
    }
}