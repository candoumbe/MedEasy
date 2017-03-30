using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.API.Filters
{
    /// <summary>
    /// Filter that check if model is valid <strong>BEFORE</strong> executing actions.
    /// </summary>
    /// <remarks>
    /// This attribute sets the result to <see cref="BadRequestObjectResult"/> if model state is not valid.
    /// </remarks>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelAttribute> _logger;

        /// <summary>
        /// Builds a new <see cref="ValidateModelAttribute"/> instance
        /// </summary>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="logger"/> is <c>null</c>.</exception>
        public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            _logger = logger;
        }


        /// <summary>
        /// <see cref="ActionFilterAttribute.OnActionExecuting(ActionExecutingContext)"/>
        /// </summary>
        /// <param name="context">context of the action that will be called.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogDebug($"{SerializeObject(context.ActionArguments)}");
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
