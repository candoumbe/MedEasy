using Measures.API.Routing;
using Measures.DTO;
using MedEasy.DTO.Search;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
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
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUrlHelper _urlHelper;
        private IOptions<MeasuresApiOptions> ApiOptions { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment">Gives access to hosting environment</param>
        /// <param name="urlHelper"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="apiOptions">Gives access to the API configuration</param>
        public RootController(IHostingEnvironment hostingEnvironment, IUrlHelper urlHelper, IActionContextAccessor actionContextAccessor, IOptions<MeasuresApiOptions> apiOptions)
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
                        new Form
                        {
                            Meta = new Link
                            {
                                Method = "GET",
                                Relation = LinkRelation.Search,
                                Href = _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = BloodPressuresController.EndpointName})
                            },
                            Items = new[]
                            {
                                new FormField
                                {
                                    Name = nameof(SearchBloodPressureInfo.From),
                                    Description = "Minimum date of measure",
                                    Type = FormFieldType.DateTime
                                },
                                new FormField
                                {
                                    Name = nameof(SearchBloodPressureInfo.To),
                                    Description = "Maximum date of measure",
                                    Type = FormFieldType.DateTime

                                },
                                new FormField
                                {
                                    Name = nameof(SearchBloodPressureInfo.Page),
                                    Type = Integer,
                                    Description = "Index of a page of results",
                                    Min = 1
                                },
                                new FormField
                                {
                                    Name = nameof(SearchBloodPressureInfo.PageSize),
                                    Type = Integer,
                                    Description = "Number of items per page",
                                    Min = 1,
                                    Max = ApiOptions.Value.MaxPageSize
                                },
                                new FormField
                                {
                                    Name = nameof(SearchBloodPressureInfo.Sort),
                                    Pattern = AbstractSearchInfo<BloodPressureInfo>.SortPattern,
                                    Type = FormFieldType.String
                                },

                            }
                        },
                        new Form
                        {
                            Meta = new Link
                            {
                                Method = "POST",
                                Relation = LinkRelation.CreateForm,
                                Href = _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = BloodPressuresController.EndpointName})
                            },
                            Items = new[]
                            {
                                new FormField { Name = nameof(BloodPressureInfo.DateOfMeasure), Type = FormFieldType.DateTime},
                                new FormField { Name = nameof(BloodPressureInfo.DiastolicPressure), Type = Integer, Min = 0},
                                new FormField { Name = nameof(BloodPressureInfo.SystolicPressure), Type = Integer, Min = 0},
                            }
                        },
                        typeof(BloodPressureInfo).ToForm(new Link
                            {
                                Method = "PATCH",
                                Relation = LinkRelation.EditForm,
                                Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = BloodPressuresController.EndpointName, id = $"{{{nameof(BloodPressureInfo.Id)}}}"})
                            })
                        //new Form
                        //{
                        //    Meta = new Link
                        //    {
                        //        Method = "PATCH",
                        //        Relation = LinkRelation.EditForm,
                        //        Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = BloodPressuresController.EndpointName, id = nameof(BloodPressureInfo.Id)})
                        //    },
                        //    Items = new[]
                        //    {
                        //        new FormField { Name = nameof(BloodPressureInfo.DateOfMeasure), Type = FormFieldType.DateTime},
                        //        new FormField { Name = nameof(BloodPressureInfo.DiastolicPressure), Type = Integer, Min = 0},
                        //        new FormField { Name = nameof(BloodPressureInfo.SystolicPressure), Type = Integer, Min = 0},
                        //    }
                        //}
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
