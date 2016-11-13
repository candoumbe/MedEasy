using Newtonsoft.Json;

namespace MedEasy.DTO
{

    [JsonObject]
    public class SpecialtyInfo : ResourceBase<int>
    {
        /// <summary>
        /// Name of the specialty
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }   
    }
}