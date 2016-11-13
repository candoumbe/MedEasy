using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.DTO
{
    /// <summary>
    /// Data to create a <see cref="SpecialtyInfo"/>
    /// </summary>
    [JsonObject]
    public class CreateSpecialtyInfo
    {
        
        /// <summary>
        /// Name of the specialty
        /// </summary>
        [Required]
        [JsonProperty]
        public string Name { get; set; }

    }
}