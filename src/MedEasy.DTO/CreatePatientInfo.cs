using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    /// <summary>
    /// Informations to give when requesting
    /// </summary>
    [JsonObject]
    public class CreatePatientInfo
    {
        
        [JsonProperty]
        public string Firstname { get; set; }

        [JsonProperty]
        public string Lastname { get; set; }

        [JsonProperty]
        public DateTimeOffset? BirthDate { get; set; }

        [JsonProperty]
        public string BirthPlace { get; set; }

        
        [JsonProperty]
        public int? MainDoctorId { get; set; }


        [JsonProperty]
        public string Fullname => $"{Firstname}{(Firstname != null ? " " : string.Empty)}{Lastname}";
        
    }
}
