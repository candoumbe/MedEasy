using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    [JsonObject]
    public class DoctorInfo : IResource<int>
    {
        [JsonProperty]
        public int Id { get; set; }

        [JsonProperty]
        public string Firstname { get; set; }

        [JsonProperty]
        public string Lastname { get; set; }

        [JsonProperty]
        public BrowsableSpecialtyInfo Specialty { get; set; }

        [JsonProperty]
        public DateTime? UpdatedDate { get; set; }

        [JsonProperty]
        public int? SpecialtyId { get; set; }
    }

}
