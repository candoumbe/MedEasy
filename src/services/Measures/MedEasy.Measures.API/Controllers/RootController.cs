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
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MedEasy.Measures.API.Controllers
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
        private IOptions<PrescriptionApiOptions> ApiOptions { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment">Gives access to hosting environment</param>
        /// <param name="urlHelperFactory"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiOptions">Gives access to the API configuration</param>
        public RootController(IHostingEnvironment hostingEnvironment, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, IOptions<PrescriptionApiOptions> apiOptions)
        {
            _hostingEnvironment = hostingEnvironment;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            ApiOptions = apiOptions;
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
            int page = 1,
                pageSize = ApiOptions.Value.DefaultPageSize;
            IList<Endpoint> endpoints = new List<Endpoint>() {
                
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
