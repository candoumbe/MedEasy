using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace MedEasy.Core.Results
{
    public class PartialObjectResult : OkObjectResult
    {
        public PartialObjectResult(object value) : base(value)
        {
            StatusCode = Status206PartialContent;
        }
    }
}
