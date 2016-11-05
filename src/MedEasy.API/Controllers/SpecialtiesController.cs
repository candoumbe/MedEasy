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
using MedEasy.Handlers.Specialty.Queries;
using MedEasy.Queries.Specialty;
using MedEasy.Handlers.Specialty.Commands;
using MedEasy.Commands.Specialty;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="SpecialtyInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class SpecialtiesController : RestCRUDControllerBase<int, Specialty, SpecialtyInfo, IWantOneSpecialtyInfoByIdQuery, IWantManySpecialtyInfoQuery, Guid, CreateSpecialtyInfo, ICreateSpecialtyCommand, IRunCreateSpecialtyCommand>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(SpecialtiesController).Replace("Controller", string.Empty);


        /// <summary>
        /// Name of the controller without the "Controller" suffix 
        /// </summary>
        protected override string ControllerName => EndpointName;



        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IRunCreateSpecialtyCommand _iRunCreateSpecialtyCommand;
        private readonly IRunDeleteSpecialtyByIdCommand _iRunDeleteSpecialtyByIdCommand;
        private readonly IHandleFindDoctorsBySpecialtyIdQuery _iFindDoctorsBySpecialtyIdQueryHandler;

        /// <summary>
        /// Builds a new <see cref="SpecialtiesController"/> instance
        /// </summary>
        /// <param name="getByIdQueryHandler">Handler of GET one resource</param>
        /// <param name="getManySpecialtyQueryHandler">Handler of GET many resources</param>
        /// <param name="iRunCreateSpecialtyCommand">Runner of CREATE resource command</param>
        /// <param name="iRunDeleteSpecialtyByIdCommand">Runner of DELETE resource command</param>
        /// <param name="logger">logger</param>
        /// <param name="urlHelperFactory">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="iFindDoctorsBySpecialtyIdQueryHandler">handlers for queries to get doctors by specialty id</param>
        /// <param name="actionContextAccessor"></param>
        public SpecialtiesController(ILogger<SpecialtiesController> logger, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor, IOptions<MedEasyApiOptions> apiOptions,
            IHandleGetSpecialtyInfoByIdQuery getByIdQueryHandler,
            IHandleGetManySpecialtyInfosQuery getManySpecialtyQueryHandler,
            IRunCreateSpecialtyCommand iRunCreateSpecialtyCommand,
            IRunDeleteSpecialtyByIdCommand iRunDeleteSpecialtyByIdCommand,
            IHandleFindDoctorsBySpecialtyIdQuery iFindDoctorsBySpecialtyIdQueryHandler) : base(logger, apiOptions, getByIdQueryHandler, getManySpecialtyQueryHandler, iRunCreateSpecialtyCommand, urlHelperFactory, actionContextAccessor)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _iRunCreateSpecialtyCommand = iRunCreateSpecialtyCommand;
            _iRunDeleteSpecialtyByIdCommand = iRunDeleteSpecialtyByIdCommand;
            _iFindDoctorsBySpecialtyIdQueryHandler = iFindDoctorsBySpecialtyIdQueryHandler;
        }


        /// <summary>
        /// Gets all the entries in the repository
        /// </summary>
        [HttpGet]
        [Produces(typeof(IEnumerable<SpecialtyInfo>))]
        public async Task<IActionResult> Get([FromQuery] GenericGetQuery query)
        {
            IPagedResult<SpecialtyInfo> result = await GetAll(query);

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && query.Page > 1;

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            string firstPageUrl = urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page - 1 })
                    : null;

            string nextPageUrl = query.Page < result.PageCount
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page + 1 })
                    : null;

            string lastPageUrl = result.PageCount > 0
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = result.PageCount })
                    : null;


            IGetResponse<SpecialtyInfo> response = new GenericPagedGetResponse<SpecialtyInfo>(
                result.Entries,
                firstPageUrl,
                previousPageUrl,
                nextPageUrl,
                lastPageUrl,
                result.Total);


            return new OkObjectResult(response);
        }


        /// <summary>
        /// Gets the <see cref="SpecialtyInfo"/> resource by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to look for</param>
        /// <returns></returns>
        [HttpHead("{id:int}")]
        [HttpGet("{id:int}")]
        [Produces(typeof(SpecialtyInfo))]
        public async override Task<IActionResult> Get(int id) => await base.Get(id);



        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="info">data used to create the resource</param>
        /// <returns>the created resource</returns>
        [HttpPost]
        [Produces(typeof(SpecialtyInfo))]
        public async Task<IActionResult> Post([FromBody] CreateSpecialtyInfo info)
        {
            SpecialtyInfo output = await _iRunCreateSpecialtyCommand.RunAsync(new CreateSpecialtyCommand(info));
            return new CreatedAtActionResult(nameof(Get), EndpointName, new { output.Id}, output);
        }


        /// <summary>
        /// Updates the specified resource
        /// </summary>
        /// <param name="id">identifier of the resource to update</param>
        /// <param name="info">new values to set</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [Produces(typeof(SpecialtyInfo))]
        public async Task<IActionResult> Put(int id, [FromBody] SpecialtyInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/specialties/5

        /// <summary>
        /// Delete the <see cref="SpecialtyInfo"/> by its 
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _iRunDeleteSpecialtyByIdCommand.RunAsync(new DeleteSpecialtyByIdCommand(id));
            return await Task.FromResult(new OkResult());
        }


        /// <summary>
        /// Finds doctors by the specialty they practice
        /// </summary>
        /// <param name="id">Specialty to lookup doctors for</param>
        /// <param name="query">Page of result configuration (page index, page size, ..)</param>
        /// <returns></returns>
        [HttpGet("{id:int}/Doctors")]
        [Produces(typeof(IEnumerable<DoctorInfo>))]
        public async Task<IActionResult> Doctors(int id, [FromQuery] GenericGetQuery query)
        {
            if (query == null)
            {
                query = new GenericGetQuery
                {
                    Page = 1,
                    PageSize = ApiOptions.Value.DefaultPageSize
                };
            }

            query.PageSize = Math.Min(ApiOptions.Value.MaxPageSize, query.PageSize);

            IPagedResult<DoctorInfo> pageResult = await _iFindDoctorsBySpecialtyIdQueryHandler.HandleAsync(new FindDoctorsBySpecialtyIdQuery(id, query));
            Debug.Assert(pageResult != null);

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            IGetResponse<DoctorInfo> pagedResponse = new GenericPagedGetResponse<DoctorInfo>(
                pageResult.Entries,
                first: urlHelper.Action(nameof(SpecialtiesController.Doctors), EndpointName, new { id, query.PageSize, Page = 1 }),
                previous: pageResult.PageCount > 1 && query.Page > 1
                    ? urlHelper.Action(nameof(SpecialtiesController.Doctors), EndpointName, new { id, query.PageSize, Page = query.Page - 1 })
                    : null,
                next: query.Page < pageResult.PageCount
                    ? urlHelper.Action(nameof(SpecialtiesController.Doctors), EndpointName, new { id, query.PageSize, Page = query.Page + 1 })
                    : null,
                last: pageResult.PageCount > 1 
                    ? urlHelper.Action(nameof(SpecialtiesController.Doctors), EndpointName, new { id, query.PageSize, Page = pageResult.PageCount })
                    : null
                );

            return new OkObjectResult(pagedResponse);

        }
    }
}
