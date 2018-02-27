using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// Form field representation
    /// </summary>
    /// <remarks>
    ///     Inspired by ION spec (see http://ionwg.org/draft-ion.html#form-fields for more details)
    /// </remarks>
    [JsonObject]
    public class FormField
    {
        /// <summary>
        /// indicates whether or not the field value may be modified or submitted to a linked resource location. 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Type of the field 
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FormFieldType Type { get; set; }

        /// <summary>
        /// Description of the field
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        /// <summary>
        /// Name of the field that should be submitted
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// Regular expression that the field should be validated against.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Pattern { get; set; }

        /// <summary>
        /// Short hint that described the expected value of the field.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Placeholder { get; set; }

        /// <summary>
        /// Indicates if the field must be submitted
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Required { get; set; }

        /// <summary>
        /// Indicates the maximum length of the value
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxLength { get; set; }

        /// <summary>
        /// Indicates the minimum length of the value
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MinLength { get; set; }

        /// <summary>
        /// Indicates that <see cref="Value"/>value must be greater than or equal to the specified <see cref="Min"/> value
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Min { get; set; }

        /// <summary>
        /// Indicates that <see cref="Value"/>value must be less than or equal to the specified <see cref="Max"/> value
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Max { get; set; }


        /// <summary>
        /// Indicates whether or not the field value is considered sensitive information 
        /// and should be kept secret.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Secret { get; set; }

        /// <summary>
        /// a string description of the field that may be used to enhance usability.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }


    }
}