using AutoMapper;
using MedEasy.API.Results;
using MedEasy.Commands;
using MedEasy.Commands.Appointment;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO;
using MedEasy.DTO.Search;
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Handlers.Core.Appointment.Queries;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Handlers.Core.Search.Queries;
using MedEasy.Objects;
using MedEasy.Queries.Appointment;
using MedEasy.Queries.Search;
using MedEasy.RestObjects;
using MedEasy.Validators;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MedEasy.Data.DataFilterLogic;
using static MedEasy.Data.DataFilterOperator;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="AppointmentInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class AppointmentsController : RestCRUDControllerBase<Guid, Appointment, AppointmentInfo, IWantOneAppointmentInfoByIdQuery, IWantPageOfAppointmentInfosQuery, Guid, CreateAppointmentInfo, ICreateAppointmentCommand, IRunCreateAppointmentCommand>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(AppointmentsController).Replace("Controller", string.Empty);


        /// <summary>
        /// Name of the controller without the "Controller" suffix 
        /// </summary>
        protected override string ControllerName => EndpointName;



        private readonly IRunCreateAppointmentCommand _iRunCreateAppointmentCommand;
        private readonly IRunDeleteAppointmentInfoByIdCommand _iRunDeleteAppointmentByIdCommand;
        private readonly IMapper _mapper;
        private readonly IRunPatchAppointmentCommand _iRunPatchAppointmentCommand;
        private readonly IHandleSearchQuery _iHandleSearchQuery;

        /// <summary>
        /// Builds a new <see cref="AppointmentsController"/> instance.
        /// </summary>
        /// <param name="apiOptions">Options of the API.</param>
        /// <param name="getByIdQueryHandler">Handler of GET one resource</param>
        /// <param name="iHandleSearchQuery">Handler of GET /api/Appointments/search queries.</param>
        /// <param name="getPageOfAppointmentQueryHandler">Handler of GET many resources</param>
        /// <param name="iRunCreateAppointmentCommand">Runner of CREATE resource command</param>
        /// <param name="iRunDeleteAppointmentByIdCommand">Runner of DELETE resource command</param>
        /// <param name="iRunPatchAppointmentCommand">Runner of PATCH resource command</param>
        /// <param name="logger">logger</param>
        /// <param name="urlHelper">Helper to build URLs.</param>
        /// <param name="mapper">Instance of <see cref="IMapper"/> allowing to map from one type to an other</param>
        public AppointmentsController(ILogger<AppointmentsController> logger, IUrlHelper urlHelper,
            IOptionsSnapshot<MedEasyApiOptions> apiOptions,
            IMapper mapper,
            IHandleGetAppointmentInfoByIdQuery getByIdQueryHandler,
            IHandleGetPageOfAppointmentInfosQuery getPageOfAppointmentQueryHandler,
            IRunCreateAppointmentCommand iRunCreateAppointmentCommand,
            IRunDeleteAppointmentInfoByIdCommand iRunDeleteAppointmentByIdCommand,
            IRunPatchAppointmentCommand iRunPatchAppointmentCommand,
            IHandleSearchQuery iHandleSearchQuery) : base(logger, apiOptions, getByIdQueryHandler, getPageOfAppointmentQueryHandler, iRunCreateAppointmentCommand, urlHelper)
        {
            _iRunCreateAppointmentCommand = iRunCreateAppointmentCommand;
            _iRunDeleteAppointmentByIdCommand = iRunDeleteAppointmentByIdCommand;
            _mapper = mapper;
            _iRunPatchAppointmentCommand = iRunPatchAppointmentCommand;
            _iHandleSearchQuery = iHandleSearchQuery;



        }


        /// <summary>
        /// Gets all the entries in the repository
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BrowsableResource<AppointmentInfo>>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration query)
        {
            if (query == null)
            {
                query = new PaginationConfiguration();
            }

            IPagedResult<AppointmentInfo> result = await GetAll(query);

            int count = result.Entries.Count();

            bool hasPreviousPage = count > 0 && query.Page > 1;

            string firstPageUrl = UrlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? UrlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page - 1 })
                    : null;

            string nextPageUrl = query.Page < result.PageCount
                    ? UrlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page + 1 })
                    : null;
            string lastPageUrl = result.PageCount > 0
                    ? UrlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = result.PageCount })
                    : null;


            IEnumerable<BrowsableResource<AppointmentInfo>> resources = result.Entries
                .Select(x => new BrowsableResource<AppointmentInfo>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link { Relation = "self", Href = UrlHelper.Action(nameof(Get), new { x.Id })},
                        new Link { Relation = nameof(Appointment.Doctor), Href = UrlHelper.Action(nameof(DoctorsController.Get), DoctorsController.EndpointName, new {id  = x.DoctorId})},
                        new Link { Relation = nameof(Appointment.Patient), Href = UrlHelper.Action(nameof(PatientsController.Get), PatientsController.EndpointName, new {id = x.PatientId})},
                    }
                })
            #region DEBUG
                .ToArray()
            #endregion
                ;


            IGenericPagedGetResponse<BrowsableResource<AppointmentInfo>> response = new GenericPagedGetResponse<BrowsableResource<AppointmentInfo>>(
                resources,
                firstPageUrl,
                previousPageUrl,
                nextPageUrl,
                lastPageUrl,
                result.Total);


            return new OkObjectResult(response);
        }


        /// <summary>
        /// Gets the <see cref="AppointmentInfo"/> resource by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to look for</param>
        /// <param name="cancellationToken">notifies lower level to abort processing the request</param>
        /// <returns></returns>
        [HttpHead("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BrowsableResource<AppointmentInfo>), 200)]
        public async override Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default(CancellationToken)) => await base.Get(id);



        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="info">data used to create the resource</param>
        /// <param name="cancellationToken">notifies lower level to abort processing the request</param>
        /// <returns>the created resource</returns>
        /// <response code="409">the new appointment overlaps an existing one and </response>
        /// <response code="404">Patient and/or doctor do(es)n't exist</response>
        [HttpPost]
        [ProducesResponseType(typeof(BrowsableResource<AppointmentInfo>), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Post([FromBody] CreateAppointmentInfo info, CancellationToken cancellationToken = default(CancellationToken))
        {
            Option<AppointmentInfo, CommandException> output = await _iRunCreateAppointmentCommand.RunAsync(new CreateAppointmentCommand(info), cancellationToken);

            return output.Match(
                some: x =>
               {
                   IBrowsableResource<AppointmentInfo> browsableResource = new BrowsableResource<AppointmentInfo>
                   {
                       Resource = x,
                       Links = new[]
                        {
                            new Link { Relation = nameof(Appointment.Doctor),   },
                            new Link { Relation = nameof(Appointment.Patient)  },
                        }
                   };

                   return new CreatedAtActionResult(nameof(Get), ControllerName, new { id = x.Id }, browsableResource);
               },

                none: exception =>
                {
                    IActionResult result;
                    switch (exception)
                    {
                        case CommandNotValidException<Guid> notValidException:
                            result = new BadRequestObjectResult(notValidException.Errors);
                            break;
                        case CommandEntityNotFoundException cenf:
                            result = new NotFoundObjectResult(cenf.Message);
                            break;
                        default:
                            result = new InternalServerErrorResult();
                            break;
                    }

                    return result;
                });

        }


        /// <summary>
        /// Updates the specified resource
        /// </summary>
        /// <param name="id">identifier of the resource to update</param>
        /// <param name="info">new values to set</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Produces(typeof(AppointmentInfo))]
        public Task<IActionResult> Put(int id, [FromBody] AppointmentInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/specialties/5

        /// <summary>
        /// Delete the <see cref="AppointmentInfo"/> by its 
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <param name="cancellationToken">notifies lower level to abort processing the request</param>
        /// <response code="204">if the deletion succeed</response>
        /// <response code="400">if the resource cannot be deleted</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _iRunDeleteAppointmentByIdCommand.RunAsync(new DeleteAppointmentByIdCommand(id), cancellationToken);
            return new NoContentResult();
        }

        /// <summary>
        /// Partially update a patient resource.
        /// </summary>
        /// <remarks>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        ///
        ///     // PATCH api/Patients/1
        ///     
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/MainAppointmentId",
        ///             "from": "string",
        ///             "value": 1
        ///       }
        ///     ]
        /// 
        /// The set of changes to apply will be applied atomically. 
        /// 
        /// </remarks>
        /// <param name="id">id of the resource to update</param>
        /// <param name="changes">set of changes to apply to the resource</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="204">The resource was successfully patched </response>
        /// <response code="400">Changes are not valid</response>
        /// <response code="404">Resource to "PATCH" not found</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<AppointmentInfo> changes, CancellationToken cancellationToken = default(CancellationToken))
        {
            PatchInfo<Guid, Appointment> data = new PatchInfo<Guid, Appointment>
            {
                Id = id,
                PatchDocument = _mapper.Map<JsonPatchDocument<Appointment>>(changes)
            };

            await _iRunPatchAppointmentCommand.RunAsync(new PatchCommand<Guid, Appointment>(data), cancellationToken);

            return new NoContentResult();
        }


        /// <summary>
        /// Search Appointments resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <remarks>
        /// All criteria are combined as a AND.
        /// 
        /// Advanded search :
        /// Several operators that can be used to make an advanced search :
        /// '*' : match zero or more characters in a string property.
        /// 
        ///     // GET api/Appointments/Search?Firstname=Bruce
        ///     will match all resources which have exactly 'Bruce' in the Firstname property
        ///     
        ///     // GET api/Appointments/Search?Firstname=B*e
        ///     will match match all resources which starts with 'B' and ends with 'e'.
        /// 
        /// '?' : match exactly one charcter in a string property.
        /// 
        /// '!' : negate a criteria
        /// 
        ///     // GET api/Appointments/Search?Firstname=!Bruce
        ///     will match all resources where Firstname is not "Bruce"
        ///     
        /// </remarks>
        /// <response code="200">Array of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="400">one the search criteria is not valid</response>
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(IEnumerable<BrowsableResource<AppointmentInfo>>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        public async Task<IActionResult> Search([FromQuery] SearchAppointmentInfo search, CancellationToken cancellationToken = default(CancellationToken))
        {


            IList<IDataFilter> filters = new List<IDataFilter>();
            if (search.From.HasValue)
            {
                filters.Add(new DataFilter { Field = nameof(Appointment.StartDate), Operator = GreaterThanOrEqual, Value = search.From });
            }

            if (search.To.HasValue)
            {
                filters.Add(new DataFilter { Field = nameof(Appointment.StartDate), Operator = LessThanOrEqualTo, Value = search.To });
            }

            SearchQueryInfo<AppointmentInfo> searchQueryInfo = new SearchQueryInfo<AppointmentInfo>
            {
                Page = search.Page,
                PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize),
                Filter = filters.Count() == 1
                    ? filters.Single()
                    : new DataCompositeFilter { Logic = And, Filters = filters },
                Sorts = (search.Sort ?? $"-{nameof(AppointmentInfo.UpdatedDate)}").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x =>
                        {
                            x = x.Trim();
                            Sort sort;
                            if (x.StartsWith("-"))
                            {
                                x = x.Substring(1);
                                sort = new Sort { Direction = Data.SortDirection.Descending, Expression = x.ToLambda<AppointmentInfo>() };
                            }
                            else
                            {
                                sort = new Sort { Direction = Data.SortDirection.Ascending, Expression = x.ToLambda<AppointmentInfo>() };
                            }

                            return sort;
                        })
            };

            IPagedResult<AppointmentInfo> pageOfResult = await _iHandleSearchQuery.Search<Appointment, AppointmentInfo>(new SearchQuery<AppointmentInfo>(searchQueryInfo), cancellationToken);

            search.PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize);
            int count = pageOfResult.Entries.Count();
            bool hasPreviousPage = count > 0 && search.Page > 1;

            string firstPageUrl = UrlHelper.Action(nameof(Search), ControllerName, new { search.From, search.To, search.DoctorId, search.PatientId, Page = 1, search.PageSize, search.Sort });
            string previousPageUrl = hasPreviousPage
                    ? UrlHelper.Action(nameof(Search), ControllerName, new { search.From, search.To, search.DoctorId, search.PatientId, Page = search.Page - 1, search.PageSize, search.Sort })
                    : null;

            string nextPageUrl = search.Page < pageOfResult.PageCount
                    ? UrlHelper.Action(nameof(Search), ControllerName, new { search.From, search.To, search.DoctorId, search.PatientId, Page = search.Page + 1, search.PageSize, search.Sort })
                    : null;
            string lastPageUrl = pageOfResult.PageCount > 1
                    ? UrlHelper.Action(nameof(Search), ControllerName, new { search.From, search.To, search.DoctorId, search.PatientId, Page = pageOfResult.PageCount, search.PageSize, search.Sort })
                    : null;

            IEnumerable<BrowsableResource<AppointmentInfo>> results = pageOfResult.Entries
                .Select(x => new BrowsableResource<AppointmentInfo>
                {
                    Resource = x,
                    Links = new List<Link>(BuildAdditionalLinksForResource(x))
                    {
                        new Link { Relation = "self", Href = UrlHelper.Action(nameof(Get), new { x.Id }) }
                    }
                });

            IGenericPagedGetResponse<BrowsableResource<AppointmentInfo>> reponse = new GenericPagedGetResponse<BrowsableResource<AppointmentInfo>>(
                results,
                first: firstPageUrl,
                previous: previousPageUrl,
                next: nextPageUrl,
                last: lastPageUrl,
                count: pageOfResult.Total);

            return new OkObjectResult(reponse);

        }

        /// <summary>
        /// Builds additional links for a <see cref="AppointmentInfo"/> resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        protected override IEnumerable<Link> BuildAdditionalLinksForResource(AppointmentInfo resource) =>
            new[]
            {
                new Link { Relation = "doctor", Href = UrlHelper.Action(nameof(DoctorsController.Get), DoctorsController.EndpointName, new {id  = resource.DoctorId})},
                new Link { Relation = "patient", Href = UrlHelper.Action(nameof(PatientsController.Get), PatientsController.EndpointName, new {id = resource.PatientId})},
            };

    }
}
