using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MedEasy.RestObjects;

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
            var description = new
            {
                patients = new
                {
                    links = new[] 
                    {
                        new Link
                        {
                            Href = urlHelper.Link("default", new { controller = PatientsController.EndpointName }),
                            Relation = "collection"
                        }
                    }
                },
                doctors = new
                {
                    links = new[]
                    {
                        new Link
                        {
                            Href = urlHelper.Link("default", new { controller = DoctorsController.EndpointName }),
                            Relation = "collection"
                        }
                    }
                },
                specialties = new
                {
                    links = new[]
                    {
                        new Link
                        {
                            Href = urlHelper.Link("default", new { controller = SpecialtiesController.EndpointName }),
                            Relation = "collection"
                        }
                    }
                },
                appointments = new
                {
                    links = new[]
                    {
                        new Link
                        {
                            Href = urlHelper.Link("default", new { controller = AppointmentsController.EndpointName }),
                            Relation = "collection"
                        }
                    }
                }

#if DEBUG
                ,
                swagger = new
                {
                    links = new Link
                    {
                        Href = urlHelper.Link("default", new { controller = "swagger", action = "ui" }),
                        Relation = "documentation"
                    }
                }
#endif
            };

            return new OkObjectResult(description);
        }
    }
}
