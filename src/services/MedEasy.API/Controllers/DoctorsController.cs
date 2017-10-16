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
using MedEasy.DTO.Search;
using System.Threading;
using Optional;
using MedEasy.Handlers.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using FluentValidation.Results;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="DoctorInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class DoctorsController : RestCRUDControllerBase<Guid, Doctor, DoctorInfo, IWantOneDoctorInfoByIdQuery, IWantManyDoctorInfoQuery, Guid, CreateDoctorInfo, ICreateDoctorCommand, IRunCreateDoctorCommand>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(DoctorsController).Replace("Controller", string.Empty);


        /// <summary>
        /// Name of the controller without the "Controller" suffix 
        /// </summary>
        protected override string ControllerName => EndpointName;
        
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
        /// <param name="iRunPatchDoctorCommand">Runs Patch Doctor resource commands</param>
        /// <param name="logger">logger</param>
        /// <param name="urlHelper">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="mapper">Instance of <see cref="IMapper"/> allowing to map from one type to an other</param>
        public DoctorsController(ILogger<DoctorsController> logger, IUrlHelper urlHelper,
            IOptionsSnapshot<MedEasyApiOptions> apiOptions,
            IMapper mapper,
            IHandleGetDoctorInfoByIdQuery getByIdQueryHandler,
            IHandleGetPageOfDoctorInfosQuery getManyDoctorQueryHandler,
            IRunCreateDoctorCommand iRunCreateDoctorCommand,
            IRunDeleteDoctorInfoByIdCommand iRunDeleteDoctorByIdCommand,
            IRunPatchDoctorCommand iRunPatchDoctorCommand,
            IHandleSearchQuery iHandleSearchQuery) : base(logger, apiOptions, getByIdQueryHandler, getManyDoctorQueryHandler, iRunCreateDoctorCommand, urlHelper)
        { 
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
        [ProducesResponseType(typeof(IEnumerable<BrowsableResource<DoctorInfo>>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), 400)]
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                query = new PaginationConfiguration();
            }

            IPagedResult<DoctorInfo> result = await GetAll(query);
            
            int count = result.Entries.Count();
             
            bool hasPreviousPage = count > 0 && query.Page > 1;

            string firstPageUrl = UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = ControllerName, query.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = ControllerName, query.PageSize, Page = query.Page - 1 })
                    : null;

            string nextPageUrl = query.Page < result.PageCount
                    ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = ControllerName, query.PageSize, Page = query.Page + 1 })
                    : null;
            string lastPageUrl = result.PageCount > 0
                    ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = ControllerName,  query.PageSize, Page = result.PageCount })
                    : null;

            IEnumerable<BrowsableResource<DoctorInfo>> resources = result.Entries
                .Select(x => new BrowsableResource<DoctorInfo> { Resource = x, Links = BuildAdditionalLinksForResource(x) });

            IGenericPagedGetResponse<BrowsableResource<DoctorInfo>> response = new GenericPagedGetResponse<BrowsableResource<DoctorInfo>>(
                resources,
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
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        [HttpHead("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DoctorInfo), 200)]
        public async override Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default) => await base.Get(id, cancellationToken);



        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="info">data used to create the resource</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns>the created resource</returns>
        /// <response code="404">if <see cref="DoctorInfo.SpecialtyId"/> is not empty but does not represent a specialty id.</response>
        [HttpPost]
        [ProducesResponseType(typeof(DoctorInfo), 201)]
        [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), 400)]
        public async Task<IActionResult> Post([FromBody] CreateDoctorInfo info, CancellationToken cancellationToken = default)
        {
            Option<DoctorInfo, CommandException> output = await _iRunCreateDoctorCommand.RunAsync(new CreateDoctorCommand(info), cancellationToken);

            return output.Match(
                some: x => new CreatedAtActionResult(nameof(Get), ControllerName, new { id = x.Id }, new BrowsableResource<DoctorInfo> { Resource = x, Links = BuildAdditionalLinksForResource(x) }),
                none: exception =>
               {
                   IActionResult result;
                   switch (exception)
                   {
                       case CommandEntityNotFoundException cenf:
                           result = new NotFoundResult();
                           break;
                       default:
                           result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
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
        [Produces(typeof(DoctorInfo))]
        public Task<IActionResult> Put(Guid id, [FromBody] DoctorInfo info) => throw new NotImplementedException();

        // DELETE api/specialties/5

        /// <summary>
        /// Delete the <see cref="DoctorInfo"/> by its 
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">if the deletion succeed</response>
        /// <response code="400">if the resource cannot be deleted</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            await _iRunDeleteDoctorByIdCommand.RunAsync(new DeleteDoctorByIdCommand(id), cancellationToken);
            return new NoContentResult();
        }

        /// <summary>
        /// Partially update a doctor resource.
        /// </summary>
        /// <remarks>
        /// Use <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        ///
        ///     // PATCH api/Doctors/
        ///     
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/MainDoctorId",
        ///             "from": "string",
        ///             "value": "e1aa24f4-69a8-4d3a-aca9-ec15c6910dc9"
        ///         },
        ///         {
        ///             "op": "update",
        ///             "path": "/Firstname",
        ///             "from": "string",
        ///             "value": "Hugo"
        ///         }
        ///     ]
        /// 
        /// The set of changes to apply will be applied atomically. 
        /// 
        /// </remarks>
        /// <param name="id">id of the resource to update</param>
        /// <param name="changes">set of changes to apply to the resource</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">The resource was successfully patched </response>
        /// <response code="400">Invalid set of changes.</response>
        /// <response code="404">Resource to "PATCH" not found.</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), 400)]
        public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<DoctorInfo> changes, CancellationToken cancellationToken = default)
        {
            PatchInfo<Guid, Doctor> data = new PatchInfo<Guid, Doctor>
            {
                Id = id,
                PatchDocument = _mapper.Map<JsonPatchDocument<Doctor>>(changes)
            };
            await _iRunPatchDoctorCommand.RunAsync(new PatchCommand<Guid, Doctor>(data), cancellationToken);


            return new NoContentResult();
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
        [ProducesResponseType(typeof(IEnumerable<BrowsableResource<DoctorInfo>>), 200)]
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

            string firstPageUrl = UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = ControllerName, search.Firstname, search.Lastname, Page = 1, search.PageSize, search.Sort });
            string previousPageUrl = hasPreviousPage
                    ? UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new {controller = ControllerName, search.Firstname, search.Lastname, Page = search.Page - 1, search.PageSize, search.Sort })
                    : null;

            string nextPageUrl = search.Page < pageOfResult.PageCount
                    ? UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = ControllerName, search.Firstname, search.Lastname, Page = search.Page + 1, search.PageSize, search.Sort })
                    : null;
            string lastPageUrl = pageOfResult.PageCount > 1
                    ? UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = ControllerName, search.Firstname, search.Lastname, Page = pageOfResult.PageCount, search.PageSize, search.Sort })
                    : null;

            IEnumerable<BrowsableResource<DoctorInfo>> resources = pageOfResult.Entries
                .Select(x => new BrowsableResource<DoctorInfo> { Resource = x, Links = BuildAdditionalLinksForResource(x) });


            IGenericPagedGetResponse<BrowsableResource<DoctorInfo>> reponse = new GenericPagedGetResponse<BrowsableResource<DoctorInfo>>(
                resources,
                first: firstPageUrl,
                previous: previousPageUrl,
                next: nextPageUrl,
                last: lastPageUrl,
                count: pageOfResult.Total);

            return new OkObjectResult(reponse);

        }

    }
}
