using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MedEasy.API.Filters
{
    /// <summary>
    /// Filter that check if model is valid BEFORE executing actions
    /// </summary>
    /// <remarks>
    /// This attribute sets the result to <see cref="BadRequestObjectResult"/> if model state is not valid.
    /// </remarks>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// <see cref="ActionFilterAttribute.OnActionExecuting(ActionExecutingContext)"/>
        /// </summary>
        /// <param name="context">context of the action that will be called.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
