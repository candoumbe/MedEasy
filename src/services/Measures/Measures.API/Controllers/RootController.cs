using Measures.API.Routing;
using Measures.DTO;
using MedEasy.DTO.Search;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using static MedEasy.RestObjects.FormFieldType;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Measures.API.Controllers
{
    /// <summary>
    /// Controller that describe 
    /// </summary>
    [Controller]
    [Route("/")]
    [Route("/measures")]
    public class RootController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUrlHelper _urlHelper;
        private IOptions<MeasuresApiOptions> ApiOptions { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment">Gives access to hosting environment</param>
        /// <param name="urlHelper"></param>
        /// <param name="apiOptions">Gives access to the API configuration</param>
        /// 
        public RootController(IHostingEnvironment hostingEnvironment, IUrlHelper urlHelper, IOptions<MeasuresApiOptions> apiOptions)
        {
            _hostingEnvironment = hostingEnvironment;
            _urlHelper = urlHelper;
             ApiOptions = apiOptions;
        }


        /// <summary>
        /// Describes all endpoints
        /// </summary>
        /// <remarks>
        /// 
        ///     API clients should only relies on link's relation to navigate through all resources returned by this API
        ///     
        /// 
        ///     
        /// </remarks>
        /// <response code="200"></response>
        [HttpGet]
        [HttpOptions]
        [HttpHead]
        [ProducesResponseType(typeof(IEnumerable<Endpoint>), 200)]
        public IEnumerable<Endpoint> Index()
        {
            MeasuresApiOptions apiOptions = ApiOptions.Value;
            int page = 1,
                pageSize = apiOptions.DefaultPageSize,
                maxPageSize = apiOptions.MaxPageSize;
            IList<Endpoint> endpoints = new List<Endpoint>() {
                new Endpoint
                {
                    Name = BloodPressuresController.EndpointName.ToLowerKebabCase(),
                    Link = new Link
                    {
                        Title = "Collection of blood pressures",
                        Method = "GET",
                        Relation = LinkRelation.Collection,
                        Href = _urlHelper.Link(RouteNames.DefaultGetAllApi, new {controller = BloodPressuresController.EndpointName, page, pageSize})
                    },
                    Forms = new[]
                    {
                        new FormBuilder<SearchBloodPressureInfo>()
                            .SetRelation(LinkRelation.Search)
                            .SetMethod("GET")
                            .SetHref(_urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = BloodPressuresController.EndpointName}))
                            .AddField(x => x.From)
                            .AddField(x => x.To)
                            .AddField(x => x.Page)
                            .AddField(x => x.PageSize, new FormFieldAttributes { Max = apiOptions.MaxPageSize })
                            .AddField(x => x.Sort)
                            .Build(),

                        new FormBuilder<CreateBloodPressureInfo>()
                            .SetRelation(LinkRelation.CreateForm)
                            .SetMethod("POST")
                            .SetHref(_urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = BloodPressuresController.EndpointName}))
                            .AddField(x => x.DateOfMeasure)
                            .AddField(x => x.SystolicPressure)
                            .AddField(x => x.DiastolicPressure)
                            .Build(),
                        
                        typeof(BloodPressureInfo).ToForm(new Link
                            {
                                Method = "PATCH",
                                Relation = LinkRelation.EditForm,
                                Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = BloodPressuresController.EndpointName, id = $"{{{nameof(BloodPressureInfo.Id)}}}"})
                            })
                        
                    }
                },

                new Endpoint
                {
                    Name = PatientsController.EndpointName.ToLowerKebabCase(),
                    Link = new Link
                    {
                        Title = "Collection of patients",
                        Method = "GET",
                        Relation = LinkRelation.Collection,
                        Href = _urlHelper.Link(RouteNames.DefaultGetAllApi, new {controller = PatientsController.EndpointName, page, pageSize})
                    },
                    Forms = new[]
                    {
                        new FormBuilder<SearchPatientInfo>()
                            .SetRelation(LinkRelation.Search)
                            .SetMethod("GET")
                            .SetHref(_urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = PatientsController.EndpointName, page, pageSize }))
                            .AddField(x => x.BirthDate)
                            .AddField(x => x.Firstname)
                            .AddField(x => x.Lastname)
                            .AddField(x => x.Page)
                            .AddField(x => x.PageSize, new FormFieldAttributes { Max = apiOptions.MaxPageSize })
                            .AddField(x => x.Sort)
                            .Build(),
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
                        Href = _urlHelper.Link("default", new { controller = "swagger" }),
                        Relation = "help"
                    }
                });
            }

            return endpoints.OrderBy(x => x.Name);
        }
    }
}
