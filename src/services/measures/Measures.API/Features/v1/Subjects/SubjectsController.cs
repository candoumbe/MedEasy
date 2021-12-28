// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Measures.API.Features.v1.Patients
{
    using DataFilters;

    using Measures.API.Features.Subjects;
    using Measures.API.Features.v1.BloodPressures;
    using Measures.API.Routing;
    using Measures.CQRS.Commands.BloodPressures;
    using Measures.CQRS.Commands.Patients;
    using Measures.CQRS.Queries.Subjects;
    using Measures.DTO;
    using Measures.Ids;

    using MedEasy.Attributes;
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
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Options;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using static Microsoft.AspNetCore.Http.StatusCodes;

    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="SubjectInfo"/> resources
    /// </summary>
    [Route("[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
    public class SubjectsController
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(SubjectsController).Replace("Controller", string.Empty);

        /// <summary>
        /// Helper to build URLs
        /// </summary>
        private readonly LinkGenerator _urlHelper;

        /// <summary>
        /// Options of the API
        /// </summary>
        private readonly IOptionsSnapshot<MeasuresApiOptions> _apiOptions;

        /// <summary>
        /// Builds a new <see cref="SubjectsController"/> instance
        /// </summary>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="urlHelper">Helper class to build URL strings.</param>
        /// <param name="mediator"></param>
        public SubjectsController(LinkGenerator urlHelper,
                                  IOptionsSnapshot<MeasuresApiOptions> apiOptions,
                                  IMediator mediator)
        {
            _urlHelper = urlHelper;
            _apiOptions = apiOptions;
            _mediator = mediator;
        }

        /// <summary>
        /// Gets all the resources of the endpoint
        /// </summary>
        /// <param name="pagination">Sets the resultset size and index</param>
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
        [ProducesResponseType(typeof(GenericPagedGetResponse<Browsable<SubjectInfo>>), Status200OK)]
        [ProducesResponseType(typeof(GenericPagedGetResponse<Browsable<SubjectInfo>>), Status206PartialContent)]
        public async Task<IActionResult> Get([FromQuery, RequireNonDefault] PaginationConfiguration pagination, CancellationToken cancellationToken = default)
        {
            pagination.PageSize = Math.Min(pagination.PageSize, _apiOptions.Value.MaxPageSize);
            Page<SubjectInfo> result = await _mediator.Send(new GetPageOfSubjectInfoQuery(pagination), cancellationToken)
                .ConfigureAwait(false);

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && pagination.Page > 1;

            string firstPageUrl = _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page - 1 })
                    : null;

            string nextPageUrl = pagination.Page < result.Count
                    ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page + 1 })
                    : null;
            string lastPageUrl = result.Count > 0
                    ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = result.Count })
                    : firstPageUrl;

            IEnumerable<Browsable<SubjectInfo>> resources = result.Entries
                .Select(x => new Browsable<SubjectInfo>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = LinkRelation.Self,
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, x.Id})
                        }
                    }
                });

            GenericPagedGetResponse<Browsable<SubjectInfo>> response = new(
                resources,
                firstPageUrl,
                previousPageUrl,
                nextPageUrl,
                lastPageUrl,
                result.Total);

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Gets the <see cref="SubjectInfo"/> resource by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to look for</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">The resource was found</response>
        /// <response code="404">Resource not found</response>
        /// <response code="400"><paramref name="id"/> is not a valid <see cref="Guid"/></response>
        [HttpHead("{id}")]
        [HttpOptions("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Browsable<SubjectInfo>), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Get([RequireNonDefault] SubjectId id, CancellationToken cancellationToken = default)
        {
            Option<SubjectInfo> result = await _mediator.Send(new GetSubjectInfoByIdQuery(id), cancellationToken)
                .ConfigureAwait(false);

            return result.Match<IActionResult>(
                some: resource =>
                {
                    Browsable<SubjectInfo> browsableResource = new()
                    {
                        Resource = resource,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = LinkRelation.Self,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id }),
                                Method = "GET"
                            },
                            new Link
                            {
                                Relation = "delete",
                                Href = _urlHelper.GetPathByName(
                                    RouteNames.DefaultGetOneByIdApi,
                                    new { controller = EndpointName, resource.Id }),
                                Method = "DELETE"
                            },
                            new Link
                            {
                                Relation = BloodPressuresController.EndpointName.Slugify(),
                                Method = "GET",
                                Href =  _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                                                 new {
                                                                     controller = BloodPressuresController.EndpointName,
                                                                     subjectId = resource.Id
                                                                 })
                            }
                        }
                    };
                    return new OkObjectResult(browsableResource);
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
        ///     will match all resources which starts with 'B' and ends with 'e'.
        /// </para>
        /// <para>'?' : match exactly one charcter in a string property.</para>
        /// <para>'!' : negate a criteria</para>
        /// <para>
        ///     // GET api/Doctors/Search?Firstname=!Bruce
        ///     will match all resources where Firstname is not "Bruce"
        /// </para>
        /// </remarks>
        /// <response code="200">Array of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="400">one the search criteria is not valid</response>
        /// <response code="404">The requested page is out of results page count bounds.</response>
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [HttpOptions("[action]")]
        [ProducesResponseType(typeof(GenericPagedGetResponse<Browsable<SubjectInfo>>), Status200OK)]
        [ProducesResponseType(typeof(ErrorObject), Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery, RequireNonDefault] SearchPatientInfo search, CancellationToken cancellationToken = default)
        {
            IList<IFilter> filters = new List<IFilter>();
            if (!string.IsNullOrEmpty(search.Name))
            {
                filters.Add($"{nameof(search.Name)}={search.Name}".ToFilter<SubjectInfo>());
            }

            SearchQueryInfo<SubjectInfo> searchQuery = new()
            {
                Filter = filters.Count == 1
                    ? filters.Single()
                    : new MultiFilter { Logic = FilterLogic.And, Filters = filters },
                Page = search.Page,
                PageSize = search.PageSize,
                Sort = search.Sort?.ToSort<SubjectInfo>() ?? new Sort<SubjectInfo>(nameof(SubjectInfo.UpdatedDate), SortDirection.Descending)
            };
            Page<SubjectInfo> page = await _mediator.Send(new SearchQuery<SubjectInfo>(searchQuery), cancellationToken)
                .ConfigureAwait(false);
            IActionResult actionResult;

            if (searchQuery.Page <= page.Count)
            {
                GenericPagedGetResponse<Browsable<SubjectInfo>> response = new(
                        items: page.Entries.Select(x => new Browsable<SubjectInfo>
                        {
                            Resource = x,
                            Links = new[] {
                            new Link
                            {
                                Method = "GET",
                                Relation = LinkRelation.Self,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, x.Id })
                            }
                            }
                        }),
                        first: _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                        {
                            controller = EndpointName,
                            search.Name,
                            search.BirthDate,
                            search.Sort,
                            page = 1,
                            search.PageSize
                        }),
                        previous: search.Page > 1
                            ? _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                            {
                                controller = EndpointName,
                                search.Name,
                                search.BirthDate,
                                search.Sort,
                                page = search.Page - 1,
                                search.PageSize
                            })
                            : null,
                        next: page.Count > search.Page
                            ? _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                            {
                                controller = EndpointName,
                                search.Name,
                                search.BirthDate,
                                search.Sort,
                                page = search.Page + 1,
                                search.PageSize
                            })
                            : null,
                        last: _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                        {
                            controller = EndpointName,
                            search.Name,
                            search.BirthDate,
                            search.Sort,
                            page = Math.Max(page.Count, 1),
                            search.PageSize
                        }),
                        total: page.Total);
                actionResult = new OkObjectResult(response);
            }
            else
            {
                actionResult = new NotFoundResult();
            }

            return actionResult;
        }

        /// <summary>
        /// Delete a subject resource by its id
        /// </summary>
        /// <param name="id">id of the subject resource to delete.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(typeof(ErrorObject), Status400BadRequest)]
        public async Task<IActionResult> Delete([RequireNonDefault] SubjectId id, CancellationToken ct = default)
        {
            DeleteCommandResult result = await _mediator.Send(new DeletePatientInfoByIdCommand(id), ct)
                .ConfigureAwait(false);

            return result switch
            {
                DeleteCommandResult.Done => new NoContentResult(),
                DeleteCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                DeleteCommandResult.Failed_NotFound => new NotFoundResult(),
                DeleteCommandResult.Failed_Conflict => new StatusCodeResult(Status409Conflict),
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, $"Unexpected value <{result}> for {nameof(DeleteCommandResult)}"),
            };
        }

        /// <summary>
        /// Retrieves a page of blood pressures for a subject
        /// </summary>
        /// <param name="id">id of the subject which</param>
        /// <param name="pagination">pagination</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{id}/bloodpressures")]
        [HttpHead("{id}/bloodpressures")]
        public async Task<IActionResult> GetBloodPressures([RequireNonDefault] SubjectId id, [FromQuery, RequireNonDefault] PaginationConfiguration pagination, CancellationToken ct = default)
        {
            GetSubjectInfoByIdQuery query = new(id);
            Option<SubjectInfo> result = await _mediator.Send(query, ct)
                .ConfigureAwait(false);

            return result.Match<IActionResult>(
                some: _ =>
                {
                    pagination.PageSize = Math.Min(pagination.PageSize, _apiOptions.Value.MaxPageSize);
                    return new RedirectToRouteResult(RouteNames.DefaultSearchResourcesApi, new
                    {
                        controller = BloodPressuresController.EndpointName,
                        subjectId = id,
                        pagination.Page,
                        pagination.PageSize

                    })
                    { PreserveMethod = true };
                },
                none: () => new NotFoundResult()
            );
        }

        /// <summary>
        /// Create a new blood pressure measure linked to the subject resource with the specified id
        /// </summary>
        /// <param name="id">Patient's id</param>
        /// <param name="newResource">measure of the blood pressure</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of the request</param>
        /// <returns></returns>
        /// <response code="400"><paramref name="id"/> or body was not provided</response>
        /// <response code="404">unknown <paramref name="id"/> was provided</response>
        [HttpPost("{id}/bloodpressures")]
        [ProducesResponseType(typeof(Browsable<BloodPressureInfo>), Status201Created)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> PostBloodPressure([RequireNonDefault] SubjectId id, [FromBody] NewBloodPressureModel newResource, CancellationToken cancellationToken = default)
        {
            CreateBloodPressureInfo createBloodPressureInfo = new()
            {
                SubjectId = id,
                DateOfMeasure = newResource.DateOfMeasure,
                DiastolicPressure = newResource.DiastolicPressure,
                SystolicPressure = newResource.SystolicPressure
            };

            Option<BloodPressureInfo, CreateCommandResult> optionalResource = await _mediator.Send(new CreateBloodPressureInfoForPatientIdCommand(createBloodPressureInfo), cancellationToken)
                .ConfigureAwait(false);

            return optionalResource.Match(
                some: (resource) =>
                {
                    Browsable<BloodPressureInfo> browsableResource = new()
                    {
                        Resource = resource,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = "subject",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, id = resource.SubjectId }),
                                Method = "GET"
                            },
                            new Link
                            {
                                Relation = LinkRelation.Self,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = BloodPressuresController.EndpointName, resource.Id }),
                                Method = "GET"
                            },
                            new Link
                            {
                                Relation = "delete-bloodpressure",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = BloodPressuresController.EndpointName, resource.Id }),
                                Method = "DELETE"
                            },
                        }
                    };

                    return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = BloodPressuresController.EndpointName, resource.Id }, browsableResource);
                },
                none: (createResult) =>
                {
                    return (IActionResult)(createResult switch
                    {
                        CreateCommandResult.Failed_Conflict => new ConflictResult(),
                        CreateCommandResult.Failed_NotFound => new NotFoundResult(),
                        CreateCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                        _ => throw new ArgumentOutOfRangeException($"Unexpected result <{createResult}> when creating a blood pressure resource"),
                    });
                }
            );
        }

        /// <summary>
        /// Creates a new subject resource
        /// </summary>
        /// <param name="newPatient">data for the resource to create</param>
        /// <param name="ct"></param>
        /// <response code="400">data provided does not allow to create the resource</response>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Browsable<SubjectInfo>), Status201Created)]
        public async Task<IActionResult> Post([FromBody] NewSubjectInfo newPatient, CancellationToken ct = default)
        {
            CreateSubjectInfoCommand cmd = new(newPatient);

            SubjectInfo resource = await _mediator.Send(cmd, ct)
                                                  .ConfigureAwait(false);

            Browsable<SubjectInfo> browsableResource = new()
            {
                Resource = resource,
                Links = new[]
                {
                    new Link
                    {
                        Relation = LinkRelation.Self,
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, resource.Id})
                    },
                    new Link
                    {
                        Relation = "bloodpressures",
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                                        new { controller = BloodPressuresController.EndpointName, subjectId = resource.Id, page = 1, pageSize = _apiOptions.Value.DefaultPageSize }) }
                }
            };

            return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id }, browsableResource);
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
        public async Task<IActionResult> Patch([RequireNonDefault] Guid id, [FromBody] JsonPatchDocument<SubjectInfo> changes, CancellationToken ct = default)
        {
            PatchInfo<Guid, SubjectInfo> patchInfo = new()
            {
                Id = id,
                PatchDocument = changes
            };
            PatchCommand<Guid, SubjectInfo> cmd = new(patchInfo);

            ModifyCommandResult result = await _mediator.Send(cmd, ct)
                                                        .ConfigureAwait(false);

            return result switch
            {
                ModifyCommandResult.Done => new NoContentResult(),
                ModifyCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                ModifyCommandResult.Failed_NotFound => new NotFoundResult(),
                ModifyCommandResult.Failed_Conflict => new ConflictResult(),
                _ => throw new NotSupportedException($"Unsupported {result} when patching a subject resource")
            };
        }
    }
}
