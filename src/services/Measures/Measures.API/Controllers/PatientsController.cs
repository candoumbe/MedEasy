using AutoMapper.QueryableExtensions;
using Measures.API.Routing;
using Measures.DTO;
using Measures.Objects;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO.Search;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Measures.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="PatientInfo"/> resources
    /// </summary>
    [Route("measures/[controller]")]
    public class PatientsController : AbstractBaseController<Patient, PatientInfo, Guid>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(PatientsController).Replace("Controller", string.Empty);

        /// <summary>
        /// Name of the ressource
        /// </summary>
        protected override string ControllerName => EndpointName;

        /// <summary>
        /// Options of the API
        /// </summary>
        public IOptionsSnapshot<MeasuresApiOptions> ApiOptions { get; }




        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance
        /// </summary>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="expressionBuilder">Builds <see cref="Expression{TDelegate}"/></param>
        /// <param name="unitOfWorkFactory">Factory to build <see cref="IUnitOfWork"/> instance.</param>
        /// <param name="logger">logger</param>
        /// <param name="urlHelper">Helper class to build URL strings.</param>
        public PatientsController(ILogger<PatientsController> logger, IUrlHelper urlHelper,
            IOptionsSnapshot<MeasuresApiOptions> apiOptions,
            IExpressionBuilder expressionBuilder,
            IUnitOfWorkFactory unitOfWorkFactory) : base(logger, unitOfWorkFactory, expressionBuilder, urlHelper)
        {
            ApiOptions = apiOptions;
        }



        /// <summary>
        /// Gets all the resources of the endpoint
        /// </summary>
        /// <param name="pagination">index of the page of resources to get</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of the request</param>
        /// <remarks>
        /// Resources are returned as pages. The <paramref name="pagination"/>'s value is used has a hint by the server
        /// and there's no garanty that the size of page of result will be equal to the <paramref name="pagination"/>' property <see cref="PaginationConfiguration.PageSize"/> set in the query.
        /// In particular, the number of resources on a page may be caped by the server.
        /// </remarks>
        /// <response code="200">items of the page</response>
        /// <response code="400"><paramref name="pagination"/><see cref="PaginationConfiguration.Page"/> or <paramref name="pagination"/><see cref="PaginationConfiguration.PageSize"/> is negative or zero</response>
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<PatientInfo>>), 200)]
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration pagination, CancellationToken cancellationToken = default)
        {
            using (IUnitOfWork uow = UowFactory.New())
            {
                pagination.PageSize = Math.Min(pagination.PageSize, ApiOptions.Value.MaxPageSize);
                Expression<Func<Patient, PatientInfo>> selector = ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
                Page<PatientInfo> result = await uow.Repository<Patient>()
                    .ReadPageAsync(
                        selector,
                        pagination.PageSize,
                        pagination.Page,
                        new[] { OrderClause<PatientInfo>.Create(x => x.UpdatedDate) },
                        cancellationToken);

                int count = result.Entries.Count();
                bool hasPreviousPage = count > 0 && pagination.Page > 1;

                string firstPageUrl = UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = 1 });
                string previousPageUrl = hasPreviousPage
                        ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page - 1 })
                        : null;

                string nextPageUrl = pagination.Page < result.Count
                        ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page + 1 })
                        : null;
                string lastPageUrl = result.Count > 0
                        ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = result.Count })
                        : firstPageUrl;


                IEnumerable<BrowsableResource<PatientInfo>> resources = result.Entries
                    .Select(x => new BrowsableResource<PatientInfo>
                    {
                        Resource = x,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = LinkRelation.Self,
                                Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, x.Id})
                            }
                        }
                    });

                IGenericPagedGetResponse<BrowsableResource<PatientInfo>> response = new GenericPagedGetResponse<BrowsableResource<PatientInfo>>(
                    resources,
                    firstPageUrl,
                    previousPageUrl,
                    nextPageUrl,
                    lastPageUrl,
                    result.Total);


                return new OkObjectResult(response);
            }
        }



        /// <summary>
        /// Gets the <see cref="PatientInfo"/> resource by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to look for</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">The resource was found</response>
        /// <response code="404">Resource not found</response>
        /// <response code="400"><paramref name="id"/> is not a valid <see cref="Guid"/></response>
        [HttpHead("{id}")]
        [HttpOptions("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BrowsableResource<PatientInfo>), 200)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default)
        {
            IActionResult actionResult;

            if (id == default)
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                using (IUnitOfWork uow = UowFactory.New())
                {
                    Expression<Func<Patient, PatientInfo>> selector = ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
                    Option<PatientInfo> result = await uow.Repository<Patient>()
                        .SingleOrDefaultAsync(selector, x => x.Id == id);

                    actionResult = result.Match<IActionResult>(
                        some: resource =>
                        {
                            BrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
                            {
                                Resource = resource,
                                Links = new[]
                                {
                                    new Link
                                    {
                                        Relation = LinkRelation.Self,
                                        Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id }),
                                        Method = "GET"
                                    },
                                    new Link
                                    {
                                        Relation = "delete",
                                        Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id }),
                                        Method = "DELETE"
                                    },
                                    new Link
                                    {
                                        Relation = BloodPressuresController.EndpointName.ToLowerKebabCase(),
                                        Method = "GET",
                                        Href =  UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = BloodPressuresController.EndpointName, patientId = resource.Id })
                                    }
                                }
                            };
                            return new OkObjectResult(browsableResource);
                        },
                        none: () => new NotFoundResult()
                    );
                }
            }

            return actionResult;

        }

        /// <summary>
        /// Search doctors resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of the request.</param>
        /// <remarks>
        /// All criteria are combined as a AND.
        /// 
        /// Advanded search :
        /// Several operators that can be used to make an advanced search :
        /// '*' : match zero or more characters in a string property.
        /// 
        ///     // GET api/Doctors/Search?Firstname=Bruce
        ///     will match all resources which have exactly 'Bruce' in the Firstname property
        ///     
        ///     // GET api/Doctors/Search?Firstname=B*e
        ///     will match match all resources which starts with 'B' and ends with 'e'.
        /// 
        /// '?' : match exactly one charcter in a string property.
        /// 
        /// '!' : negate a criteria
        /// 
        ///     // GET api/Doctors/Search?Firstname=!Bruce
        ///     will match all resources where Firstname is not "Bruce"
        ///     
        /// </remarks>
        /// <response code="200">Array of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="400">one the search criteria is not valid</response>
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [HttpOptions("[action]")]
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<PatientInfo>>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        public async Task<IActionResult> Search([FromQuery]SearchPatientInfo search, CancellationToken cancellationToken = default)
        {

            IList<IDataFilter> filters = new List<IDataFilter>();
            if (!string.IsNullOrEmpty(search.Firstname))
            {
                filters.Add($"{(nameof(search.Firstname))}={search.Firstname}".ToFilter<PatientInfo>());
            }

            if (!string.IsNullOrEmpty(search.Lastname))
            {
                filters.Add($"{(nameof(search.Lastname))}={search.Lastname}".ToFilter<PatientInfo>());
            }


            SearchQueryInfo<PatientInfo> searchQuery = new SearchQueryInfo<PatientInfo>
            {
                Filter = filters.Count() == 1
                    ? filters.Single()
                    : new DataCompositeFilter { Logic = DataFilterLogic.And, Filters = filters },
                Page = search.Page,
                PageSize = search.PageSize,
                Sorts = (search.Sort ?? $"-{nameof(PatientInfo.UpdatedDate)}").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x =>
                        {
                            x = x.Trim();
                            Sort sort;
                            if (x.StartsWith("-"))
                            {
                                x = x.Substring(1);
                                sort = new Sort { Direction = MedEasy.Data.SortDirection.Descending, Expression = x.ToLambda<PatientInfo>() };
                            }
                            else
                            {
                                sort = new Sort { Direction = MedEasy.Data.SortDirection.Ascending, Expression = x.ToLambda<PatientInfo>() };
                            }

                            return sort;
                        })
            };
            Page<PatientInfo> resources = await Search(searchQuery, cancellationToken);

            GenericPagedGetResponse<BrowsableResource<PatientInfo>> page = new GenericPagedGetResponse<BrowsableResource<PatientInfo>>(
                    items: resources.Entries.Select(x => new BrowsableResource<PatientInfo>
                    {
                        Resource = x,
                        Links = new[] {
                            new Link
                            {
                                Method = "GET",
                                Relation = LinkRelation.Self,
                                Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = ControllerName, x.Id })
                            }
                        }
                    }),
                    count: resources.Total,
                    first: UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
                    {
                        controller = ControllerName,
                        search.Firstname,
                        search.Lastname,
                        search.BirthDate,
                        search.Sort,
                        page = 1,
                        search.PageSize
                    }),
                    previous: search.Page > 1
                        ? UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
                        {
                            controller = ControllerName,
                            search.Firstname,
                            search.Lastname,
                            search.BirthDate,
                            search.Sort,
                            page = search.Page - 1,
                            search.PageSize
                        })
                        : null,
                    next: resources.Count > search.Page
                        ? UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
                        {
                            controller = ControllerName,
                            search.Firstname,
                            search.Lastname,
                            search.BirthDate,
                            search.Sort,
                            page = search.Page + 1,
                            search.PageSize
                        })
                        : null,
                    last: UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
                    {
                        controller = ControllerName,
                        search.Firstname,
                        search.Lastname,
                        search.BirthDate,
                        search.Sort,
                        page = Math.Max(resources.Count, 1),
                        search.PageSize
                    })
                );


            return new OkObjectResult(page);

        }


        /// <summary>
        /// Get the last <see cref="TemperatureInfo"/> measures.
        /// </summary>
        /// <remarks>
        /// Results are ordered by <see cref="PhysiologicalMeasurement.DateOfMeasure"/> descending.
        /// </remarks>
        /// <param name="id">id of the patient to get most recent measures from</param>
        /// <param name="count">Number of result to get at most</param>
        /// <param name="cancellationToken">Notifies to cancel execution of the request.</param>
        /// <returns>Array of <see cref="TemperatureInfo"/></returns>
        [HttpGet("{id}/most-recent-temperatures")]
        [HttpHead("{id}/most-recent-temperatures")]
        [HttpOptions("{id}/most-recent-temperatures")]
        [ProducesResponseType(typeof(IEnumerable<BrowsableResource<TemperatureInfo>>), 200)]
        public async Task<IActionResult> MostRecentTemperatures(Guid id, int? count, CancellationToken cancellationToken = default)
        {
            IActionResult actionResult;


            if (id == default || (count.HasValue && count.Value <= 0))
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                using (IUnitOfWork uow = UowFactory.New())
                {
                    Expression<Func<Temperature, TemperatureInfo>> selector = ExpressionBuilder.GetMapExpression<Temperature, TemperatureInfo>();
                    Option<IEnumerable<TemperatureInfo>> measures =
                        await uow.Repository<Patient>().AnyAsync(x => x.UUID == id, cancellationToken).ConfigureAwait(false)
                        ? Option.Some(await uow.Repository<Temperature>()
                        .WhereAsync(
                            selector,
                            predicate: (TemperatureInfo x) => x.PatientId == id,
                            orderBy: new[]
                            {
                            OrderClause<TemperatureInfo>.Create(att => att.DateOfMeasure, MedEasy.DAL.Repositories.SortDirection.Descending)
                            },
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false))
                        : default;

                    actionResult = measures.Match<IActionResult>(
                        some: (temperatures) =>
                       {

                           IEnumerable<BrowsableResource<TemperatureInfo>> resources = temperatures.Select(
                               x => new BrowsableResource<TemperatureInfo>
                               {
                                   Resource = x,
                                   Links = new[]
                                   {
                                        new Link{ Method = "GET", Relation = LinkRelation.Self, Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { x.Id })},
                                        new Link{ Method = "GET", Relation = "patient", Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { x.PatientId })}
                                   }
                               });


                           return new OkObjectResult(resources);
                       },
                        none: () => new NotFoundResult()
                        );
                }
            }

            return actionResult;
        }


        ///// <summary>
        ///// Create a new <see cref="TemperatureInfo"/> resource.
        ///// </summary>
        ///// <param name="id">id of the patient the new measure will be attached to</param>
        ///// <param name="newTemperature">input to create the new resource</param>
        ///// <see cref="IPhysiologicalMeasureService.AddNewMeasureAsync{TPhysiologicalMeasure, TPhysiologicalMeasureInfo}(ICommand{Guid, CreatePhysiologicalMeasureInfo{TPhysiologicalMeasure}, TPhysiologicalMeasureInfo}, CancellationToken)"/>
        ///// <response code="201">if the creation succeed</response>
        ///// <response code="400"><paramref name="newTemperature"/> is not valid or <paramref name="id"/> is negoative or zero</response>.
        ///// <response code="404">patient not found.</response>.
        //[HttpPost("{id}/[action]")]
        //[ProducesResponseType(typeof(TemperatureInfo), 201)]
        //[ProducesResponseType(typeof(ModelStateDictionary), 400)]
        //public async Task<IActionResult> Temperatures(Guid id, [FromBody] CreateTemperatureInfo newTemperature)
        //{
        //    CreatePhysiologicalMeasureInfo<Temperature> input = new CreatePhysiologicalMeasureInfo<Temperature>
        //    {
        //        PatientId = id,
        //        Measure = new Temperature
        //        {
        //            DateOfMeasure = newTemperature.DateOfMeasure,
        //            Value = newTemperature.Value
        //        }
        //    };

        //    Option<TemperatureInfo, CommandException> output = await _physiologicalMeasureService
        //        .AddNewMeasureAsync(new AddNewPhysiologicalMeasureCommand<Temperature, TemperatureInfo>(input));

        //    return output.Match(
        //        some: temperature => new CreatedAtActionResult(nameof(Temperatures), EndpointName, new { id = temperature.PatientId, temperatureId = temperature.Id }, temperature),
        //        none: exception =>
        //        {
        //            IActionResult actionResult;
        //            switch (exception)
        //            {
        //                case CommandEntityNotFoundException cenf:
        //                    actionResult = new NotFoundResult();
        //                    break;
        //                default:
        //                    actionResult = new InternalServerErrorResult();
        //                    break;
        //            }

        //            return actionResult;

        //        });
        //}

        ///// <summary>
        ///// Create a new <see cref="PatientInfo"/> resource
        ///// </summary>
        ///// <remarks>
        ///// <see cref="CreatePatientInfo.SystolicPressure"/> and <see cref="CreatePatientInfo.DiastolicPressure"/> values must be expressed in 
        /////  millimeters of mercury (mmHg)
        ///// </remarks>
        ///// <param name="id">id of the patient the new blood pressure will be attached to</param>
        ///// <param name="newPatient">input to create the new resource</param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <response code="201">the resource creation succeed</response>
        ///// <response code="400"><paramref name="newPatient"/> is not valid or <paramref name="id"/> is negative or zero</response>
        //[HttpPost("{id}/[action]")]
        //[ProducesResponseType(typeof(BrowsableResource<PatientInfo>), 200)]
        //public async Task<IActionResult> Patients(Guid id, [FromBody] CreatePatientInfo newPatient, CancellationToken cancellationToken = default)
        //{
        //    CreatePhysiologicalMeasureInfo<Patient> info = new CreatePhysiologicalMeasureInfo<Patient>
        //    {
        //        PatientId = id,
        //        Measure = new Patient
        //        {
        //            DateOfMeasure = newPatient.DateOfMeasure,
        //            SystolicPressure = newPatient.SystolicPressure,
        //            DiastolicPressure = newPatient.DiastolicPressure
        //        }
        //    };
        //    Option<PatientInfo, CommandException> output = await _physiologicalMeasureService.AddNewMeasureAsync(new AddNewPhysiologicalMeasureCommand<Patient, PatientInfo>(info));

        //    return output.Match(
        //        some: bp => new CreatedAtActionResult(nameof(Patients), EndpointName, new { id = bp.PatientId, bloodPressureId = bp.Id }, bp),
        //        none: exception =>
        //        {
        //            IActionResult actionResult;
        //            switch (exception)
        //            {
        //                case CommandEntityNotFoundException cenf:
        //                    actionResult = new NotFoundResult();
        //                    break;
        //                default:
        //                    actionResult = new InternalServerErrorResult();
        //                    break;
        //            }


        //            return actionResult;

        //        });
        //}

        ///// <summary>
        ///// Gets one mesure of temperature for the specified patient
        ///// </summary>
        ///// <param name="id">Id of the <see cref="PatientInfo"/>.</param>
        ///// <param name="temperatureId">id of the <see cref="TemperatureInfo"/> to get</param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <returns></returns>
        //[HttpGet("{id}/[action]/{temperatureId}")]
        //[HttpHead("{id}/[action]/{temperatureId}")]
        //[ProducesResponseType(typeof(BrowsableResource<TemperatureInfo>), 200)]
        //[ProducesResponseType(typeof(ModelStateDictionary), 400)]
        //public async Task<IActionResult> Temperatures(Guid id, Guid temperatureId, CancellationToken cancellationToken = default)
        //{
        //    Option<TemperatureInfo> output = await _physiologicalMeasureService.GetOneMeasureAsync<Temperature, TemperatureInfo>(new WantOnePhysiologicalMeasureQuery<TemperatureInfo>(id, temperatureId), cancellationToken);

        //    return output.Match<IActionResult>(
        //        some: x =>
        //        {
        //            return new OkObjectResult(new BrowsableResource<TemperatureInfo>
        //            {
        //                Resource = x,
        //                Links = new[]
        //                {
        //                    new Link{ Href = UrlHelper.Action(nameof(Get), EndpointName, new { id } ), Method = "GET", Relation = "patient" },
        //                    new Link
        //                    {
        //                        Href = UrlHelper.Action(nameof(Temperatures), EndpointName, new { id = x.PatientId, temperatureId = x.Id }),
        //                        Relation = "self",
        //                        Method = "GET"
        //                    },
        //                    new Link { Href = UrlHelper.Action(nameof(Temperatures), EndpointName, new { x.Id }), Relation = "remove", Method = "DELETE" },
        //                    new Link { Href = UrlHelper.Action(nameof(Temperatures), EndpointName, new { x.Id }), Relation = "direct-link", Method = "GET" }
        //                }
        //            });
        //        },

        //        none: () => new NotFoundResult());
        //}

        ///// <summary>
        ///// Gets one mesure of temperature for the specified patient
        ///// </summary>
        ///// <param name="id">Id of the <see cref="PatientInfo"/>.</param>
        ///// <param name="bloodPressureId">id of the <see cref="PatientInfo"/> to get</param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <returns></returns>
        ///// <reponse code="404">Patient not found</reponse>
        //[HttpGet("{id}/[action]/{bloodPressureId}")]
        //[HttpHead("{id}/[action]/{bloodPressureId}")]
        //[ProducesResponseType(typeof(BrowsableResource<PatientInfo>), 200)]
        //public async Task<IActionResult> Patients(Guid id, Guid bloodPressureId, CancellationToken cancellationToken = default)
        //{
        //    Option<PatientInfo> output = await _physiologicalMeasureService
        //        .GetOneMeasureAsync<Patient, PatientInfo>(new WantOnePhysiologicalMeasureQuery<PatientInfo>(id, bloodPressureId), cancellationToken);

        //    return output.Match<IActionResult>(
        //        some: x => new OkObjectResult(new BrowsableResource<PatientInfo>
        //        {
        //            Resource = x,
        //            Links = new[]
        //                {
        //                    new Link
        //                    {
        //                        Href = UrlHelper.Action(nameof(Patients), EndpointName, new { id = x.PatientId, temperatureId = x.Id }),
        //                        Relation = "self"
        //                    }
        //                }
        //        }),
        //        none: () => new NotFoundResult());
        //}


        ///// <summary>
        ///// Get the last <see cref="TemperatureInfo"/> measures.
        ///// </summary>
        ///// <remarks>
        ///// Results are ordered by <see cref="PhysiologicalMeasurement.DateOfMeasure"/> descending.
        ///// </remarks>
        ///// <param name="id">id of the patient to get most recent measures from</param>
        ///// <param name="count">Number of result to get at most</param>
        ///// <returns>Array of <see cref="TemperatureInfo"/></returns>
        //[HttpGet("{id}/[action]")]
        //[HttpHead("{id}/[action]")]
        //[ProducesResponseType(typeof(IEnumerable<BrowsableResource<TemperatureInfo>>), 200)]
        //public async Task<IActionResult> MostRecentTemperatures(Guid id, [FromQuery]int? count)
        //{
        //    Option<IEnumerable<TemperatureInfo>> output = await MostRecentMeasureAsync<Temperature, TemperatureInfo>(new GetMostRecentPhysiologicalMeasuresInfo { PatientId = id, Count = count });

        //    return output.Match<IActionResult>(
        //        some: items => new OkObjectResult(
        //            items.Select(x => new BrowsableResource<TemperatureInfo>
        //            {
        //                Resource = x,
        //                Links = new[]
        //                    {
        //                        new Link
        //                        {
        //                            Href = UrlHelper.Action(nameof(TemperatureInfo), EndpointName, new { id = x.PatientId, temperatureId = x.Id }),
        //                            Relation = "self"
        //                        }
        //                    }
        //            })
        //            .AsEnumerable()),
        //        none: () => new NotFoundResult());
        //}



        ///// <summary>
        ///// Search patients resource based on some criteria.
        ///// </summary>
        ///// <param name="search">Search criteria</param>
        ///// <param name="cancellationToken">Notfies to cancel the search operation</param>
        ///// <remarks>
        ///// All criteria are combined as a AND.
        ///// 
        ///// Advanded search :
        ///// Several operators that can be used to make an advanced search :
        ///// '*' : match zero or more characters in a string property.
        ///// 
        /////     // GET api/Patients/Search?Firstname=Bruce
        /////     will match all resources which have exactly 'Bruce' in the Firstname property
        /////     
        /////     // GET api/Patients/Search?Firstname=B*e
        /////     will match match all resources which starts with 'B' and ends with 'e'.
        ///// 
        ///// '?' : match exactly one charcter in a string property.
        ///// 
        ///// '!' : negate a criteria
        ///// 
        /////     // GET api/Patients/Search?Firstname=!Bruce
        /////     will match all resources where Firstname is not "Bruce"
        /////     
        ///// </remarks>
        ///// <response code="200">Array of resources that matches <paramref name="search"/> criteria.</response>
        ///// <response code="400">one the search criteria is not valid</response>
        //[HttpGet("[action]")]
        //[ProducesResponseType(typeof(IEnumerable<BrowsableResource<PatientInfo>>), 200)]
        //[ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        //public async Task<IActionResult> Search([FromQuery] SearchPatientInfo search, CancellationToken cancellationToken = default)
        //{


        //    IList<IDataFilter> filters = new List<IDataFilter>();
        //    if (!string.IsNullOrWhiteSpace(search.Firstname))
        //    {
        //        filters.Add($"{nameof(PatientInfo.Firstname)}={search.Firstname}".ToFilter<PatientInfo>());
        //    }

        //    if (!string.IsNullOrWhiteSpace(search.Lastname))
        //    {
        //        filters.Add($"{nameof(PatientInfo.Lastname)}={search.Lastname}".ToFilter<PatientInfo>());
        //    }

        //    if (search.BirthDate.HasValue)
        //    {

        //        filters.Add(new DataFilter(field : nameof(PatientInfo.BirthDate), @operator : DataFilterOperator.EqualTo, value : search.BirthDate.Value));
        //    }

        //    SearchQueryInfo<PatientInfo> searchQueryInfo = new SearchQueryInfo<PatientInfo>
        //    {
        //        Page = search.Page,
        //        PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize),
        //        Filter = filters.Count() == 1
        //            ? filters.Single()
        //            : new DataCompositeFilter { Logic = And, Filters = filters },
        //        Sorts = (search.Sort ?? $"-{nameof(PatientInfo.UpdatedDate)}").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
        //                .Select(x =>
        //                {
        //                    x = x.Trim();
        //                    Sort sort;
        //                    if (x.StartsWith("-"))
        //                    {
        //                        x = x.Substring(1);
        //                        sort = new Sort { Direction = Data.SortDirection.Descending, Expression = x.ToLambda<PatientInfo>() };
        //                    }
        //                    else
        //                    {
        //                        sort = new Sort { Direction = Data.SortDirection.Ascending, Expression = x.ToLambda<PatientInfo>() };
        //                    }

        //                    return sort;
        //                })
        //    };


        //    Page<PatientInfo> pageOfResult = await _iHandleSearchQuery.Search<Patient, PatientInfo>(new SearchQuery<PatientInfo>(searchQueryInfo), cancellationToken);

        //    search.PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize);
        //    int count = pageOfResult.Entries.Count();
        //    bool hasPreviousPage = count > 0 && search.Page > 1;

        //    string firstPageUrl = UrlHelper.Action(nameof(Search), EndpointName, new { search.Firstname, search.Lastname, search.BirthDate, Page = 1, search.PageSize, search.Sort });
        //    string previousPageUrl = hasPreviousPage
        //            ? UrlHelper.Action(nameof(Search), EndpointName, new { search.Firstname, search.Lastname, search.BirthDate, Page = search.Page - 1, search.PageSize, search.Sort })
        //            : null;

        //    string nextPageUrl = search.Page < pageOfResult.PageCount
        //            ? UrlHelper.Action(nameof(Search), EndpointName, new { search.Firstname, search.Lastname, search.BirthDate, Page = search.Page + 1, search.PageSize, search.Sort })
        //            : null;
        //    string lastPageUrl = pageOfResult.PageCount > 1
        //            ? UrlHelper.Action(nameof(Search), EndpointName, new { search.Firstname, search.Lastname, search.BirthDate, Page = pageOfResult.PageCount, search.PageSize, search.Sort })
        //            : null;

        //    IEnumerable<BrowsableResource<PatientInfo>> resources = pageOfResult.Entries
        //        .Select(x => new BrowsableResource<PatientInfo> { Resource = x, Links = BuildAdditionalLinksForResource(x) });

        //    IGenericPagedGetResponse<BrowsableResource<PatientInfo>> reponse = new GenericPagedGetResponse<BrowsableResource<PatientInfo>>(
        //        resources,
        //        first: firstPageUrl,
        //        previous: previousPageUrl,
        //        next: nextPageUrl,
        //        last: lastPageUrl,
        //        count: pageOfResult.Total);

        //    return new OkObjectResult(reponse);

        //}


        ///// <summary>
        ///// Partially update a patient resource.
        ///// </summary>
        ///// <remarks>
        ///// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        ///// Only the declared modifications will be applied to the resource.
        /////
        /////     // PATCH api/Patients/3594c436-8595-444d-9e6b-2686c4904725
        /////     
        /////     [
        /////         {
        /////             "op": "update",
        /////             "path": "/MainDoctorId",
        /////             "from": "string",
        /////             "value": "e1aa24f4-69a8-4d3a-aca9-ec15c6910dc9"
        /////       }
        /////     ]
        ///// 
        ///// The set of changes to apply will be applied atomically. 
        ///// 
        ///// </remarks>
        ///// <param name="id">id of the resource to update.</param>
        ///// <param name="changes">set of changes to apply to the resource.</param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <response code="200">The resource was successfully patched.</response>
        ///// <response code="400">Changes are not valid for the selected resource.</response>
        ///// <response code="404">Resource to "PATCH" not found</response>
        //[HttpPatch("{id}")]
        //[ProducesResponseType(typeof(IEnumerable<ValidationFailure>), 400)]
        //public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<PatientInfo> changes, CancellationToken cancellationToken = default)
        //{
        //    PatchInfo<Guid, Patient> data = new PatchInfo<Guid, Patient>
        //    {
        //        Id = id,
        //        PatchDocument = _mapper.Map<JsonPatchDocument<Patient>>(changes)
        //    };
        //    await _iRunPatchPatientCommmand.RunAsync(new PatchCommand<Guid, Patient>(data), cancellationToken);

        //    return new NoContentResult();
        //}

        ///// <summary>
        ///// Gets one patient's <see cref="BodyWeightInfo"/>.
        ///// </summary>
        ///// <param name="id">patient id</param>
        ///// <param name="bodyWeightId">id of the <see cref="BodyWeightInfo"/> resource to get</param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <response code="200">the resource was found</response>
        ///// <response code="400">either <paramref name="id"/> or <paramref name="bodyWeightId"/> is negative or zero</response>
        ///// <response code="404"><paramref name="id"/> does not identify a <see cref="PatientInfo"/> resource or <paramref name="bodyWeightId"/></response> 
        //[HttpGet("{id}/[action]/{bodyWeightId}")]
        //[HttpHead("{id}/[action]/{bodyWeightId}")]
        //[ProducesResponseType(typeof(BrowsableResource<BodyWeightInfo>), 200)]
        //public async Task<IActionResult> BodyWeights(Guid id, Guid bodyWeightId, CancellationToken cancellationToken = default)
        //{
        //    Option<BodyWeightInfo> output = await _physiologicalMeasureService.GetOneMeasureAsync<BodyWeight, BodyWeightInfo>(new WantOnePhysiologicalMeasureQuery<BodyWeightInfo>(id, bodyWeightId));

        //    return output.Match<IActionResult>(
        //        some: x => new OkObjectResult(new BrowsableResource<BodyWeightInfo>
        //        {
        //            Resource = x,
        //            Links = new[]
        //                   {
        //                       new Link
        //                       {
        //                           Href = UrlHelper.Action(nameof(BodyWeights), EndpointName, new { id = x.PatientId, bodyWeightId = x.Id }),
        //                           Relation = "self"
        //                       }
        //                   }
        //        }),
        //        none: () => new NotFoundResult());
        //}


        ///// <summary>
        ///// Delete the specified blood pressure resource
        ///// </summary>
        ///// <param name="input"></param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <response code="200">the operation succeed</response>
        ///// <response code="400">if the operation is not allowed</response>
        //[HttpDelete("{id}/[action]/{measureId}")]
        //public async Task<IActionResult> Patients(DeletePhysiologicalMeasureInfo input, CancellationToken cancellationToken = default)
        //{
        //    await DeleteOneMeasureAsync<Patient>(input, cancellationToken);
        //    return new NoContentResult();
        //}

        ///// <summary>
        ///// Delete the specified blood pressure resource
        ///// </summary>
        ///// <param name="input"></param>
        ///// <response code="200">if the operation succeed</response>
        ///// <response code="400">if the operation is not allowed</response>
        //[HttpDelete("{id}/[action]/{measureId}")]
        //public async Task<IActionResult> Temperatures(DeletePhysiologicalMeasureInfo input)
        //{
        //    await DeleteOneMeasureAsync<Temperature>(input);
        //    return new NoContentResult();
        //}

        ///// <summary>
        ///// Delete the specified body weight resource
        ///// </summary>
        ///// <param name="input"></param>
        ///// <response code="200">if the operation succeed</response>
        ///// <response code="400">if the operation is not allowed</response>
        //[HttpDelete("{id}/[action]/{measureId}")]
        //public async Task<IActionResult> BodyWeights(DeletePhysiologicalMeasureInfo input)
        //{
        //    await DeleteOneMeasureAsync<BodyWeight>(input);
        //    return new NoContentResult();
        //}

        ///// <summary>
        ///// Gets mot recents <see cref="PhysiologicalMeasurement"/>
        ///// </summary>
        ///// <typeparam name="TPhysiologicalMeasure"></typeparam>
        ///// <typeparam name="TPhysiologicalMeasureInfo"></typeparam>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //private async Task<Option<IEnumerable<TPhysiologicalMeasureInfo>>> MostRecentMeasureAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(GetMostRecentPhysiologicalMeasuresInfo query)
        //    where TPhysiologicalMeasure : PhysiologicalMeasurement
        //    where TPhysiologicalMeasureInfo : PhysiologicalMeasurementInfo
        //    => await _physiologicalMeasureService.GetMostRecentMeasuresAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(new WantMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasureInfo>(query));


        ///// <summary>
        ///// Gets mot recents <see cref="PhysiologicalMeasurement"/>
        ///// </summary>
        ///// <typeparam name="TPhysiologicalMeasure">Type of measure to delete</typeparam>
        ///// <param name="input"></param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <returns></returns>
        //private async Task DeleteOneMeasureAsync<TPhysiologicalMeasure>(DeletePhysiologicalMeasureInfo input, CancellationToken cancellationToken = default)
        //    where TPhysiologicalMeasure : PhysiologicalMeasurement
        //    => await _physiologicalMeasureService.DeleteOnePhysiologicalMeasureAsync<TPhysiologicalMeasure>(new DeleteOnePhysiologicalMeasureCommand(input), cancellationToken);

        ///// <summary>
        ///// Gets one of the patient's prescription
        ///// </summary>
        ///// <param name="id">Id of the patient</param>
        ///// <param name="prescriptionId">Identifier of the prescription to get</param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <returns></returns>
        ///// <response code="200">if the prescription was found</response>
        ///// <response code="404">no prescription with the <paramref name="prescriptionId"/> found.</response>
        ///// <response code="404">no patient with the <paramref name="id"/> found</response>
        //[HttpGet("{id}/[action]/{prescriptionId}")]
        //[HttpHead("{id}/[action]/{prescriptionId}")]
        //[ProducesResponseType(typeof(BrowsableResource<PrescriptionHeaderInfo>), 200)]
        //public async Task<IActionResult> Prescriptions(Guid id, Guid prescriptionId, CancellationToken cancellationToken = default)
        //{
        //    Option<PrescriptionHeaderInfo> output = await _prescriptionService.GetOnePrescriptionByPatientIdAsync(id, prescriptionId, cancellationToken);

        //    return output.Match<IActionResult>(
        //        some: x =>
        //           {
        //               IBrowsableResource<PrescriptionHeaderInfo> browsableResource = new BrowsableResource<PrescriptionHeaderInfo>
        //               {
        //                   Links = new[]
        //                   {
        //                       new Link
        //                        {
        //                            Relation = nameof(Measures.Items),
        //                            Href = UrlHelper.Action(nameof(PrescriptionsController.Details), PrescriptionsController.EndpointName , new { x.Id })
        //                        },
        //                       new Link
        //                       {
        //                           Relation = "self",
        //                           Href = UrlHelper.Action(nameof(Prescriptions), EndpointName, new { id = x.PatientId, prescriptionId = x.Id })
        //                       }
        //                   },
        //                   Resource = x
        //               };

        //               return new OkObjectResult(browsableResource);
        //           },
        //        none: () => new NotFoundResult());
        //}


        ///// <summary>
        ///// Create a new prescription for a patient
        ///// </summary>
        ///// <param name="id">id of the patient</param>
        ///// <param name="newPrescription">prescription details</param>
        ///// <returns></returns>
        ///// <response code="201">The header of the prescription.</response>
        ///// <response code="400">if <paramref name="newPrescription"/> contains invalid data.</response>
        //[HttpPost("{id}/[action]")]
        //[ProducesResponseType(typeof(PrescriptionHeaderInfo), 201)]
        //public async Task<IActionResult> Prescriptions(Guid id, [FromBody] CreatePrescriptionInfo newPrescription)
        //{
        //    PrescriptionHeaderInfo createdPrescription = await _prescriptionService.CreatePrescriptionForPatientAsync(id, newPrescription);

        //    IBrowsableResource<PrescriptionHeaderInfo> browsableResource = new BrowsableResource<PrescriptionHeaderInfo>
        //    {
        //        Resource = createdPrescription,
        //        Links = new[]
        //        {
        //            new Link {
        //                Href = UrlHelper.Action(nameof(PrescriptionsController.Details), PrescriptionsController.EndpointName, new {createdPrescription.Id}),
        //                Method = "GET",
        //                Relation = nameof(Measures.Items),
        //                Title = "Content"
        //            }
        //        }
        //    };
        //    return new CreatedAtActionResult(nameof(Prescriptions), EndpointName, new { id = createdPrescription.PatientId, prescriptionId = createdPrescription.Id }, browsableResource);
        //}


        ///// <summary>
        ///// Gets the most recent prescriptions
        ///// </summary>
        ///// <remarks>
        /////     Only the metadata of the prescriptions are retireved from here. To get the full content of the prescriptions
        /////     You should call :
        /////     
        /////     // api/Prescriptions/{id}/Items
        /////     
        /////     where {id} is the id of the prescription
        /////     
        ///// </remarks>
        ///// <param name="id">id of the patient to get the most </param>
        ///// <param name="count"></param>
        ///// <returns></returns>
        ///// <reponse code="200">List of the most recent prescriptions' metadata</reponse>
        ///// <reponse code="404">List of prescriptions' metadata</reponse>
        //[HttpGet("{id}/[action]")]
        //[HttpHead("{id}/[action]")]
        //[ProducesResponseType(typeof(IEnumerable<PrescriptionHeaderInfo>), 200)]
        //public async Task<IActionResult> MostRecentPrescriptions(Guid id, [FromQuery] int? count)
        //{
        //    GetMostRecentPrescriptionsInfo input = new GetMostRecentPrescriptionsInfo { PatientId = id, Count = count };
        //    Option<IEnumerable<PrescriptionHeaderInfo>> prescriptions = await _prescriptionService.GetMostRecentPrescriptionsAsync(new WantMostRecentPrescriptionsQuery(input));

        //    return prescriptions.Match<IActionResult>(
        //        some: x => new OkObjectResult(x),
        //        none: () => new NotFoundResult());
        //}


        ///// <summary>
        ///// Adds a new <see cref="BodyWeightInfo"/> for the patient with the specified <paramref name="id"/>.
        ///// </summary>
        ///// <param name="id">id of the patient the measure will be created for</param>
        ///// <param name="newBodyWeight">measure to add</param>
        ///// <response code="201">the resource created successfully</response>
        ///// <response code="400"><paramref name="newBodyWeight"/> or <paramref name="id"/> are not valid.</response>
        //[HttpPost("{id}/[action]")]
        //[ProducesResponseType(typeof(BodyWeightInfo), 200)]
        //public async Task<IActionResult> BodyWeights(Guid id, [FromBody] CreateBodyWeightInfo newBodyWeight)
        //{

        //    CreatePhysiologicalMeasureInfo<BodyWeight> input = new CreatePhysiologicalMeasureInfo<BodyWeight>
        //    {
        //        PatientId = id,
        //        Measure = new BodyWeight
        //        {
        //            DateOfMeasure = newBodyWeight.DateOfMeasure,
        //            Value = newBodyWeight.Value
        //        }
        //    };
        //    Option<BodyWeightInfo, CommandException> output = await _physiologicalMeasureService.AddNewMeasureAsync<BodyWeight, BodyWeightInfo>(new AddNewPhysiologicalMeasureCommand<BodyWeight, BodyWeightInfo>(input));
        //    return output.Match(
        //        some: bw => new CreatedAtActionResult(nameof(BodyWeights), EndpointName, new { id = bw.PatientId, bodyWeightId = bw.Id }, bw),
        //        none: exception =>
        //        {
        //            IActionResult actionResult;
        //            switch (exception)
        //            {
        //                case CommandEntityNotFoundException cenf:
        //                    actionResult = new NotFoundResult();
        //                    break;
        //                default:
        //                    actionResult = new InternalServerErrorResult();
        //                    break;
        //            }
        //            return actionResult;
        //        });
        //}

        ///// <summary>
        ///// Generates additional id for the patient resource
        ///// </summary>
        ///// <param name="resource"></param>
        ///// <returns></returns>
        //protected IEnumerable<Link> BuildAdditionalLinksForResource(PatientInfo resource)
        //    => resource.MainDoctorId.HasValue
        //        ? new[]
        //        {
        //            new Link {
        //                Relation = "main-doctor",
        //                Href = UrlHelper.Action(nameof(DoctorsController.Get), DoctorsController.EndpointName, new { id = resource.MainDoctorId })
        //            },
        //            new Link
        //            {
        //                Relation = "delete",
        //                Method = "DELETE",
        //                Href = UrlHelper.Action(nameof(PatientsController.Delete), EndpointName, new { id = resource.Id })
        //            },
        //            new Link
        //            {
        //                Relation = nameof(Documents).ToLower(),
        //                Href = UrlHelper.Action(nameof(PatientsController.Documents), EndpointName, new { id = resource.Id })
        //            },
        //            new Link
        //            {
        //                Relation = nameof(PatientsController.MostRecentTemperatures).ToLowerKebabCase(),
        //                Href = UrlHelper.Action(nameof(PatientsController.MostRecentTemperatures), EndpointName, new { id = resource.Id })
        //            },
        //            new Link
        //            {
        //                Relation = nameof(PatientsController.MostRecentPatients).ToLowerKebabCase(),
        //                Href = UrlHelper.Action(nameof(PatientsController.MostRecentPatients), EndpointName, new { id = resource.Id })
        //            }
        //        }
        //        : Enumerable.Empty<Link>();


        ///// <summary>
        ///// Gets all patient's documents metadata
        ///// </summary>
        ///// <remarks>
        ///// This method gets all documents' metadata that are related to the patient with the specified <paramref name="id"/>.
        ///// Documents are sorted by their last updated date descending.
        ///// </remarks>
        ///// <param name="id">id of the patient to get documents from</param>
        ///// <param name="page">Index of the page of result set (the first page is 1).</param>
        ///// <param name="pageSize">Size of a page of results.</param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <returns></returns>
        ///// <response code="200">The documents' metadata.</response>
        ///// <response code="404">if no patient found.</response>
        //[HttpGet("{id}/[action]")]
        //[ProducesResponseType(typeof(IEnumerable<BrowsableResource<DocumentMetadataInfo>>), 200)]
        //public async Task<IActionResult> Documents(Guid id, [FromQuery]int page, [FromQuery]int pageSize, CancellationToken cancellationToken = default)
        //{
        //    PaginationConfiguration query = new PaginationConfiguration
        //    {
        //        Page = page,
        //        PageSize = Math.Min(ApiOptions.Value.MaxPageSize, pageSize)
        //    };

        //    Option<Page<DocumentMetadataInfo>> result = await _iHandleGetDocumentByPatientIdQuery.HandleAsync(new WantDocumentsByPatientIdQuery(id, query), cancellationToken);



        //    return result.Match<IActionResult>(
        //        some: pageOfResult =>

        //           {
        //               int count = pageOfResult.Entries.Count();
        //               bool hasPreviousPage = count > 0 && query.Page > 1;

        //               string firstPageUrl = UrlHelper.Action(nameof(Documents), EndpointName, new { PageSize = query.PageSize, Page = 1, id });
        //               string previousPageUrl = hasPreviousPage
        //                       ? UrlHelper.Action(nameof(Documents), EndpointName, new { PageSize = query.PageSize, Page = query.Page - 1, id, })
        //                       : null;

        //               string nextPageUrl = query.Page < pageOfResult.PageCount
        //                       ? UrlHelper.Action(nameof(Documents), EndpointName, new { PageSize = query.PageSize, Page = query.Page + 1, id })
        //                       : null;
        //               string lastPageUrl = pageOfResult.PageCount > 0
        //                       ? UrlHelper.Action(nameof(Documents), EndpointName, new { PageSize = query.PageSize, Page = pageOfResult.PageCount, id })
        //                       : null;

        //               IEnumerable<BrowsableResource<DocumentMetadataInfo>> resources = pageOfResult.Entries
        //                    .Select(x => new BrowsableResource<DocumentMetadataInfo> { Resource = x });

        //               IGenericPagedGetResponse<BrowsableResource<DocumentMetadataInfo>> response = new GenericPagedGetResponse<BrowsableResource<DocumentMetadataInfo>>(
        //                   resources,
        //                   firstPageUrl,
        //                   previousPageUrl,
        //                   nextPageUrl,
        //                   lastPageUrl,
        //                   pageOfResult.Total);

        //               return new OkObjectResult(response);
        //           },
        //        none: () => new NotFoundResult());
        //}


        ///// <summary>
        ///// Creates a new <paramref name="document"/> and attaches it to the patient resource with the specified <paramref name="id"/>.
        ///// </summary>
        ///// <param name="id">id of the patient resource <paramref name="document"/> must be attached the to.</param>
        ///// <param name="document">The file to upload</param>
        ///// <returns></returns>
        ///// <response code="201">the document metadata</response>
        ///// <response code="400">Invalid data sent (no binary content, missing required field(s), ...)</response>
        ///// <response code="404">No patient found</response>
        //[HttpPost("{id}/[action]")]
        //[ProducesResponseType(typeof(DocumentMetadataInfo), 201)]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> Documents(Guid id, [FromForm] IFormFile document)
        //{
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        await document.CopyToAsync(ms);
        //        CreateDocumentInfo documentInfo = new CreateDocumentInfo
        //        {
        //            MimeType = document.ContentType,
        //            Content = ms.ToArray(),
        //            Title = document.Name
        //        };
        //        CreateDocumentForPatientCommand cmd = new CreateDocumentForPatientCommand(id, documentInfo);
        //        Option<DocumentMetadataInfo, CommandException> resource = await _iRunCreateDocumentForPatientCommand.RunAsync(cmd);

        //        return resource.Match(
        //            some: doc =>
        //           {
        //               IBrowsableResource<DocumentMetadataInfo> browsableResource = new BrowsableResource<DocumentMetadataInfo>
        //               {
        //                   Resource = doc,
        //                   Links = new[]
        //                    {
        //                        new Link { Relation = "direct-link" },
        //                        new Link { Relation = "file" }
        //                    }
        //               };

        //               return new CreatedAtActionResult(
        //                    nameof(Documents),
        //                    EndpointName,
        //                    new { id, documentId = doc.Id },
        //                    browsableResource);
        //           },
        //            none: exception =>
        //           {
        //               IActionResult result;
        //               switch (exception)
        //               {
        //                   case CommandEntityNotFoundException cenf:
        //                       result = new NotFoundObjectResult(cenf.Message);
        //                       break;
        //                   default:
        //                       result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //                       break;
        //               }

        //               return result;
        //           });
        //    }
        //}


        ///// <summary>
        ///// Gets the specified document that is associated to the specified patient
        ///// </summary>
        ///// <param name="id">id of the patient to </param>
        ///// <param name="documentMetadataId">id of the document to get</param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <returns></returns>
        ///// <response code="404">no patient/document found</response>
        ///// <response code="200">The document</response>
        //[HttpGet("{id}/[action]/{documentMetadataId}")]
        //[ProducesResponseType(typeof(DocumentMetadataInfo), 200)]
        //public async Task<IActionResult> Documents(Guid id, Guid documentMetadataId, CancellationToken cancellationToken = default)
        //{
        //    Option<DocumentMetadataInfo> resource = await _iHandleGetOneDocumentInfoByPatientIdAndDocumentId
        //        .HandleAsync(new WantOneDocumentByPatientIdAndDocumentIdQuery(id, documentMetadataId), cancellationToken);

        //    return resource.Match<IActionResult>(
        //        some: x =>
        //   {
        //       IBrowsableResource<DocumentMetadataInfo> browsableResource = new BrowsableResource<DocumentMetadataInfo>
        //       {
        //           Resource = x,
        //           Links = new[]
        //           {
        //                new Link { Relation = "file", Href = UrlHelper.Action(nameof(DocumentsController.File), DocumentsController.EndpointName, new { x.Id }) },
        //                new Link { Relation = "direct-link", Href = UrlHelper.Action(nameof(DocumentsController.Get), DocumentsController.EndpointName, new { x.Id }) }
        //           }
        //       };

        //       return new OkObjectResult(browsableResource);
        //   },
        //    none: () => new NotFoundResult());
        //}

    }
}
