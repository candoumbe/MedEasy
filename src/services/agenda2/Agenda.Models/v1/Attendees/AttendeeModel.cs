using MedEasy.Models;
using Newtonsoft.Json;
using System;

namespace Agenda.Models.v1.Attendees
{
    [JsonObject]
    public class AttendeeModel : ModelBase<Guid>
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string PhoneNumber { get; set; }

        [JsonProperty]
        public string Email { get; set; }
    }
}