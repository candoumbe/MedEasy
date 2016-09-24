using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace MedEasy.Filters
{
    public class HandleErrorAttribute : ExceptionFilterAttribute
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<HandleErrorAttribute> _logger;

        public HandleErrorAttribute(IHostingEnvironment hostingEnvironment, ILogger<HandleErrorAttribute> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }


        public override Task OnExceptionAsync(ExceptionContext context)
        {
            return base.OnExceptionAsync(context);
        }
    }
}
