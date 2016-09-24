using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    [JsonObject]
    public class PatientInfo : IResource<int>
    {
        [JsonProperty]
        public int Id { get; set; }

        [JsonProperty]
        public string Firstname { get; set; }

        [JsonProperty]
        public string Lastname { get; set; }

        [JsonProperty]
        public DateTime? BirthDate { get; set; }

        [JsonProperty]
        public string BirthPlace { get; set; }

        [JsonProperty]
        public BrowsableDoctorInfo MainDoctor { get; set; }

        [JsonProperty]
        public int? MainDoctorId { get; set; }


        [JsonProperty]
        public string Fullname { get; set; }

        [JsonProperty]
        public DateTime? UpdatedDate { get; set; }
        
    }
}
