using Newtonsoft.Json;
using System;

namespace MedEasy.DTO
{

    [JsonObject]
    public class SpecialtyInfo : ResourceBase<Guid>
    {
        /// <summary>
        /// Name of the specialty
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }   
    }
}