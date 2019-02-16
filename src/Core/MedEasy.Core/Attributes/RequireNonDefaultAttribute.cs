using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using static System.AttributeTargets;
namespace MedEasy.Core.Attributes
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
        private static Type[] PrimitiveTypes =
        {
            typeof(string),

            typeof(int),
            typeof(long),
            typeof(short),
            typeof(decimal),
            typeof(float),

            typeof(DateTime), typeof(DateTime?),
            typeof(DateTimeOffset), typeof(DateTimeOffset?),
            typeof(Guid), typeof(Guid?),
            typeof(bool), typeof(bool?)
        };
        public override string FormatErrorMessage(string name) => $"{(string.IsNullOrWhiteSpace(name) ? "the field" : $"'{name}'")} must have a non default value";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult validationResult = ValidationResult.Success;
            //try
            //{
            if (value == null)
            {
                validationResult = new ValidationResult(FormatErrorMessage(validationContext?.MemberName));
            }
            else
            {
                TypeInfo typeInfo = value.GetType().GetTypeInfo();
                if (typeInfo.IsPrimitive || typeInfo.IsValueType)
                {
                    if (Equals(value, Activator.CreateInstance(value.GetType())))
                    {
                        validationResult = new ValidationResult(FormatErrorMessage(validationContext?.MemberName));
                    }
                }
                else if (typeInfo.DeclaredConstructors.Once(x => !x.GetParameters().Any()))
                {
                    IEnumerable<PropertyInfo> pis = typeInfo.GetRuntimeProperties()
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

            //}
            //catch
            //{
            //    validationResult = new ValidationResult("Unexpected error occured during validation");
            //}

            return validationResult;
        }
    }
}
