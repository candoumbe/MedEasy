using MedEasy.RestObjects;
using Newtonsoft.Json;
using System;

namespace Agenda.DTO
{
    [JsonObject]
    public class AttendeeInfo : Resource<Guid>
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string PhoneNumber { get; set; }

        [JsonProperty]
        public string Email { get; set; }
    }
}