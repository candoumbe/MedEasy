using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using static Microsoft.AspNetCore.Http.HttpMethods;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc;

namespace MedEasy.Core.Filters
{
    /// <summary>
    /// Filter to validate model
    /// </summary>
    public class ValidateModelActionFilterAttribute : ActionFilterAttribute
    {
        public ValidateModelActionFilterAttribute() => Order = 1;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IEnumerable<ParameterInfo> parameters = (context.ActionDescriptor as ControllerActionDescriptor)
                ?.MethodInfo?.GetParameters() ?? Enumerable.Empty<ParameterInfo>();

            foreach (ParameterInfo parameter in parameters)
            {
                context.ActionArguments.TryGetValue(parameter.Name, out object value);
                IEnumerable<CustomAttributeData> validationAttributes = parameter.CustomAttributes
                    .Where(attr => typeof(ValidationAttribute).IsAssignableFrom(attr.AttributeType))
#if DEBUG
                    .ToArray()
#endif
                    ;
                foreach (CustomAttributeData attributeData in validationAttributes)
                {
                    Attribute attributeInstance = parameter.GetCustomAttribute(attributeData.AttributeType);

                    if (attributeInstance is ValidationAttribute validationAttribute)
                    {
                        bool isValid = validationAttribute.IsValid(value);
                        if (!isValid)
                        {
                            context.ModelState.AddModelError(parameter.Name, validationAttribute.FormatErrorMessage(parameter.Name));
                        }
                    }
                }
            }

            if (!context.ModelState.IsValid || context.ModelState.ErrorCount > 0)
            {
                ValidationProblemDetails validationProblemDetails = new()
                {
                    Title = "Validation failed",
                    Status = Status400BadRequest
                };

                IDictionary<string, IEnumerable<string>> errors = context.ModelState
                    .Where(element => !string.IsNullOrWhiteSpace(element.Key))
                    .ToDictionary(item => item.Key, item => item.Value.Errors.Select(x => x.ErrorMessage).Distinct());

                if (errors.Count > 0)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> item in errors)
                    {
                        validationProblemDetails.Errors.Add(item.Key, item.Value.ToArray());
                    }
                }

                BadRequestObjectResult result = new(validationProblemDetails);

                if (IsPost(context.HttpContext.Request.Method))
                {
                    result.StatusCode = Status422UnprocessableEntity;
                }

                context.Result = result;
            }
        }
    }
}
