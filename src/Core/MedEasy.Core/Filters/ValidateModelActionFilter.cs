using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using static Microsoft.AspNetCore.Http.HttpMethods;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace MedEasy.Core.Filters
{
    /// <summary>
    /// Filter to validate model
    /// </summary>
    public class ValidateModelActionFilter : ActionFilterAttribute
    {
        public ValidateModelActionFilter() => Order = 1;

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
                    Attribute attributeInstance = CustomAttributeExtensions.GetCustomAttribute(parameter, attributeData.AttributeType);

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
#if NETCOREAPP2_1
                ValidationProblemDetails validationProblemDetails = new ValidationProblemDetails
                {
                    Title = "Validation failed",
                    Status = Status400BadRequest
                };
#else
                ErrorObject errorObject = new ErrorObject

                {
                    Code = "BAD_REQUEST",
                    Description = "Validation failed",
                };
#endif

                IDictionary<string, IEnumerable<string>> errors = context.ModelState
                    .Where(element => !string.IsNullOrWhiteSpace(element.Key))
                    .ToDictionary(item => item.Key, item => item.Value.Errors.Select(x => x.ErrorMessage));

                if (errors.Count > 0)
                {
#if NETCOREAPP2_0
                    errorObject.Errors = errors;
#else 
                    foreach (KeyValuePair<string, IEnumerable<string>> item in errors)
                    {
                        validationProblemDetails.Errors.Add(item.Key, item.Value.ToArray());
                    }
#endif

                }
#if NETCOREAPP2_0
                BadRequestObjectResult result = new BadRequestObjectResult(errorObject); 
#else
                BadRequestObjectResult result = new BadRequestObjectResult(validationProblemDetails);
#endif
                if (IsPost(context.HttpContext.Request.Method))
                {

                    result.StatusCode = Status422UnprocessableEntity;
                }

                context.Result = result;
            }
        }
    }

}
