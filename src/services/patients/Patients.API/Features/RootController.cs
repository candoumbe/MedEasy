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
using Microsoft.AspNetCore.Routing;

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
        private readonly LinkGenerator _urlHelper;
        private readonly IOptions<PatientsApiOptions> _apiOptions;
        private readonly ApiVersion _apiVersion;

        /// <summary>
        /// Builds a new <see cref="RootController"/> instance
        /// </summary>
        /// <param name="hostingEnvironment">Gives access to hosting environment</param>
        /// <param name="urlHelper"></param>
        /// <param name="apiOptions">Gives access to the API configuration</param>
        public RootController(IHostEnvironment hostingEnvironment, LinkGenerator urlHelper, IOptions<PatientsApiOptions> apiOptions, ApiVersion apiVersion)
        {
            _hostingEnvironment = hostingEnvironment;
            _urlHelper = urlHelper;
            _apiOptions = apiOptions;
            _apiVersion = apiVersion;
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
            PatientsApiOptions apiOptions = _apiOptions.Value;
            int page = 1,
                pageSize = apiOptions.DefaultPageSize,
                maxPageSize = apiOptions.MaxPageSize;
            string version = _apiVersion.ToString();

            IList<Endpoint> endpoints = new List<Endpoint>() {
                new Endpoint
                {
                    Name = PatientsController.EndpointName.Slugify(),
                    Link = new Link
                    {
                        Title = "Collection of patients",
                        Method = "GET",
                        Relation = LinkRelation.Collection,
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new {controller = PatientsController.EndpointName, page, pageSize, version})
                    },
                    Forms = new[]
                    {
                        new Form
                        {
                            Meta = new Link
                            {
                                Method = "GET",
                                Relation = LinkRelation.Search,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new {controller = PatientsController.EndpointName, page, pageSize, version})

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
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new {controller = PatientsController.EndpointName, version})
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
                        Href = _urlHelper.GetPathByName(RouteNames.Default, new { controller = "swagger" }),
                        Relation = "help"
                    }
                });
            }

            return endpoints.OrderBy(x => x.Name);
        }
    }
}
