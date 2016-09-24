using Newtonsoft.Json;

namespace MedEasy.DTO
{

    [JsonObject]
    public class CreateSpecialtyInfo
    {
        [JsonProperty]
        public string Code { get; set; }

        [JsonProperty]
        public string Name { get; set; }

    }
}