namespace MedEasy.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    using static System.AttributeTargets;

    /// <summary>
    /// Marks a property/parameter/class so that its value cannot be its default.
    /// </summary>
    [AttributeUsage(Parameter | Property | Class, AllowMultiple = false, Inherited = false)]
    public class RequireNonDefaultAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name) => $"{(string.IsNullOrWhiteSpace(name) ? "the field" : $"'{name}'")} must not have a default value";

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
                else if (typeInfo.DeclaredConstructors.Once(x => !x.GetParameters().Any()))
                {
                    IEnumerable<PropertyInfo> pis = type.GetRuntimeProperties()
                        .Where(pi => pi.CanRead);

                    IEnumerable<(string PropertyName, bool IsValid)> properties = pis.AsParallel()
                                                                                             .Select(pi => (pi.Name, ValidationResult.Success == IsValid(pi.GetValue(value), validationContext)))
                                                                                             .ToArray();

                    if (! properties.AtLeastOnce(pi => pi.IsValid))
                    {
                        validationResult = new ValidationResult(FormatErrorMessage(validationContext?.MemberName));
                    }
                }
            }

            return validationResult;
        }
    }
}
