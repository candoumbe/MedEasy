using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
