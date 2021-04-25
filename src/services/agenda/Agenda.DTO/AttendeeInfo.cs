using Agenda.Ids;

using MedEasy.RestObjects;

using Newtonsoft.Json;

namespace Agenda.DTO
{
    [JsonObject]
    public class AttendeeInfo : Resource<AttendeeId>
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string PhoneNumber { get; set; }

        [JsonProperty]
        public string Email { get; set; }
    }
}