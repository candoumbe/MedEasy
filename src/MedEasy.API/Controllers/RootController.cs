using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MedEasy.RestObjects;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using MedEasy.DTO.Search;
using System.Linq;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        /// <param name="urlHelperFactory"></param>
        /// <param name="actionContextAccessor"></param>
        public RootController(IHostingEnvironment hostingEnvironment, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }


        /// <summary>
        /// Describes all endpoints
        /// </summary>
        /// <remarks>
        /// 
        ///     API clients should only relies on link's relation to navigate through all resources.
        ///     
        /// 
        ///     
        /// </remarks>
        /// <response code="200"></response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Endpoint>), 200)]
        public IEnumerable<Endpoint> Index()
        {
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            IList<Endpoint> endpoints = new List<Endpoint>() {
                new Endpoint
                {
                    Name = PatientsController.EndpointName,
                    Link = new Link
                    {
                        Href = urlHelper.Link("default", new { controller = PatientsController.EndpointName }),
                        Relation = "collection"
                    },
                    Forms = new []
                    {
                        typeof(CreatePatientInfo).ToForm(new Link {
                            Href = urlHelper.Link("default", new { controller = PatientsController.EndpointName }),
                            Method = "POST",
                            Relation = "create-form"
                         }),
                        typeof(SearchPatientInfo).ToForm(new Link {
                            Href = urlHelper.Link("default", new { controller = PatientsController.EndpointName, action=nameof(PatientsController.Search) }),
                            Relation = "search"
                         })
                    }
                },
                new Endpoint
                {
                    Name = DoctorsController.EndpointName,
                    Link = new Link
                    {
                        Href = urlHelper.Link("default", new { controller = DoctorsController.EndpointName }),
                        Relation = "collection"
                    },
                    Forms = new []
                    {
                        typeof(CreateDoctorInfo).ToForm(new Link {
                            Href = urlHelper.Link("default", new { controller = DoctorsController.EndpointName }),
                            Method = "POST",
                            Relation = "create-form"
                         })
                    }
                },
                new Endpoint
                {
                    Name = SpecialtiesController.EndpointName,
                    Link = new Link
                    {
                        Href = urlHelper.Link("default", new { controller = SpecialtiesController.EndpointName }),
                        Relation = "collection"
                    },
                    Forms = new []
                    {
                        typeof(CreateSpecialtyInfo).ToForm(new Link {
                            Href = urlHelper.Link("default", new { controller = SpecialtiesController.EndpointName }),
                            Method = "POST",
                            Relation = "create-form"
                         })
                    }
                },
                new Endpoint
                {
                    Name = PrescriptionsController.EndpointName,
                    Link = new Link
                    {
                        Href = urlHelper.Link("default", new { controller = PrescriptionsController.EndpointName }),
                        Relation = "collection"
                    },
                    Forms = new []
                    {
                        typeof(CreatePrescriptionInfo).ToForm(new Link {
                            Href = urlHelper.Link("default", new { controller = PrescriptionsController.EndpointName }),
                            Method = "POST",
                            Relation = "create-form"
                         })
                    }
                },
                new Endpoint
                {
                    Name = DocumentsController.EndpointName,
                    Link = new Link
                    {
                        Href = urlHelper.Link("default", new { controller = DocumentsController.EndpointName }),
                        Relation = "collection"
                    },
                    Forms = new []
                    {
                        typeof(CreateDocumentInfo).ToForm(new Link {
                            Href = urlHelper.Link("default", new { controller = DocumentsController.EndpointName }),
                            Method = "POST",
                            Relation = "create-form"
                        })
                    }
                },
                new Endpoint
                {
                    Name = AppointmentsController.EndpointName,
                    Link = new Link
                    {
                        Href = urlHelper.Link("default", new { controller = AppointmentsController.EndpointName }),
                        Relation = "collection"
                    },
                    Forms = new []
                    {
                        typeof(CreateAppointmentInfo).ToForm(new Link {
                            Href = urlHelper.Link("default", new { controller = AppointmentsController.EndpointName }),
                            Method = "POST",
                            Relation = "create-form"
                        })
                    }
                }
            };

            if (!_hostingEnvironment.IsProduction())
            {
                endpoints.Add(new Endpoint
                {
                    Name = "documentation",
                    Link = new Link
                    {
                        Href = urlHelper.Link("default", new { controller = "swagger" }),
                        Relation = "documentation"
                    }
                });
            }

            return endpoints.OrderBy(x => x.Name);
        }
    }
}
