using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{

    [JsonObject]
    public class SpecialtyInfo : IResource<int>
    {
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public string Code { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public DateTimeOffset? UpdatedDate { get; set; }
        
    }
}