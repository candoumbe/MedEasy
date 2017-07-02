using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace MedEasy.API.Results
{
    /// <summary>
    /// An <see cref="ActionResult"/> which returns a InternalServerError (500)
    /// </summary>
    public class InternalServerErrorResult : StatusCodeResult
    {
        /// <summary>
        /// Builds a new <see cref="InternalServerErrorResult"/> instance.
        /// </summary>
        public InternalServerErrorResult() : base(Status500InternalServerError)
        {

        }
    }
}
