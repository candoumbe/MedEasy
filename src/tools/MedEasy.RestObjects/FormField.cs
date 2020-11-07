#if NETSTANDARD1_1
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#else
using System.Text.Json.Serialization;
#endif
namespace MedEasy.RestObjects
{
    /// <summary>
    /// Form field representation
    /// </summary>
    /// <remarks>
    ///     Inspired by ION spec (see http://ionwg.org/draft-ion.html#form-fields for more details)
    /// </remarks>
#if NETSTANDARD1_1
    [JsonObject]
#endif
    public class FormField
    {
        /// <summary>
        /// indicates whether or not the field value may be modified or submitted to a linked resource location. 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public bool? Enabled { get; set; }

        /// <summary>
        /// Type of the field 
        /// </summary>
#if NETSTANDARD1_1
        [JsonConverter(typeof(StringEnumConverter))]
#else
        [JsonConverter(typeof(JsonStringEnumConverter))]
#endif
        public FormFieldType Type { get; set; }

        /// <summary>
        /// Description of the field
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public string Label { get; set; }

        /// <summary>
        /// Name of the field that should be submitted
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public string Name { get; set; }

        /// <summary>
        /// Regular expression that the field should be validated against.
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public string Pattern { get; set; }

        /// <summary>
        /// Short hint that described the expected value of the field.
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public string Placeholder { get; set; }

        /// <summary>
        /// Indicates if the field must be submitted
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public bool? Required { get; set; }

        /// <summary>
        /// Indicates the maximum length of the value
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public int? MaxLength { get; set; }

        /// <summary>
        /// Indicates the minimum length of the value
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public int? MinLength { get; set; }

        /// <summary>
        /// Indicates that <see cref="Value"/>value must be greater than or equal to the specified <see cref="Min"/> value
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif        
        public int? Min { get; set; }

        /// <summary>
        /// Indicates that <see cref="Value"/>value must be less than or equal to the specified <see cref="Max"/> value
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public int? Max { get; set; }

        /// <summary>
        /// Indicates whether or not the field value is considered sensitive information 
        /// and should be kept secret.
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public bool? Secret { get; set; }

        /// <summary>
        /// a string description of the field that may be used to enhance usability.
        /// </summary>
#if NETSTANDARD1_1
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#endif
        public string Description { get; set; }
    }
}