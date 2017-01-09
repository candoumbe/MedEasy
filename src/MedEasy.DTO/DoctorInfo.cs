using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    [JsonObject]
    public class DoctorInfo : ResourceBase<int>
    {
        

        [JsonProperty]
        public string Firstname { get; set; }

        [JsonProperty]
        public string Lastname { get; set; }
        

        [JsonProperty]
        public int? SpecialtyId { get; set; }
    }

}
