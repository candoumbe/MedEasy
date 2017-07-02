using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    [JsonObject]
    public class DoctorInfo : Resource<Guid>
    {
        

        [JsonProperty(PropertyName = nameof(Firstname))]
        public string Firstname { get; set; }

        [JsonProperty(PropertyName = nameof(Lastname))]
        public string Lastname { get; set; }
        

        [JsonProperty(PropertyName = nameof(SpecialtyId))]
        public Guid? SpecialtyId { get; set; }
    }

}
