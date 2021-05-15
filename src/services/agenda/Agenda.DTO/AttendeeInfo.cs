namespace Agenda.DTO
{
    using Agenda.Ids;

    using MedEasy.RestObjects;

    using Newtonsoft.Json;

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