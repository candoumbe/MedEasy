using AutoMapper;

using DataFilters;

using Forms;

using Measures.API.Features.Patients;
using Measures.API.Features.v1.BloodPressures;
using Measures.API.Routing;
using Measures.CQRS.Commands;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Commands.Patients;
using Measures.CQRS.Queries.Patients;
using Measures.DTO;
using Measures.Models.v1;

using MedEasy.Attributes;
using MedEasy.Core.Results;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.DTO.Search;
using MedEasy.Models;
using MedEasy.RestObjects;

using MediatR;

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

using Optional;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static Forms.LinkRelation;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Measures.API.Features.v1
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="GenericMeasureFormInfo"/> resources
    /// </summary>
    [Route("/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
    public class FormsController
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(FormsController).Replace("Controller", string.Empty);

        /// <summary>
        /// Helper to build URLs
        /// </summary>
        private readonly LinkGenerator _urlHelper;

        /// <summary>
        /// Current version of the endpoint
        /// </summary>
        private readonly ApiVersion _apiVersion;
        private readonly IMapper _mapper;

        /// <summary>
        /// Options of the API
        /// </summary>
        private readonly IOptionsSnapshot<MeasuresApiOptions> _apiOptions;

        /// <summary>
        /// Builds a new <see cref="FormsController"/> instance
        /// </summary>
        /// <param name="urlHelper">Helper class to build URL strings.</param>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="mediator"></param>
        /// <param name="apiVersion"></param>
        /// <param name="mapper"></param>
        public FormsController(LinkGenerator urlHelper, IOptionsSnapshot<MeasuresApiOptions> apiOptions, IMediator mediator, ApiVersion apiVersion, IMapper mapper)
        {
            _urlHelper = urlHelper;
            _apiOptions = apiOptions;
            _mediator = mediator;
            _apiVersion = apiVersion;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets all the resources of the endpoint
        /// </summary>
        /// <param name="page">index of the page of resources to get</param>
        /// <param name="pageSize">index of the page of resources to get</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of the request</param>
        /// <remarks>
        /// Resources are returned as pages. The <paramref name="pagination"/>'s value is used has a hint by the server
        /// and there's no garanty that the size of page of result will be equal to the <paramref name="pagination"/>' property <see cref="PaginationConfiguration.PageSize"/> set in the query.
        /// In particular, the number of resources on a page may be caped by the server.
        /// </remarks>
        /// <response code="200">The page returned contains the whole set of resources at the time the query was made.</response>
        /// <response code="206">The page returned contains the a set of resources and there are more pages available.</response>
        /// <response code="400"><paramref name="pagination"/><see cref="PaginationConfiguration.Page"/> or <paramref name="pagination"/><see cref="PaginationConfiguration.PageSize"/> is negative or zero</response>
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [ProducesResponseType(typeof(GenericPageModel<Browsable<GenericMeasureFormModel>>), Status200OK)]
        public async Task<GenericPageModel<Browsable<GenericMeasureFormModel>>> Get([Minimum(1)] int page,
                                                                                    [Minimum(1)] int pageSize,
                                                                                    CancellationToken cancellationToken = default)
        {
            PaginationConfiguration pagination = new PaginationConfiguration
            {
                Page = page,
                PageSize = Math.Min(pageSize, _apiOptions.Value.MaxPageSize)
            };
            Page<GenericMeasureFormInfo> result = await _mediator.Send(new GetPageOfGenericMeasureFormInfoQuery(pagination), cancellationToken)
                                                                 .ConfigureAwait(false);

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && pagination.Page > 1;
            string version = _apiVersion?.ToString();

            string firstPageUrl = _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = 1, version });
            string previousPageUrl = hasPreviousPage
                    ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page - 1, version })
                    : null;

            string nextPageUrl = pagination.Page < result.Count
                    ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page + 1, version })
                    : null;
            string lastPageUrl = result.Count > 0
                    ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = result.Count, version })
                    : firstPageUrl;

            IEnumerable<Browsable<GenericMeasureFormModel>> resources = result.Entries
                .Select(x => new Browsable<GenericMeasureFormModel>
                {
                    Resource = new GenericMeasureFormModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Fields = x.Fields
                    },
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = Self,
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, x.Id, version})
                        }
                    }
                });

            return new GenericPageModel<Browsable<GenericMeasureFormModel>>
            {
                Items = resources,
                Links = new PageLinksModel
                {
                    First = new Link { Href = firstPageUrl, Relation = First },
                    Previous = previousPageUrl is null
                        ? null
                        : new Link { Href = previousPageUrl, Relation = Previous },
                    Next = nextPageUrl is null
                        ? null
                        : new Link { Href = nextPageUrl, Relation = Next },
                    Last = new Link { Href = lastPageUrl, Relation = Last },
                },
                Total = result.Total
            };
        }

        /// <summary>
        /// Gets one resource by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to look for</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">The resource was found</response>
        /// <response code="404">Resource not found</response>
        /// <response code="400"><paramref name="id"/> is not a valid <see cref="Guid"/></response>
        [HttpHead("{id}")]
        [HttpOptions("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Browsable<GenericMeasureFormModel>), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ActionResult<Browsable<GenericMeasureFormModel>>> Get([RequireNonDefault] Guid id, CancellationToken cancellationToken = default)
        {
            Option<GenericMeasureFormInfo> result = await _mediator.Send(new GetOneMeasureFormByIdQuery(id), cancellationToken)
                .ConfigureAwait(false);

            return result.Match<ActionResult<Browsable<GenericMeasureFormModel>>>(
                some: resource =>
                {
                    string version = _apiVersion.ToString();
                    return new Browsable<GenericMeasureFormModel>
                    {
                        Resource = new GenericMeasureFormModel
                        {
                            Id = resource.Id,
                            Name = resource.Name,
                            Fields = resource.Fields
                        },
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = Self,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id, version }),
                                Method = "GET"
                            },
                            new Link
                            {
                                Relation = "delete",
                                Href = _urlHelper.GetPathByName(
                                    RouteNames.DefaultGetOneByIdApi,
                                    new { controller = EndpointName, resource.Id, version }),
                                Method = "DELETE"
                            }
                        }
                    };
                },
                none: () => new NotFoundResult()
            );
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
        ///     will match match all resources which starts with 'B' and ends with 'e'.
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
        /// <response code="404">The requested page is out of results page count bounds.</response>
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [HttpOptions("[action]")]
        [ProducesResponseType(typeof(GenericPageModel<Browsable<PatientInfo>>), Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery, RequireNonDefault] SearchPatientInfo search, CancellationToken cancellationToken = default)
        {
            IList<IFilter> filters = new List<IFilter>();
            if (!string.IsNullOrEmpty(search.Name))
            {
                filters.Add($"{nameof(search.Name)}={search.Name}".ToFilter<PatientInfo>());
            }

            SearchQueryInfo<PatientInfo> searchQuery = new SearchQueryInfo<PatientInfo>
            {
                Filter = filters.Count == 1
                    ? filters.Single()
                    : new MultiFilter { Logic = FilterLogic.And, Filters = filters },
                Page = search.Page,
                PageSize = search.PageSize,
                Sort = search.Sort?.ToSort<PatientInfo>() ?? new Sort<PatientInfo>(nameof(PatientInfo.UpdatedDate), SortDirection.Descending)
            };
            Page<PatientInfo> page = await _mediator.Send(new SearchQuery<PatientInfo>(searchQuery), cancellationToken)
                .ConfigureAwait(false);
            IActionResult actionResult;
            string version = _apiVersion.ToString();

            if (searchQuery.Page <= page.Count)
            {
                GenericPageModel<Browsable<PatientInfo>> response = new GenericPageModel<Browsable<PatientInfo>>
                {
                    Items = page.Entries.Select(x => new Browsable<PatientInfo>
                    {
                        Resource = x,
                        Links = new[] {
                            new Link
                            {
                                Method = "GET",
                                Relation = Self,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, x.Id })
                            }
                        }
                    }),
                    Links = new PageLinksModel
                    {
                        First = new Link
                        {
                            Relation = First,
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                            {
                                controller = EndpointName,
                                search.Name,
                                search.BirthDate,
                                search.Sort,
                                page = 1,
                                search.PageSize,
                                version
                            })
                        },
                        Previous = search.Page > 1
                            ? new Link
                            {
                                Relation = Previous,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                                {
                                    controller = EndpointName,
                                    search.Name,
                                    search.BirthDate,
                                    search.Sort,
                                    page = search.Page - 1,
                                    search.PageSize,
                                    version
                                })
                            }
                            : null,
                        Next = page.Count > search.Page
                            ? new Link
                            {
                                Relation = Next,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                                {
                                    controller = EndpointName,
                                    search.Name,
                                    search.BirthDate,
                                    search.Sort,
                                    page = search.Page + 1,
                                    search.PageSize,
                                    version
                                })
                            }
                            : null,
                        Last = new Link
                        {
                            Relation = Last,
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                            {
                                controller = EndpointName,
                                search.Name,
                                search.BirthDate,
                                search.Sort,
                                page = Math.Max(page.Count, 1),
                                search.PageSize,
                                version
                            })
                        }
                    },
                    Total = page.Total
                };
                actionResult = new OkObjectResult(response);
            }
            else
            {
                actionResult = new NotFoundResult();
            }

            return actionResult;
        }

        /// <summary>
        /// Delete a resource by its id
        /// </summary>
        /// <param name="id">id of the resource to delete.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        public async Task<ActionResult> Delete([RequireNonDefault] Guid id, CancellationToken ct = default)
        {
            DeleteCommandResult result = await _mediator.Send(new DeleteMeasureFormInfoByIdCommand(id), ct)
                .ConfigureAwait(false);

            return result switch
            {
                DeleteCommandResult.Done => new NoContentResult(),
                DeleteCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                DeleteCommandResult.Failed_NotFound => new NotFoundResult(),
                DeleteCommandResult.Failed_Conflict => new ConflictResult(),
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, $"Unexpected value <{result}> for {nameof(DeleteCommandResult)}"),
            };
        }

        /// <summary>
        /// Creates a new patient resource
        /// </summary>
        /// <param name="newform">data for the resource to create</param>
        /// <param name="ct"></param>
        /// <response code="400">data provided does not allow to create the resource</response>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Browsable<GenericMeasureFormModel>), Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        public async Task<ActionResult<Browsable<GenericMeasureFormModel>>> Post([FromBody] NewMeasureFormModel newform, CancellationToken ct = default)
        {
            CreateGenericMeasureFormInfo data = new CreateGenericMeasureFormInfo
            {
                Name = newform.Name,
                Fields = newform.Fields
            };

            CreateGenericMeasureFormInfoCommand cmd = new CreateGenericMeasureFormInfoCommand(data);

            GenericMeasureFormInfo resource = await _mediator.Send(cmd, ct)
                                                             .ConfigureAwait(false);

            string version = _apiVersion.ToString();
            Browsable<GenericMeasureFormModel> browsableResource = new Browsable<GenericMeasureFormModel>
            {
                Resource = new GenericMeasureFormModel
                {
                    Id = resource.Id,
                    Name = resource.Name,
                    Fields = resource.Fields
                },
                Links = new[]
                {
                    new Link
                    {
                        Relation = Self,
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, resource.Id, version})
                    },
                    new Link
                    {
                        Relation = "delete",
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                                        new { controller = BloodPressuresController.EndpointName, patientId = resource.Id, page = 1, pageSize = _apiOptions.Value.DefaultPageSize, version }) }
                    }
            };

            return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id, version }, browsableResource);
        }

        /// <summary>
        /// Partially updates a resource with the specified id
        /// </summary>
        /// <param name="id">id of the resource to patch</param>
        /// <param name="changes">modifications to perform. Will be applied atomically.</param>
        /// <param name="ct">Notification to cancel the execution of the request</param>
        /// <returns></returns>
        /// <reponse code="204">the resource was updated successfully</reponse>
        /// <reponse code="404">the resource was not found</reponse>
        /// <reponse code="400"><paramref name="id"/> or <paramref name="changes"/> are not valid</reponse>
        [HttpPatch("{id}")]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Patch([RequireNonDefault] Guid id, [FromBody] JsonPatchDocument<GenericMeasureFormModel> changes, CancellationToken ct = default)
        {
            PatchInfo<Guid, GenericMeasureFormInfo> data = new PatchInfo<Guid, GenericMeasureFormInfo>
            {
                Id = id,
                PatchDocument = _mapper.Map<JsonPatchDocument<GenericMeasureFormInfo>>(changes)
            };

            ModifyCommandResult result = await _mediator.Send(new PatchCommand<Guid, GenericMeasureFormInfo>(data), ct)
                                                        .ConfigureAwait(false);

            return result switch {
                ModifyCommandResult.Done => new NoContentResult(),
                ModifyCommandResult.Failed_Conflict => new ConflictResult(),
                ModifyCommandResult.Failed_NotFound => new NotFoundResult(),
                ModifyCommandResult.Failed_Unauthorized => new UnauthorizedResult()
            };
        }
    }
}
