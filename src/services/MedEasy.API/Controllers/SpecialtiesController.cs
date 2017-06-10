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
using MedEasy.Queries.Specialty;
using MedEasy.Commands.Specialty;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using MedEasy.Handlers.Core.Specialty.Commands;
using MedEasy.Handlers.Core.Specialty.Queries;
using System.Threading;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="SpecialtyInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class SpecialtiesController : RestCRUDControllerBase<Guid, Specialty, SpecialtyInfo, IWantOneSpecialtyInfoByIdQuery, IWantManySpecialtyInfoQuery, Guid, CreateSpecialtyInfo, ICreateSpecialtyCommand, IRunCreateSpecialtyCommand>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(SpecialtiesController).Replace("Controller", string.Empty);


        /// <summary>
        /// Name of the controller without the "Controller" suffix 
        /// </summary>
        protected override string ControllerName => EndpointName;

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
        /// <param name="urlHelper">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="iFindDoctorsBySpecialtyIdQueryHandler">handlers for queries to get doctors by specialty id</param>
        /// <param name="apiOptions">Options accessor</param>
        public SpecialtiesController(ILogger<SpecialtiesController> logger, IUrlHelper urlHelper,
            IOptionsSnapshot<MedEasyApiOptions> apiOptions, IHandleGetSpecialtyInfoByIdQuery getByIdQueryHandler,
            IHandleGetManySpecialtyInfosQuery getManySpecialtyQueryHandler,
            IRunCreateSpecialtyCommand iRunCreateSpecialtyCommand,
            IRunDeleteSpecialtyByIdCommand iRunDeleteSpecialtyByIdCommand,
            IHandleFindDoctorsBySpecialtyIdQuery iFindDoctorsBySpecialtyIdQueryHandler) : base(logger, apiOptions, getByIdQueryHandler, getManySpecialtyQueryHandler, iRunCreateSpecialtyCommand, urlHelper)
        {
            _iRunCreateSpecialtyCommand = iRunCreateSpecialtyCommand;
            _iRunDeleteSpecialtyByIdCommand = iRunDeleteSpecialtyByIdCommand;
            _iFindDoctorsBySpecialtyIdQueryHandler = iFindDoctorsBySpecialtyIdQueryHandler;
        }


        /// <summary>
        /// Gets all the entries in the repository
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SpecialtyInfo>), 200)]
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration query)
        {
            IPagedResult<SpecialtyInfo> result = await GetAll(query);

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


            IGenericPagedGetResponse<SpecialtyInfo> response = new GenericPagedGetResponse<SpecialtyInfo>(
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
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">Resource</response>
        /// <response code="404">Resource not found</response>
        [HttpHead("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SpecialtyInfo), 200)]
        public async override Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default(CancellationToken)) => await base.Get(id, cancellationToken);



        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="info">data used to create the resource</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="404">Resource not found</response>
        /// <returns>the created resource</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SpecialtyInfo), 201)]
        public async Task<IActionResult> Post([FromBody] CreateSpecialtyInfo info, CancellationToken cancellationToken = default(CancellationToken))
        {
            SpecialtyInfo output = await _iRunCreateSpecialtyCommand.RunAsync(new CreateSpecialtyCommand(info), cancellationToken);
            return new CreatedAtActionResult(nameof(Get), EndpointName, new { output.Id}, output);
        }


        /// <summary>
        /// Updates the specified resource
        /// </summary>
        /// <param name="id">identifier of the resource to update</param>
        /// <param name="info">new values to set</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SpecialtyInfo), 200)]
        public Task<IActionResult> Put(Guid id, [FromBody] SpecialtyInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/specialties/5

        /// <summary>
        /// Delete the <see cref="SpecialtyInfo"/> by its 
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="404">Resource not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _iRunDeleteSpecialtyByIdCommand.RunAsync(new DeleteSpecialtyByIdCommand(id), cancellationToken);
            return new NoContentResult();
        }


        /// <summary>
        /// Finds doctors by the specialty they practice
        /// </summary>
        /// <param name="id">Specialty to lookup doctors for</param>
        /// <param name="query">Page of result configuration (page index, page size, ..)</param>
        /// <response code="200"></response>
        /// <response code="404">Specialty not found</response>
        [HttpGet("{id}/Doctors")]
        [ProducesResponseType(typeof(IEnumerable<DoctorInfo>), 200)]
        public async Task<IActionResult> Doctors(Guid id, [FromQuery] PaginationConfiguration query)
        {
            if (query == null)
            {
                query = new PaginationConfiguration
                {
                    Page = 1,
                    PageSize = ApiOptions.Value.DefaulLimit
                };
            }

            query.PageSize = Math.Min(ApiOptions.Value.MaxPageSize, query.PageSize);

            IPagedResult<DoctorInfo> pageResult = await _iFindDoctorsBySpecialtyIdQueryHandler.HandleAsync(new FindDoctorsBySpecialtyIdQuery(id, query));
            Debug.Assert(pageResult != null);

            IGenericPagedGetResponse<DoctorInfo> pagedResponse = new GenericPagedGetResponse<DoctorInfo>(
                pageResult.Entries,
                first: UrlHelper.Action(nameof(SpecialtiesController.Doctors), EndpointName, new { id, query.PageSize, Page = 1 }),
                previous: pageResult.PageCount > 1 && query.Page > 1
                    ? UrlHelper.Action(nameof(SpecialtiesController.Doctors), EndpointName, new { id, query.PageSize, Page = query.Page - 1 })
                    : null,
                next: query.Page < pageResult.PageCount
                    ? UrlHelper.Action(nameof(SpecialtiesController.Doctors), EndpointName, new { id, query.PageSize, Page = query.Page + 1 })
                    : null,
                last: pageResult.PageCount > 1 
                    ? UrlHelper.Action(nameof(SpecialtiesController.Doctors), EndpointName, new { id, query.PageSize, Page = pageResult.PageCount })
                    : null
                );

            return new OkObjectResult(pagedResponse);

        }
    }
}
