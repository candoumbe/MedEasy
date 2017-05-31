using Newtonsoft.Json;
using System;

namespace MedEasy.DTO
{

    [JsonObject]
    public class SpecialtyInfo : Resource<Guid>
    {
        /// <summary>
        /// Name of the specialty
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }   
    }
}