using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static Microsoft.AspNetCore.Http.StatusCodes;
namespace MedEasy.Core.Filters
{
    /// <summary>
    /// Attribute to validates model
    /// </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState)
                {
                    StatusCode = Status406NotAcceptable
                };
            }
        }
    }

}
