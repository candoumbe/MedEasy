using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MedEasy.Controllers
{
    public class HomeController : Controller
    {
        //[ViewDataDictionary]
        //public ViewDataDictionary ViewData { get; set; }

        public HomeController(ILoggerFactory logger)
            //:base(logger)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            return new ViewResult();
        }

        [HttpGet]
        public IActionResult About()
        {
            //ViewData["Message"] = "Your application description page.";

            return new ViewResult
            {
                //ViewData = ViewData
            };
        }

        [HttpGet]
        public IActionResult Contact()
        {
            //ViewDataDictionary viewData = new ViewDataDictionary(_metadataProvider, new ModelStateDictionary());
            //ViewData["Message"] = "Your page.";

            return new ViewResult
            {
                //ViewData = ViewData
            };
        }


        public IActionResult Error() => new ViewResult();
        
    }
}
