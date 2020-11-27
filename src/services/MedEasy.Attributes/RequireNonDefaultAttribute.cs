using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using static System.AttributeTargets;

namespace MedEasy.Attributes
{
    /// <summary>
    /// Marks a property/parameter/class so that its value cannot be its default.
    /// </summary>
    /// <remarks>
    /// </remarks>

    [AttributeUsage(Parameter | Property | Class, AllowMultiple = false, Inherited = false)]
    public class RequireNonDefaultAttribute : ValidationAttribute
    {
        /// <summary>
        /// List of all types that can be directly converted to their string representation
        /// </summary>
        /// <inheritdoc/>
        public override string FormatErrorMessage(string name) => $"{(string.IsNullOrWhiteSpace(name) ? "the field" : $"'{name}'")} must have a non default value";

        /// <inheritdoc/>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult validationResult = ValidationResult.Success;

            if (value is null)
            {
                validationResult = new ValidationResult(FormatErrorMessage(validationContext?.MemberName));
            }
            else
            {
                Type type = value.GetType();
                TypeInfo typeInfo = type.GetTypeInfo();
                if (typeInfo.IsPrimitive || typeInfo.IsValueType)
                {
                    if (Equals(value, Activator.CreateInstance(value.GetType())))
                    {
                        validationResult = new ValidationResult(FormatErrorMessage(validationContext?.MemberName));
                    }
                }
                else if (typeInfo.DeclaredConstructors.Once(x => x.GetParameters().Length == 0))
                {
                    IEnumerable<PropertyInfo> pis = type.GetRuntimeProperties()
                        .Where(pi => pi.CanRead);

                    bool foundPropertyWithNonDefaultValue = false;
                    IEnumerator<PropertyInfo> enumerator = pis.GetEnumerator();

                    while (enumerator.MoveNext() && !foundPropertyWithNonDefaultValue)
                    {
                        PropertyInfo currentProp = enumerator.Current;
                        object currentVal = currentProp.GetValue(value);

                        foundPropertyWithNonDefaultValue = ValidationResult.Success == IsValid(currentVal, validationContext);
                    }

                    if (!foundPropertyWithNonDefaultValue)
                    {
                        validationResult = new ValidationResult(FormatErrorMessage(validationContext?.MemberName));
                    }
                }
            }

            return validationResult;
        }
    }
}
