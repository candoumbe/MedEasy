using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO.Autocomplete
{
    /// <summary>
    /// Model autocomplete response for doctor informations
    /// </summary>
    [JsonObject]
    public class DoctorAutocompleteInfo : AutocompleteInfo<Guid>
    {
        /// <summary>
        /// Doctor's firstname
        /// </summary>
        [JsonProperty]
        public string Firstname { get; set; }
        /// <summary>
        /// Doctor's lastname
        /// </summary>
        [JsonProperty]
        public string Lastname { get; set; }

        /// <summary>
        /// Doctor' specialty
        /// </summary>
        [JsonProperty]
        public string Specialty { get; set; }
        
    }

}
