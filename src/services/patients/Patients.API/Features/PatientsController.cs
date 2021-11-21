namespace Patients.API.Controllers
{

    using DataFilters;

    using FluentValidation.Results;

    using MedEasy.Attributes;
    using MedEasy.Core;
    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;
    using MedEasy.DTO;
    using MedEasy.DTO.Search;
    using MedEasy.RestObjects;

    using MediatR;

    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Optional;

    using Patients.API.Routing;
    using Patients.CQRS.Commands;
    using Patients.CQRS.Queries;
    using Patients.DTO;
    using Patients.Ids;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static MedEasy.RestObjects.LinkRelation;

    /// <summary>
    /// Endpoint for <see cref="PatientInfo"/> resources.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class PatientsController
    {
        private IOptionsSnapshot<PatientsApiOptions> ApiOptions { get; }

        private readonly ILogger<PatientsController> _logger;
        private readonly LinkGenerator _urlHelper;
        private readonly IMediator _mediator;

        private static string ControllerName => EndpointName;

        /// <summary>
        /// Name of the resources
        /// </summary>
        public static string EndpointName => nameof(PatientsController).Replace("Controller", string.Empty);

        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="urlHelper">Helper to build resources links</param>
        /// <param name="apiOptions">Snapshot of API Options</param>
        /// <param name="mediator">Mediator service</param>
        public PatientsController(ILogger<PatientsController> logger,
                                  LinkGenerator urlHelper,
                                  IOptionsSnapshot<PatientsApiOptions> apiOptions,
                                  IMediator mediator)
        {
            _logger = logger;
            _urlHelper = urlHelper;
            ApiOptions = apiOptions;
            _mediator = mediator;
        }

        /// <summary>
        /// Gets all the resources of the endpoint
        /// </summary>
        /// <param name="pagination">index of the page of resources to get</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Resources are returned as pages. The <paramref name="pagination"/>'s value is used has a hint by the server
        /// and there's no garanty that the size of page of result will be equal to the <paramref name="pageSize"/> set in the query.
        /// In particular, the number of resources on a page may be caped by the server.
        /// </remarks>
        /// <response code="200">items of the page</response>
        /// <response code="400"><paramref name="pagination"/> is not valid</response>
        /// <response code="404"><paramref name="pagination"/> is outside range of available page</response>
        [HttpGet, HttpHead, HttpOptions]
        [ProducesResponseType(typeof(GenericPagedGetResponse<Browsable<PatientInfo>>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute, RequireNonDefault] PaginationConfiguration pagination, CancellationToken cancellationToken = default)
        {
            pagination = new() { Page = pagination.Page, PageSize = Math.Min(ApiOptions.Value.MaxPageSize, pagination.PageSize) };

            GetPageOfPatientsQuery query = new(pagination);
            Page<PatientInfo> pageOfResult = await _mediator.Send(query, cancellationToken)
                                                            .ConfigureAwait(false);

            GenericPagedGetResponse<Browsable<PatientInfo>> response = new(pageOfResult.Entries
                                                                                       .Select(item => new Browsable<PatientInfo>
                                                                                       {
                                                                                           Resource = item,
                                                                                           Links = new[]
                                                                                           {
                                                                                               new Link
                                                                                               {
                                                                                                   Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = ControllerName, item.Id }),
                                                                                                   Method = "GET",
                                                                                                   Relation = Self
                                                                                               }
                                                                                           }
                                                                                       }),
                                                                           first: _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi,
                                                                                                    new { page = 1, pagination.PageSize, controller = EndpointName }),
                                                                           next: pageOfResult.Count > 1
                                                                                ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi,
                                                                                                           new { page = pagination.Page + 1, pagination.PageSize, controller = EndpointName })
                                                                                : null,
                                                                           last: _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi,
                                                                                                          new { page = pageOfResult.Count, pagination.PageSize, controller = EndpointName }),
                                                                           previous: pagination.Page > 1 && pageOfResult.Count > 1
                                                                                ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi,
                                                                                                           new { page = pagination.Page + 1, pagination.PageSize, controller = EndpointName })
                                                                                : null,

                                                                            total: pageOfResult.Total);
            return new OkObjectResult(response);
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
        [HttpGet("{id}")]
        [HttpOptions("{id}")]
        [ProducesResponseType(typeof(Browsable<PatientInfo>), Status200OK)]
        public async Task<IActionResult> Get([FromRoute, RequireNonDefault] PatientId id, CancellationToken cancellationToken = default)
        {
            IActionResult actionResult;
            if (id == PatientId.Empty)
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                GetOnePatientInfoByIdQuery query = new GetOnePatientInfoByIdQuery(id);
                Option<PatientInfo> result = await _mediator.Send(query, cancellationToken)
                                                            .ConfigureAwait(false);
                actionResult = result.Match<IActionResult>(
                    some: resource =>
                    {
                        Browsable<PatientInfo> browsableResource = new()
                        {
                            Resource = resource,
                            Links = new[]
                            {
                                    new Link
                                    {
                                        Relation = LinkRelation.Self,
                                        Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, Id = resource.Id.Value }),
                                        Method = "GET"
                                    },
                                    new Link
                                    {
                                        Relation = "delete",
                                        Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, Id = resource.Id.Value }),
                                        Method = "DELETE"
                                    }
                            }
                        };
                        return new OkObjectResult(browsableResource);
                    },
                    none: () => new NotFoundResult()
                );

            }
            return actionResult;
        }

        /// <summary>
        /// Creates a new <see cref="PatientInfo"/> resource.
        /// </summary>
        /// <param name="newPatient">data used to create the resource</param>
        /// <param name="ct">Notifies lower layers about the request abortion</param>
        /// <response code="201">the resource was created successfully</response>
        /// <response code="400"><paramref name="newPatient"/> is not valid</response>
        [HttpPost]
        [ProducesResponseType(typeof(Browsable<PatientInfo>), Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), Status400BadRequest)]
        public async Task<ActionResult> Post([FromBody] CreatePatientInfo newPatient, CancellationToken ct = default)
        {
            CreatePatientInfoCommand cmd = new(newPatient);

            Option<PatientInfo, CreateCommandFailure> optionalResource = await _mediator.Send(cmd, ct).ConfigureAwait(false);

            return optionalResource.Match<ActionResult>(
                some: resource => new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id }, new Browsable<PatientInfo>
                {
                    Resource = resource,
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = Self,
                            Method = "GET",
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, Id = resource.Id.Value })
                        },
                        new Link
                        {
                            Relation = "delete",
                            Method = "DELETE",
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, Id = resource.Id.Value })
                        }
                    }
                }),
                none: failure => failure switch
                {
                    CreateCommandFailure.Conflict => new ConflictResult(),
                    CreateCommandFailure.NotFound => new NotFoundResult(),
                    CreateCommandFailure.Unauthorized => new StatusCodeResult(Status401Unauthorized),
                });
        }

        // DELETE measures/bloodpressures/5

        /// <summary>
        /// Delete the <see cref="PatientInfo"/> by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="204">if the operation succeed</response>
        /// <response code="400">if <paramref name="id"/> is not valid.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute, RequireNonDefault] PatientId id, CancellationToken cancellationToken = default)
        {
            ActionResult result;

            if (id == PatientId.Empty)
            {
                result = new BadRequestResult();
            }
            else
            {
                DeletePatientInfoByIdCommand cmd = new(id);
                DeleteCommandResult cmdResult = await _mediator.Send(cmd, cancellationToken)
                    .ConfigureAwait(false);

                result = cmdResult switch
                {
                    DeleteCommandResult.Done => new NoContentResult(),
                    DeleteCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                    DeleteCommandResult.Failed_NotFound => new NotFoundResult(),
                    DeleteCommandResult.Failed_Conflict => new StatusCodeResult(Status409Conflict)
                };
            }

            return result;
        }

        /// <summary>
        /// Partially update a patient resource.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        /// </para>
        /// <example>
        /// <c>PATCH api/Patients/3594c436-8595-444d-9e6b-2686c4904725</c>
        /// with the following body
        /// <code>
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/Firstname",
        ///             "from": "string",
        ///             "value": "John"
        ///       }
        ///     ]
        /// </code>
        /// </example>
        /// <para>The set of changes to apply will be applied atomically and in the order they're declared. </para>
        ///
        /// </remarks>
        /// <param name="id">id of the resource to update.</param>
        /// <param name="changes">set of changes to apply to the resource.</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">The resource was successfully patched.</response>
        /// <response code="400"><paramref name="id"/> <paramref name="changes"/> are not valid for the selected resource.</response>
        /// <response code="404">Resource not found</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), 400)]
        public async Task<IActionResult> Patch([FromRoute, RequireNonDefault] PatientId id, [FromBody] JsonPatchDocument<PatientInfo> changes, CancellationToken cancellationToken = default)
        {
            PatchInfo<PatientId, PatientInfo> data = new()
            {
                Id = id,
                PatchDocument = changes
            };
            PatchCommand<PatientId, PatientInfo> cmd = new(data);

            ModifyCommandResult cmdResult = await _mediator.Send(cmd, cancellationToken)
                .ConfigureAwait(false);

            return cmdResult switch
            {
                ModifyCommandResult.Done => new NoContentResult(),
                ModifyCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                ModifyCommandResult.Failed_NotFound => new NotFoundResult(),
                ModifyCommandResult.Failed_Conflict => new StatusCodeResult(Status409Conflict),
                _ => new StatusCodeObjectResult(Status500InternalServerError, $"Unexpected <{cmdResult}> patch result"),
            };
        }

        /// <summary>
        /// Search doctors resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of the request.</param>
        /// <remarks>
        /// <para>All criteria are combined as a AND.</para>
        /// <para>
        /// Advanded search :
        /// Several operators that can be used to make an advanced search :
        /// '*' : match zero or more characters in a string property.
        /// </para>
        /// <para>
        ///     // GET api/Doctors/Search?Firstname=Bruce
        ///     will match all resources which have exactly 'Bruce' in the Firstname property
        /// </para>
        /// <para>
        ///     // GET api/Doctors/Search?Firstname=B*e
        ///     will match all resources which starts with 'B' and ends with 'e'.
        /// </para>
        /// <para>'?' : match exactly one charcter in a string property.</para>
        /// <para>'!' : negate a criteria</para>
        /// <para>
        ///     // GET api/Doctors/Search?Firstname=!Bruce
        ///     will match all resources where Firstname is not "Bruce"
        /// </para>
        ///
        /// </remarks>
        /// <response code="200">Array of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="400">one the search criteria is not valid</response>
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [ProducesResponseType(typeof(GenericPagedGetResponse<Browsable<PatientInfo>>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        public async Task<IActionResult> Search([FromQuery, RequireNonDefault] SearchPatientInfo search, CancellationToken cancellationToken = default)
        {
            SearchPatientInfoQuery request = new (new SearchPatientInfo
            {
                BirthDate = search.BirthDate,
                Firstname = search.Firstname,
                Lastname = search.Lastname,
                Sort = search.Sort,
                PageSize = Math.Min(ApiOptions.Value.MaxPageSize, search.PageSize),
                Page = search.Page,
            });

            Page<PatientInfo> pageOfResources = await _mediator.Send(request, cancellationToken)
                                                               .ConfigureAwait(false);
            IActionResult actionResult;
            if (pageOfResources.Count < request.Data.Page)
            {
                actionResult = new NotFoundResult();
            }
            else
            {
                GenericPagedGetResponse<Browsable<PatientInfo>> page = new(
                        items: pageOfResources.Entries.Select(x => new Browsable<PatientInfo>
                        {
                            Resource = x,
                            Links = new[]
                            {
                                new Link
                                {
                                    Method = "GET",
                                    Relation = Self,
                                    Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = ControllerName, id = x.Id.Value })
                                },
                                new Link()
                                {
                                    Method = "DELETE",
                                    Relation = "delete",
                                    Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = ControllerName,id = x.Id.Value })
                                }
                            }
                        }),
                        first: _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
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
                            ? _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
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
                        next: pageOfResources.Count > search.Page
                            ? _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
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
                        last: _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                        {
                            controller = ControllerName,
                            search.Firstname,
                            search.Lastname,
                            search.BirthDate,
                            search.Sort,
                            page = Math.Max(pageOfResources.Count, 1),
                            search.PageSize
                        }),
                        total: pageOfResources.Total);

                actionResult = new OkObjectResult(page);
            }

            return actionResult;
        }
    }
}
