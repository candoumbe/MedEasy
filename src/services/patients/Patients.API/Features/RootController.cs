using Patients.API.Routing;
using Patients.DTO;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using static MedEasy.RestObjects.FormFieldType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Patients.API.Controllers
{
    /// <summary>
    /// Controller that describe 
    /// </summary>
    [Controller]
    [Route("/")]
    public class RootController
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IUrlHelper _urlHelper;
        private IOptions<PatientsApiOptions> ApiOptions { get; }

        /// <summary>
        /// Builds a new <see cref="RootController"/> instance
        /// </summary>
        /// <param name="hostingEnvironment">Gives access to hosting environment</param>
        /// <param name="urlHelper"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiOptions">Gives access to the API configuration</param>
        public RootController(IHostEnvironment hostingEnvironment, IUrlHelper urlHelper, IActionContextAccessor actionContextAccessor, IOptions<PatientsApiOptions> apiOptions)
        {
            _hostingEnvironment = hostingEnvironment;
            _urlHelper = urlHelper;
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
        [HttpGet, HttpOptions]
        [ProducesResponseType(typeof(IEnumerable<Endpoint>), 200)]
        [AllowAnonymous]
        public IEnumerable<Endpoint> Index()
        {
            PatientsApiOptions apiOptions = ApiOptions.Value;
            int page = 1,
                pageSize = apiOptions.DefaultPageSize,
                maxPageSize = apiOptions.MaxPageSize;
            IList<Endpoint> endpoints = new List<Endpoint>() {
                new Endpoint
                {
                    Name = PatientsController.EndpointName.Slugify(),
                    Link = new Link
                    {
                        Title = "Collection of patients",
                        Method = "GET",
                        Relation = LinkRelation.Collection,
                        Href = _urlHelper.Link(RouteNames.DefaultGetAllApi, new {controller = PatientsController.EndpointName, page, pageSize})
                    },
                    Forms = new[]
                    {
                        new Form
                        {
                            Meta = new Link
                            {
                                Method = "GET",
                                Relation = LinkRelation.Search,
                                Href = _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new {controller = PatientsController.EndpointName, page, pageSize})

                            },
                            Items = new[]
                            {
                                new FormField { Name = nameof(SearchPatientInfo.BirthDate), Type = Date },
                                new FormField { Name = nameof(SearchPatientInfo.Firstname)},
                                new FormField { Name = nameof(SearchPatientInfo.Lastname)},
                                new FormField { Name = nameof(SearchPatientInfo.Page), Min = 1, Type = Integer},
                                new FormField { Name = nameof(SearchPatientInfo.PageSize), Min = 1, Max = maxPageSize, Type = Integer},
                                new FormField { Name = nameof(SearchPatientInfo.Sort), Pattern = SearchPatientInfo.SortPattern},
                            }
                        },

                        new FormBuilder<CreatePatientInfo>(new Link
                            {
                                Method = "POST",
                                Relation = LinkRelation.CreateForm,
                                Href = _urlHelper.Link(RouteNames.DefaultGetAllApi, new {controller = PatientsController.EndpointName})
                            })
                            .AddField(form => form.Firstname)
                            .AddField(form => form.Lastname)
                            .AddField(form => form.MainDoctorId)
                            .AddField(form => form.BirthDate)
                            .AddField(form => form.BirthPlace)
                            .Build()
                    }
                },

            };

            if (!_hostingEnvironment.IsProduction())
            {
                endpoints.Add(new Endpoint
                {
                    Name = "documentation",
                    Link = new Link
                    {
                        Href = _urlHelper.Link(RouteNames.Default, new { controller = "swagger" }),
                        Relation = "help"
                    }
                });
            }

            return endpoints.OrderBy(x => x.Name);
        }
    }
}
