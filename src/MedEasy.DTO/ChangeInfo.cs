using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.DTO
{
    /// <summary>
    /// A change to apply to a resource
    /// </summary>
    [JsonObject]
    public class ChangeInfo : IEquatable<ChangeInfo>
    {
        /// <summary>
        /// Type of change to apply
        /// </summary>
        public ChangeInfoType Op { get; set; }

        /// <summary>
        /// Path of the change
        /// </summary>
        /// <remarks>
        /// Should starts with '/' followed by the name of the property on which the change will be applied
        /// </remarks>
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// Old value
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// New value
        /// </summary>
        public object Value { get; set; }

        public override string ToString() => SerializeObject(this);

        public override int GetHashCode() 
            => Op.GetHashCode() + 
            (Path?.GetHashCode() ?? 0) + 
            (From?.GetHashCode() ?? 0) + 
            (Value?.GetHashCode() ?? 0);


        public override bool Equals(object obj)
        {
            bool equals = false;

            if (obj != null)
            {
                if (obj is ChangeInfo)
                {
                    equals = ReferenceEquals(this, obj) || Equals((ChangeInfo)obj);
                }
            }

            return equals;
        }

        public bool Equals(ChangeInfo other) 
            => other != null && 
            ((ReferenceEquals(other, this) ||
                (Equals(Op, other.Op) && Equals(Path, other.Path) && Equals(From, other.From) && Equals(Value, other.Value))));
    }
}
