using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Controller that describe 
    /// </summary>
    [Controller]
    [Route("/")]
    public class RootController 
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public RootController(IHostingEnvironment hostingEnvironment, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            IDictionary<string, string> description = new Dictionary<string, string>
            {
                [PatientsController.EndpointName] = urlHelper.Link("default", new { controller = PatientsController.EndpointName }),
                [DoctorsController.EndpointName] = urlHelper.Link("default", new { controller = DoctorsController.EndpointName }),
                [AppointmentsController.EndpointName] = urlHelper.Link("default", new { controller = AppointmentsController.EndpointName }),
                [SpecialtiesController.EndpointName] = urlHelper.Link("default", new { controller = AppointmentsController.EndpointName }),
            };

            if (_hostingEnvironment.IsDevelopment())
            {
                description.Add("documentation", urlHelper.Link("default", new { controller = "swagger", action = "ui" }));
            }

            return new OkObjectResult(description.OrderBy(x => x.Key));
        }
    }
}
