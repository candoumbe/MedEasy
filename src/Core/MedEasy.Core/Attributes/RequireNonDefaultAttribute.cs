using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MedEasy.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class RequireNonDefaultAttribute : ValidationAttribute
    {

        public override string FormatErrorMessage(string name) => $"{(string.IsNullOrWhiteSpace(name) ? "the field" : $"'{name}'")} must have a non default value";


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isValid = value != null && !Equals(value, Activator.CreateInstance(value.GetType()));
            ValidationResult validationResult;
            if (isValid)
            {
                validationResult = ValidationResult.Success;
            }
            else
            {
                string msg = FormatErrorMessage(validationContext?.MemberName);
                validationResult = new ValidationResult(msg ?? $"'{validationContext?.MemberName}' must have a non default value ");
            }

            return validationResult;
        }
    }
}
