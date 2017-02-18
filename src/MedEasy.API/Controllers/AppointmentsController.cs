using System;
using System.Threading.Tasks;
using MedEasy.Objects;
using MedEasy.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MedEasy.DAL.Repositories;
using System.Linq;
using MedEasy.Queries.Appointment;
using MedEasy.Commands.Appointment;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using MedEasy.Validators;
using Microsoft.AspNetCore.JsonPatch;
using AutoMapper;
using MedEasy.Commands;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MedEasy.Data;
using MedEasy.Queries.Search;
using static MedEasy.Data.DataFilterLogic;
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Handlers.Core.Search.Queries;
using MedEasy.Handlers.Core.Appointment.Queries;
using static MedEasy.Data.DataFilterOperator;
using MedEasy.DTO.Search;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="AppointmentInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class AppointmentsController : RestCRUDControllerBase<int, Appointment, AppointmentInfo, IWantOneAppointmentInfoByIdQuery, IWantManyAppointmentInfoQuery, Guid, CreateAppointmentInfo, ICreateAppointmentCommand, IRunCreateAppointmentCommand>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(AppointmentsController).Replace("Controller", string.Empty);


        /// <summary>
        /// Name of the controller without the "Controller" suffix 
        /// </summary>
        protected override string ControllerName => EndpointName;

       

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IRunCreateAppointmentCommand _iRunCreateAppointmentCommand;
        private readonly IRunDeleteAppointmentInfoByIdCommand _iRunDeleteAppointmentByIdCommand;
        private readonly IMapper _mapper;
        private readonly IRunPatchAppointmentCommand _iRunPatchAppointmentCommand;
        private readonly IHandleSearchQuery _iHandleSearchQuery;

        /// <summary>
        /// Builds a new <see cref="AppointmentsController"/> instance
        /// </summary>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="getByIdQueryHandler">Handler of GET one resource</param>
        /// <param name="iHandleSearchQuery">Handler of GET /api/Appointments/search queries.</param>
        /// <param name="getManyAppointmentQueryHandler">Handler of GET many resources</param>
        /// <param name="iRunCreateAppointmentCommand">Runner of CREATE resource command</param>
        /// <param name="iRunDeleteAppointmentByIdCommand">Runner of DELETE resource command</param>
        /// <param name="iRunPatchAppointmentCommand">Runner of PATCH resource command</param>
        /// <param name="logger">logger</param>
        /// <param name="urlHelperFactory">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="mapper">Instance of <see cref="IMapper"/> allowing to map from one type to an other</param>
        /// <param name="actionContextAccessor"></param>
        public AppointmentsController(ILogger<AppointmentsController> logger, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor, 
            IOptions<MedEasyApiOptions> apiOptions,
            IMapper mapper,
            IHandleGetAppointmentInfoByIdQuery getByIdQueryHandler,
            IHandleGetManyAppointmentInfosQuery getManyAppointmentQueryHandler,
            IRunCreateAppointmentCommand iRunCreateAppointmentCommand,
            IRunDeleteAppointmentInfoByIdCommand iRunDeleteAppointmentByIdCommand,
            IRunPatchAppointmentCommand iRunPatchAppointmentCommand, IHandleSearchQuery iHandleSearchQuery) : base(logger, apiOptions, getByIdQueryHandler, getManyAppointmentQueryHandler, iRunCreateAppointmentCommand, urlHelperFactory, actionContextAccessor)
        { 
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
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
        [ProducesResponseType(typeof(IEnumerable<AppointmentInfo>), 200)]
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

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);


            string firstPageUrl = urlHelper.Action(nameof(Get), ControllerName, new {PageSize = query.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page - 1 })
                    : null;

            string nextPageUrl = query.Page < result.PageCount
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page + 1 })
                    : null;
            string lastPageUrl = result.PageCount > 0
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = result.PageCount })
                    : null;


            IGenericPagedGetResponse<AppointmentInfo> response = new GenericPagedGetResponse<AppointmentInfo>(
                result.Entries,
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
        /// <returns></returns>
        [HttpHead("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AppointmentInfo), 200)]
        public async override Task<IActionResult> Get(int id) => await base.Get(id);
            

        
        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="info">data used to create the resource</param>
        /// <returns>the created resource</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AppointmentInfo), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Post([FromBody] CreateAppointmentInfo info)
        {
            AppointmentInfo output = await _iRunCreateAppointmentCommand.RunAsync(new CreateAppointmentCommand(info));
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            IBrowsableResource<AppointmentInfo> browsableResource = new BrowsableResource<AppointmentInfo>
            {
                Resource = output,
                Links = new[]
                {
                    new Link { Relation = nameof(Appointment.Doctor),   },
                    new Link { Relation = nameof(Appointment.Patient)  },
                }
            };

            return new CreatedAtActionResult(nameof(Get), ControllerName, new { id = output.Id }, browsableResource);
        }

        
        /// <summary>
        /// Updates the specified resource
        /// </summary>
        /// <param name="id">identifier of the resource to update</param>
        /// <param name="info">new values to set</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Produces(typeof(AppointmentInfo))]
        public async Task<IActionResult> Put(int id, [FromBody] AppointmentInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/specialties/5

        /// <summary>
        /// Delete the <see cref="AppointmentInfo"/> by its 
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <response code="200">if the deletion succeed</response>
        /// <response code="400">if the resource cannot be deleted</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _iRunDeleteAppointmentByIdCommand.RunAsync(new DeleteAppointmentByIdCommand(id));
            return new OkResult();
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
        /// <response code="200">The resource was successfully patched </response>
        /// <response code="400">Changes are not valid</response>
        /// <response code="404">Resource to "PATCH" not found</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<AppointmentInfo> changes)
        {
            PatchInfo<int, Appointment> data = new PatchInfo<int, Appointment>
            {
                Id = id,
                PatchDocument = _mapper.Map<JsonPatchDocument<Appointment>>(changes)
            };
            await _iRunPatchAppointmentCommand.RunAsync(new PatchCommand<int, Appointment>(data));


            return new OkResult();
        }


        /// <summary>
        /// Search Appointments resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
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
        [ProducesResponseType(typeof(IEnumerable<AppointmentInfo>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        public async Task<IActionResult> Search([FromQuery] SearchAppointmentInfo search)
        {


            IList<IDataFilter> filters = new List<IDataFilter>();
            if (search.From.HasValue)
            {
                filters.Add(new DataFilter { Field = nameof(Appointment.StartDate), Operator = GreaterThanOrEqual, Value = search.From  });
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
            
            IPagedResult<AppointmentInfo> pageOfResult = await _iHandleSearchQuery.Search<Appointment, AppointmentInfo>(new SearchQuery<AppointmentInfo>(searchQueryInfo));

            search.PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize);
            int count = pageOfResult.Entries.Count();
            bool hasPreviousPage = count > 0 && search.Page > 1;

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            string firstPageUrl = urlHelper.Action(nameof(Search), ControllerName, new { search.From, search.To, search.DoctorId, search.PatientId, Page = 1, search.PageSize, search.Sort });
            string previousPageUrl = hasPreviousPage
                    ? urlHelper.Action(nameof(Search), ControllerName, new { search.From, search.To, search.DoctorId, search.PatientId, Page = search.Page - 1, search.PageSize, search.Sort })
                    : null;

            string nextPageUrl = search.Page < pageOfResult.PageCount
                    ? urlHelper.Action(nameof(Search), ControllerName, new { search.From, search.To, search.DoctorId, search.PatientId, Page = search.Page + 1, search.PageSize, search.Sort })
                    : null;
            string lastPageUrl = pageOfResult.PageCount > 1
                    ? urlHelper.Action(nameof(Search), ControllerName, new { search.From, search.To, search.DoctorId, search.PatientId, Page = pageOfResult.PageCount, search.PageSize, search.Sort })
                    : null;

            IGenericPagedGetResponse<AppointmentInfo> reponse = new GenericPagedGetResponse<AppointmentInfo>(
                pageOfResult.Entries,
                first: firstPageUrl,
                previous: previousPageUrl,
                next: nextPageUrl,
                last: lastPageUrl,
                count: pageOfResult.Total);

            return new OkObjectResult(reponse);

        }

    }
}
