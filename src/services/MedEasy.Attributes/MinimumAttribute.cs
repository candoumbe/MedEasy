using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MinimumAttribute : RangeAttribute, IEquatable<MinimumAttribute>
    {
        public MinimumAttribute(int minimum) : base(minimum, int.MaxValue)
        {
        }

        public MinimumAttribute(double minimum) : base(minimum, double.MaxValue)
        {
        }

        public MinimumAttribute(long minimum) : base(minimum, long.MaxValue)
        {
        }

        public MinimumAttribute(float minimum) : base(minimum, float.MaxValue)
        {
        }

        /// <inheritdoc/>
        public bool Equals(MinimumAttribute other) => Minimum.Equals(other?.Minimum);

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as MinimumAttribute);

        /// <inheritdoc/>
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be greather than or equals to {Minimum}" ;
        }

        public override int GetHashCode()
        {
            int hashCode = 1062559021;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Minimum);
            return hashCode;
        }

        public static bool operator ==(MinimumAttribute left, MinimumAttribute right)
        {
            return EqualityComparer<MinimumAttribute>.Default.Equals(left, right);
        }

        public static bool operator !=(MinimumAttribute left, MinimumAttribute right)
        {
            return !(left == right);
        }
    }
}
