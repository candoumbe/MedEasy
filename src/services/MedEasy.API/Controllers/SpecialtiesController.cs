using System;
using System.Threading.Tasks;
using MedEasy.Objects;
using MedEasy.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using MedEasy.RestObjects;
using MedEasy.DAL.Repositories;
using System.Linq;
using MedEasy.Queries.Specialty;
using MedEasy.Commands.Specialty;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using MedEasy.Handlers.Core.Specialty.Commands;
using MedEasy.Handlers.Core.Specialty.Queries;
using System.Threading;
using Optional;
using MedEasy.Handlers.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using MedEasy.API.Results;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="SpecialtyInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class SpecialtiesController : RestCRUDControllerBase<Guid, Specialty, SpecialtyInfo, IWantOneSpecialtyInfoByIdQuery, IWantPageOfSpecialtyInfoQuery, Guid, CreateSpecialtyInfo, ICreateSpecialtyCommand, IRunCreateSpecialtyCommand>
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
            IHandleGetPageOfSpecialtyInfosQuery getManySpecialtyQueryHandler,
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
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<SpecialtyInfo>>), 200)]
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration query)
        {
            IPagedResult<SpecialtyInfo> result = await GetAll(query);

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
                    ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = ControllerName, query.PageSize, Page = result.PageCount })
                    : null;

            IEnumerable<BrowsableResource<SpecialtyInfo>> resources = result.Entries
                .Select(x => new BrowsableResource<SpecialtyInfo> { Resource = x, Links = BuildAdditionalLinksForResource(x) });

            IGenericPagedGetResponse<BrowsableResource<SpecialtyInfo>> response = new GenericPagedGetResponse<BrowsableResource<SpecialtyInfo>>(
                resources,
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
        [ProducesResponseType(typeof(BrowsableResource<SpecialtyInfo>), 200)]
        public async override Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default) => await base.Get(id, cancellationToken);



        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="info">data used to create the resource</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="404">Resource not found</response>
        /// <returns>the created resource</returns>
        [HttpPost]
        [ProducesResponseType(typeof(BrowsableResource<SpecialtyInfo>), 201)]
        public async Task<IActionResult> Post([FromBody] CreateSpecialtyInfo info, CancellationToken cancellationToken = default)
        {
            Option<SpecialtyInfo, CommandException> output = await _iRunCreateSpecialtyCommand.RunAsync(new CreateSpecialtyCommand(info), cancellationToken);

            return output.Match(
                some: specialty =>
                {
                    IBrowsableResource<SpecialtyInfo> browsableResource = new BrowsableResource<SpecialtyInfo>
                    {
                        Resource = specialty,
                        Links = BuildAdditionalLinksForResource(specialty)
                    };
                    return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, specialty.Id }, browsableResource);

                },
                none: exception =>
                {
                    IActionResult result;
                    switch (exception)
                    {
                        case CommandEntityNotFoundException cenf:
                            result = new BadRequestObjectResult(cenf.Message);
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
        [ProducesResponseType(typeof(SpecialtyInfo), 200)]
        public Task<IActionResult> Put(Guid id, [FromBody] SpecialtyInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/specialties/5

        /// <summary>
        /// Delete the <see cref="SpecialtyInfo"/> by its id
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="404">Resource not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
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
        [ProducesResponseType(typeof(IEnumerable<BrowsableResource<DoctorInfo>>), 200)]
        public async Task<IActionResult> Doctors(Guid id, [FromQuery] PaginationConfiguration query)
        {
            if (query == null)
            {
                query = new PaginationConfiguration
                {
                    Page = 1,
                    PageSize = ApiOptions.Value.DefaultPageSize
                };
            }

            query.PageSize = Math.Min(ApiOptions.Value.MaxPageSize, query.PageSize);

            Option<IPagedResult<DoctorInfo>> result = await _iFindDoctorsBySpecialtyIdQueryHandler.HandleAsync(new FindDoctorsBySpecialtyIdQuery(id, query));

            return result.Match<IActionResult>(
                some: page =>
                {
                    IEnumerable<BrowsableResource<DoctorInfo>> resources = page.Entries
                        .Select(x => new BrowsableResource<DoctorInfo>
                        {
                            Resource = x,
                            Links = new[]
                            {
                                new Link
                                {
                                    Relation = LinkRelation.Self,
                                    Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = DoctorsController.EndpointName, x.Id})
                                }
                            }
                        });

                    IGenericPagedGetResponse<BrowsableResource<DoctorInfo>> pagedResponse = new GenericPagedGetResponse<BrowsableResource<DoctorInfo>>(
                        resources,
                        first: UrlHelper.Link(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, new { controller = EndpointName, id, action = nameof(Doctors),  query.PageSize, Page = 1 }),
                        previous: page.PageCount > 1 && query.Page > 1
                            ? UrlHelper.Link(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, new { controller = EndpointName, id, action = nameof(Doctors), query.PageSize, Page = query.Page - 1 })
                            : null,
                        next: query.Page < page.PageCount
                            ? UrlHelper.Link(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, new { controller = EndpointName, id, action = nameof(Doctors), query.PageSize, Page = query.Page + 1 })
                            : null,
                        last: page.PageCount > 1
                            ? UrlHelper.Link(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, new { controller = EndpointName, id, action = nameof(Doctors), query.PageSize, Page = page.PageCount })
                            : null
                        );
                    return new OkObjectResult(pagedResponse);
                },
                none: () => new NotFoundResult());
        }

        /// <summary>
        /// Builds additional likc
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        protected override IEnumerable<Link> BuildAdditionalLinksForResource(SpecialtyInfo resource) =>
            new[]
            {
                new Link {
                    Method = "GET",
                    Relation = nameof(SpecialtiesController.Doctors),
                    Href = UrlHelper.Link(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, new { controller = DoctorsController.EndpointName, resource.Id })
                }
            };
    }
}
