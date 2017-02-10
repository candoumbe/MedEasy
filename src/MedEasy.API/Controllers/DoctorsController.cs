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
using MedEasy.Queries.Doctor;
using MedEasy.Commands.Doctor;
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
using MedEasy.Handlers.Core.Doctor.Commands;
using MedEasy.Handlers.Core.Search.Queries;
using MedEasy.Handlers.Core.Doctor.Queries;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="DoctorInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class DoctorsController : RestCRUDControllerBase<int, Doctor, DoctorInfo, IWantOneDoctorInfoByIdQuery, IWantManyDoctorInfoQuery, Guid, CreateDoctorInfo, ICreateDoctorCommand, IRunCreateDoctorCommand>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(DoctorsController).Replace("Controller", string.Empty);


        /// <summary>
        /// Name of the controller without the "Controller" suffix 
        /// </summary>
        protected override string ControllerName => EndpointName;

       

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IRunCreateDoctorCommand _iRunCreateDoctorCommand;
        private readonly IRunDeleteDoctorInfoByIdCommand _iRunDeleteDoctorByIdCommand;
        private readonly IMapper _mapper;
        private readonly IRunPatchDoctorCommand _iRunPatchDoctorCommand;
        private readonly IHandleSearchQuery _iHandleSearchQuery;

        /// <summary>
        /// Builds a new <see cref="DoctorsController"/> instance
        /// </summary>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="getByIdQueryHandler">Handler of GET one resource</param>
        /// <param name="iHandleSearchQuery">Handler of GET /api/Doctors/search queries.</param>
        /// <param name="getManyDoctorQueryHandler">Handler of GET many resources</param>
        /// <param name="iRunCreateDoctorCommand">Runner of CREATE resource command</param>
        /// <param name="iRunDeleteDoctorByIdCommand">Runner of DELETE resource command</param>
        /// <param name="logger">logger</param>
        /// <param name="urlHelperFactory">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="mapper">Instance of <see cref="IMapper"/> allowing to map from one type to an other</param>
        /// <param name="actionContextAccessor"></param>
        public DoctorsController(ILogger<DoctorsController> logger, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor, 
            IOptions<MedEasyApiOptions> apiOptions,
            IMapper mapper,
            IHandleGetDoctorInfoByIdQuery getByIdQueryHandler,
            IHandleGetManyDoctorInfosQuery getManyDoctorQueryHandler,
            IRunCreateDoctorCommand iRunCreateDoctorCommand,
            IRunDeleteDoctorInfoByIdCommand iRunDeleteDoctorByIdCommand,
            IRunPatchDoctorCommand iRunPatchDoctorCommand, IHandleSearchQuery iHandleSearchQuery) : base(logger, apiOptions, getByIdQueryHandler, getManyDoctorQueryHandler, iRunCreateDoctorCommand, urlHelperFactory, actionContextAccessor)
        { 
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _iRunCreateDoctorCommand = iRunCreateDoctorCommand;
            _iRunDeleteDoctorByIdCommand = iRunDeleteDoctorByIdCommand;
            _mapper = mapper;
            _iRunPatchDoctorCommand = iRunPatchDoctorCommand;
            _iHandleSearchQuery = iHandleSearchQuery;



        }


        /// <summary>
        /// Gets all the entries in the repository
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DoctorInfo>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration query)
        {
            if (query == null)
            {
                query = new PaginationConfiguration();
            }

            IPagedResult<DoctorInfo> result = await GetAll(query);
            
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


            IGenericPagedGetResponse<DoctorInfo> response = new GenericPagedGetResponse<DoctorInfo>(
                result.Entries,
                firstPageUrl,
                previousPageUrl,
                nextPageUrl,
                lastPageUrl,
                result.Total);
            

            return new OkObjectResult(response);
        }


        /// <summary>
        /// Gets the <see cref="DoctorInfo"/> resource by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to look for</param>
        /// <returns></returns>
        [HttpHead("{id:int}")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DoctorInfo), 200)]
        public async override Task<IActionResult> Get(int id) => await base.Get(id);
            

        
        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="info">data used to create the resource</param>
        /// <returns>the created resource</returns>
        [HttpPost]
        [ProducesResponseType(typeof(DoctorInfo), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Post([FromBody] CreateDoctorInfo info)
        {
            DoctorInfo output = await _iRunCreateDoctorCommand.RunAsync(new CreateDoctorCommand(info));
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            
            return new CreatedAtActionResult(nameof(Get), ControllerName, new { id = output.Id }, output);
        }

        
        /// <summary>
        /// Updates the specified resource
        /// </summary>
        /// <param name="id">identifier of the resource to update</param>
        /// <param name="info">new values to set</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [Produces(typeof(DoctorInfo))]
        public async Task<IActionResult> Put(int id, [FromBody] DoctorInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/specialties/5

        /// <summary>
        /// Delete the <see cref="DoctorInfo"/> by its 
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <response code="200">if the deletion succeed</response>
        /// <response code="400">if the resource cannot be deleted</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _iRunDeleteDoctorByIdCommand.RunAsync(new DeleteDoctorByIdCommand(id));
            return new OkResult();
        }

        /// <summary>
        /// Partially update a doctor resource.
        /// </summary>
        /// <remarks>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        ///
        ///     // PATCH api/Docgtors/1
        ///     
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/MainDoctorId",
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
        [HttpPatch("{id:int}")]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<DoctorInfo> changes)
        {
            PatchInfo<int, Doctor> data = new PatchInfo<int, Doctor>
            {
                Id = id,
                PatchDocument = _mapper.Map<JsonPatchDocument<Doctor>>(changes)
            };
            await _iRunPatchDoctorCommand.RunAsync(new PatchCommand<int, Doctor>(data));


            return new OkResult();
        }


        /// <summary>
        /// Search doctors resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
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
        [ProducesResponseType(typeof(IEnumerable<DoctorInfo>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        public async Task<IActionResult> Search([FromQuery] SearchDoctorInfo search)
        {


            IList<IDataFilter> filters = new List<IDataFilter>();
            if (!string.IsNullOrWhiteSpace(search.Firstname))
            {
                filters.Add($"{nameof(DoctorInfo.Firstname)}={search.Firstname}".ToFilter<DoctorInfo>());
            }

            if (!string.IsNullOrWhiteSpace(search.Lastname))
            {
                filters.Add($"{nameof(DoctorInfo.Lastname)}={search.Lastname}".ToFilter<DoctorInfo>());
            }

            SearchQueryInfo<DoctorInfo> searchQueryInfo = new SearchQueryInfo<DoctorInfo>
            {
                Page = search.Page,
                PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize),
                Filter = filters.Count() == 1
                    ? filters.Single()
                    : new DataCompositeFilter { Logic = And, Filters = filters },
                Sorts = (search.Sort ?? $"-{nameof(DoctorInfo.UpdatedDate)}").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x =>
                        {
                            x = x.Trim();
                            Sort sort;
                            if (x.StartsWith("-"))
                            {
                                x = x.Substring(1);
                                sort = new Sort { Direction = Data.SortDirection.Descending, Expression = x.ToLambda<DoctorInfo>() };
                            }
                            else
                            {
                                sort = new Sort { Direction = Data.SortDirection.Ascending, Expression = x.ToLambda<DoctorInfo>() };
                            }

                            return sort;
                        })
            };



            IPagedResult<DoctorInfo> pageOfResult = await _iHandleSearchQuery.Search<Doctor, DoctorInfo>(new SearchQuery<DoctorInfo>(searchQueryInfo));

            search.PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize);
            int count = pageOfResult.Entries.Count();
            bool hasPreviousPage = count > 0 && search.Page > 1;

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            string firstPageUrl = urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, Page = 1, search.PageSize, search.Sort });
            string previousPageUrl = hasPreviousPage
                    ? urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, Page = search.Page - 1, search.PageSize, search.Sort })
                    : null;

            string nextPageUrl = search.Page < pageOfResult.PageCount
                    ? urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, Page = search.Page + 1, search.PageSize, search.Sort })
                    : null;
            string lastPageUrl = pageOfResult.PageCount > 1
                    ? urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, Page = pageOfResult.PageCount, search.PageSize, search.Sort })
                    : null;

            IGenericPagedGetResponse<DoctorInfo> reponse = new GenericPagedGetResponse<DoctorInfo>(
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
