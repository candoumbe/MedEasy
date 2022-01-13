namespace MedEasy.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Specifies the minimum value for a numeric parameter or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MinimumAttribute : RangeAttribute
    {
        /// <summary>
        /// Builds a new <see cref="MinimumAttribute"/> instance with the specified <paramref name="minimum"/> value.
        /// </summary>
        /// <param name="minimum">Specifies the minimum value for the property or parameter</param>
        public MinimumAttribute(int minimum) : base(minimum, int.MaxValue)
        {
        }

        /// <summary>
        /// Builds a new <see cref="MinimumAttribute"/> instance with the specified <paramref name="minimum"/> value.
        /// </summary>
        /// <param name="minimum">Specifies the minimum value for the property or parameter</param>
        public MinimumAttribute(double minimum) : base(minimum, double.MaxValue)
        {
        }

        /// <summary>
        /// Builds a new <see cref="MinimumAttribute"/> instance with the specified <paramref name="minimum"/> value.
        /// </summary>
        /// <param name="minimum">Specifies the minimum value for the property or parameter</param>
        public MinimumAttribute(long minimum) : base(minimum, long.MaxValue)
        {
        }

        /// <summary>
        /// Builds a new <see cref="MinimumAttribute"/> instance with the specified <paramref name="minimum"/> value.
        /// </summary>
        /// <param name="minimum">Specifies the minimum value for the property or parameter</param>
        public MinimumAttribute(float minimum) : base(minimum, float.MaxValue)
        {
        }

        /// <inheritdoc/>
        public override string FormatErrorMessage(string name) => $"{name} must be greather than or equals to {Minimum}";
    }
}
