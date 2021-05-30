namespace Measures.API.Features.v1.BloodPressures
{
    using DataFilters;

    using Measures.API.Features.v1.Patients;
    using Measures.API.Routing;
    using Measures.CQRS.Commands.BloodPressures;
    using Measures.CQRS.Queries.BloodPressures;
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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using static DataFilters.FilterLogic;
    using static DataFilters.FilterOperator;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="BloodPressureInfo"/> resources
    /// </summary>
    [Route("/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class BloodPressuresController
    {
        private readonly IMediator _mediator;
        private readonly string version;

        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(BloodPressuresController).Replace("Controller", string.Empty);

        /// <summary>
        /// Helper to build URLs
        /// </summary>
        private readonly LinkGenerator _urlHelper;

        /// <summary>
        /// Options of the API
        /// </summary>
        private readonly IOptionsSnapshot<MeasuresApiOptions> _apiOptions;

        /// <summary>
        /// Builds a new <see cref="BloodPressuresController"/> instance
        /// </summary>
        /// <param name="urlHelper">Helper class to build URL strings.</param>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="mediator"></param>
        /// <param name="apiVersion"></param>
        public BloodPressuresController(LinkGenerator urlHelper, IOptionsSnapshot<MeasuresApiOptions> apiOptions, IMediator mediator, ApiVersion apiVersion)
        {
            _urlHelper = urlHelper;
            _apiOptions = apiOptions;
            _mediator = mediator;
            version = apiVersion?.ToString();
        }

        /// <summary>
        /// Gets all the resources of the endpoint
        /// </summary>
        /// <param name="page">index of the page of resources to get</param>
        /// <param name="pageSize">Number of resources per page.</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of request</param>
        /// <remarks>
        /// Resources are returned as pages. The <paramref name="pageSize"/>'s value is used has a hint by the server
        /// and there's no garanty that the size of page of result will be equal to the <see cref="PaginationConfiguration.PageSize"/> set in the query.
        /// In particular, the number of resources on a page may be caped by the server.
        /// </remarks>
        /// <response code="200">items of the page</response>
        /// <response code="400"><paramref name="page"/> or <paramref name="pageSize"/> is lower than 1.</response>
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [ProducesResponseType(typeof(GenericPagedGetResponse<Browsable<BloodPressureInfo>>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> Get([Minimum(1)] int page, [Minimum(1)] int pageSize, CancellationToken cancellationToken = default)
        {
            PaginationConfiguration pagination = new() { Page = page, PageSize = Math.Min(pageSize, _apiOptions.Value.MaxPageSize) };

            Page<BloodPressureInfo> result = await _mediator.Send(new GetPageOfBloodPressureInfoQuery(pagination), cancellationToken)
                                                            .ConfigureAwait(false);

            Debug.Assert(result != null, $"{nameof(result)} cannot be null");

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && pagination.Page > 1;

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

            IEnumerable<Browsable<BloodPressureInfo>> resources = result.Entries
                .Select(x => new Browsable<BloodPressureInfo>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = LinkRelation.Self,
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, id = x.Id.Value, version})
                        }
                    }
                });

            GenericPagedGetResponse<Browsable<BloodPressureInfo>> response = new(
                resources,
                firstPageUrl,
                previousPageUrl,
                nextPageUrl,
                lastPageUrl,
                result.Total);

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Gets the <see cref="BloodPressureInfo"/> resource by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to look for</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">The resource was found</response>
        /// <response code="404">Resource not found</response>
        /// <response code="406"><paramref name="id"/> is not a valid <see cref="Guid"/></response>
        [HttpOptions("{id}")]
        [HttpHead("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Browsable<BloodPressureInfo>), Status200OK)]
        public async Task<ActionResult<Browsable<BloodPressureInfo>>> Get([RequireNonDefault] BloodPressureId id, CancellationToken cancellationToken = default)
        {
            Option<BloodPressureInfo> result = await _mediator.Send(new GetBloodPressureInfoByIdQuery(id), cancellationToken)
                                                              .ConfigureAwait(false);

            return result.Match<ActionResult<Browsable<BloodPressureInfo>>>(
                some: bloodPressure =>
                {
                    return new Browsable<BloodPressureInfo>
                    {
                        Resource = bloodPressure,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = LinkRelation.Self,
                                Method = "GET",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, id = bloodPressure.Id.Value, version })
                            },
                            new Link
                            {
                                Relation = "delete",
                                Method = "DELETE",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, id = bloodPressure.Id.Value, version })
                            },
                            new Link
                            {
                                Relation = "patient",
                                Method = "GET",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = PatientsController.EndpointName, id = bloodPressure.PatientId.Value, version })
                            }
                        }
                    };
                },
                none: () => new NotFoundResult()
            );
        }

        ///// <summary>
        ///// Creates a new <see cref="BloodPressureInfo"/> resource.
        ///// </summary>
        ///// <param name="newBloodPressure">data used to create the resource</param>
        ///// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        ///// <response code="201">the resource was created successfully</response>
        ///// <response code="400"><paramref name="newBloodPressure"/> is not valid</response>
        //[HttpPost]
        //[ProducesResponseType(typeof(BrowsableResource<BloodPressureInfo>), 201)]
        //[ProducesResponseType(typeof(ErrorObject), 400)]
        //public async Task<IActionResult> Post([FromBody] CreateBloodPressureInfo newBloodPressure, CancellationToken cancellationToken = default)
        //{
        //    Option<BloodPressureInfo, CreateCommandResult> optionalCreatedResource = await _mediator.Send(new CreateBloodPressureInfoForPatientIdCommand(newBloodPressure), cancellationToken)
        //        .ConfigureAwait(false);

        //    return optionalCreatedResource.Match(
        //        some: (resource) =>
        //        {

        //           BrowsableResource<BloodPressureInfo> browsableResource = new BrowsableResource<BloodPressureInfo>
        //           {
        //               Resource = resource,
        //               Links = new[]
        //               {
        //                    new Link { Relation = "patient", Href = UrlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { id = resource.PatientId }) },
        //                    new Link { Relation = "delete", Href = UrlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { resource.Id }), Method = "DELETE"}
        //               }

        //           };
        //           return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { resource.Id }, browsableResource);
        //        },
        //        none: (result) =>
        //        {
        //            IActionResult actionResult;
        //            switch (result)
        //            {
        //                case CreateCommandResult.Failed_Conflict:
        //                    actionResult = new StatusCodeResult(Status409Conflict);
        //                    break;
        //                case CreateCommandResult.Failed_NotFound:
        //                    actionResult = new NotFoundResult();
        //                    break;
        //                case CreateCommandResult.Failed_Unauthorized:
        //                    actionResult = new UnauthorizedResult();
        //                    break;
        //                default:
        //                    throw new ArgumentOutOfRangeException($"Unexpected <{result}> for {nameof(CreateCommandResult)}");
        //            }

        //            return actionResult;
        //        });

        //}

        // DELETE measures/bloodpressures/B2DD169D-1619-407B-8F3E-F3F1D8DB29A3

        /// <summary>
        /// Delete the <see cref="BloodPressureInfo"/> by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="204">if the operation succeed</response>
        /// <response code="400">if <paramref name="id"/> is empty.</response>
        /// <response code="404">if resource is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([RequireNonDefault] BloodPressureId id, CancellationToken cancellationToken = default)
        {
            DeleteCommandResult result = await _mediator.Send(new DeleteBloodPressureInfoByIdCommand(id), cancellationToken)
                                                        .ConfigureAwait(false);

            return (IActionResult)(result switch
            {
                DeleteCommandResult.Done => new NoContentResult(),
                DeleteCommandResult.Failed_NotFound => new NotFoundResult(),
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, "unexpected delete result"),
            });
        }

        /// <summary>
        /// Search patients resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <param name="cancellationToken">Notfies to cancel the search operation</param>
        /// <remarks>
        /// <para>All criteria are combined as a AND.</para>
        /// <para>
        /// Advanded search :
        /// Several operators that can be used to make an advanced search :
        /// '*' : match zero or more characters in a string property.
        /// </para>
        /// <para>
        ///     // GET api/BloodPressures/Search?Firstname=Bruce
        ///     will match all resources which have exactly 'Bruce' in the Firstname property
        /// </para>
        /// <para>
        ///     `GET api/BloodPressures/Search?Firstname=B*e`
        ///     will match all resources which starts with 'B' and ends with 'e'.
        /// </para>
        /// <para>'?' : match exactly one charcter in a string property.</para>
        /// <para>'!' : negate a criteria</para>
        /// <example>
        ///     // GET api/BloodPressures/Search?Firstname=!Bruce
        ///     will match all resources where Firstname is not Bruce
        /// </example>
        ///     
        /// </remarks>
        /// <response code="200">Array of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="400">one the search criteria is not valid</response>
        /// <response code="404">requested page is out of result bound</response>
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [ProducesResponseType(typeof(GenericPagedGetResponse<Browsable<BloodPressureInfo>>), Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery, RequireNonDefault] SearchBloodPressureInfo search, CancellationToken cancellationToken = default)
        {
            IList<IFilter> filters = new List<IFilter>();

            if (search.From.HasValue)
            {
                filters.Add(new MultiFilter
                {
                    Logic = Or,
                    Filters = new[]
                    {
                        new Filter(field: nameof(BloodPressureInfo.DateOfMeasure),
                                   @operator: EqualTo,
                                   value: search.From.Value.ToInstant()),
                        new Filter(field: nameof(BloodPressureInfo.DateOfMeasure),
                                   @operator: GreaterThan,
                                   value: search.From.Value.ToInstant())
                    }
                });
            }
            if (search.To.HasValue)
            {
                filters.Add(new MultiFilter
                {
                    Logic = Or,
                    Filters = new[]
                    {
                        new Filter(field: nameof(BloodPressureInfo.DateOfMeasure),
                                   @operator: EqualTo,
                                   value: search.To.Value.ToInstant()),
                        new Filter(field: nameof(BloodPressureInfo.DateOfMeasure),
                                   @operator: LessThan,
                                   value: search.To.Value.ToInstant())
                    }
                });
            }

            if (search.PatientId is not null)
            {
                filters.Add(new Filter(field: nameof(BloodPressureInfo.PatientId),
                                       @operator: EqualTo,
                                       value: search.PatientId));
            }

            SearchQueryInfo<BloodPressureInfo> searchQueryInfo = new()
            {
                Page = search.Page,
                PageSize = Math.Min(search.PageSize, _apiOptions.Value.MaxPageSize),
                Filter = filters.Once()
                    ? filters.Single()
                    : new MultiFilter { Logic = And, Filters = filters },
                Sort = search.Sort?.ToSort<BloodPressureInfo>() ?? new Sort<BloodPressureInfo>(nameof(BloodPressureInfo.DateOfMeasure), SortDirection.Descending)
            };

            Page<BloodPressureInfo> pageOfResult = await _mediator.Send(new SearchQuery<BloodPressureInfo>(searchQueryInfo), cancellationToken)
                                                                  .ConfigureAwait(false);

            IActionResult actionResult;
            if (search.Page <= pageOfResult.Count)
            {
                int count = pageOfResult.Entries.Count();
                bool hasPreviousPage = count > 0 && search.Page > 1;

                string firstPageUrl = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                                               new
                                                               {
                                                                   controller = EndpointName,
                                                                   from = search.From?.ToDateTimeUtc(),
                                                                   to = search.To?.ToDateTimeUtc(),
                                                                   Page = 1,
                                                                   search.PageSize,
                                                                   search.Sort,
                                                                   patientId = search.PatientId?.Value
                                                               });
                string previousPageUrl = hasPreviousPage
                        ? _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                                   new
                                                   {
                                                       controller = EndpointName,
                                                       from = search.From?.ToDateTimeUtc(),
                                                       to = search.To?.ToDateTimeUtc(),
                                                       Page = search.Page - 1,
                                                       search.PageSize,
                                                       search.Sort,
                                                       patientId = search.PatientId?.Value
                                                   })
                        : null;
                string nextPageUrl = search.Page < pageOfResult.Count
                        ? _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                                   new
                                                   {
                                                       controller = EndpointName,
                                                       from = search.From?.ToDateTimeUtc(),
                                                       to = search.To?.ToDateTimeUtc(),
                                                       Page = search.Page + 1,
                                                       search.PageSize,
                                                       search.Sort,
                                                       patientId = search.PatientId?.Value
                                                   })
                        : null;

                string lastPageUrl = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                                              new
                                                              {
                                                                  controller = EndpointName,
                                                                  from = search.From?.ToDateTimeUtc(),
                                                                  to = search.To?.ToDateTimeUtc(),
                                                                  Page = pageOfResult.Count,
                                                                  search.PageSize,
                                                                  search.Sort,
                                                                  patientId = search.PatientId?.Value
                                                              });

                IEnumerable<Browsable<BloodPressureInfo>> resources = pageOfResult.Entries
                    .Select(
                        x => new Browsable<BloodPressureInfo>
                        {
                            Resource = x,
                            Links = new[]
                            {
                                new Link
                                {
                                    Relation = LinkRelation.Self,
                                    Method = "GET",
                                    Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { id = x.Id.Value })
                                }
                            }
                        });

                GenericPagedGetResponse<Browsable<BloodPressureInfo>> reponse = new(
                    resources,
                    first: firstPageUrl,
                    previous: previousPageUrl,
                    next: nextPageUrl,
                    last: lastPageUrl,
                    total: pageOfResult.Total);

                actionResult = new OkObjectResult(reponse);
            }
            else
            {
                actionResult = new NotFoundResult();
            }

            return actionResult;
        }

        /// <summary>
        /// Partially update a blood pressure resource.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        /// </para>
        /// <para>    // PATCH api/BloodPressures/3594c436-8595-444d-9e6b-2686c4904725</para>
        /// <para>
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/PatientId",
        ///             "from": "string",
        ///             "value": "e1aa24f4-69a8-4d3a-aca9-ec15c6910dc9"
        ///       }
        ///     ]
        /// </para>
        /// <para>The set of changes to apply will be applied atomically. </para>
        /// 
        /// </remarks>
        /// <param name="id">id of the resource to update.</param>
        /// <param name="changes">set of changes to apply to the resource.</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="204">The resource was successfully patched.</response>
        /// <response code="400">Changes are not valid for the selected resource.</response>
        /// <response code="404">Resource to "PATCH" not found, the patient resource was not found</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ErrorObject), 400)]
        public async Task<IActionResult> Patch([RequireNonDefault] BloodPressureId id, [FromBody] JsonPatchDocument<BloodPressureInfo> changes, CancellationToken cancellationToken = default)
        {
            PatchInfo<BloodPressureId, BloodPressureInfo> info = new()
            {
                Id = id,
                PatchDocument = changes
            };

            PatchCommand<BloodPressureId, BloodPressureInfo> cmd = new(info);

            ModifyCommandResult result = await _mediator.Send(cmd, cancellationToken)
                .ConfigureAwait(false);

            IActionResult actionResult;
            switch (result)
            {
                case ModifyCommandResult.Done:
                    actionResult = new NoContentResult();
                    break;
                case ModifyCommandResult.Failed_Unauthorized:
                    actionResult = new UnauthorizedResult();
                    break;
                case ModifyCommandResult.Failed_NotFound:
                    actionResult = new NotFoundResult();
                    break;
                case ModifyCommandResult.Failed_Conflict:
                    actionResult = new StatusCodeResult(Status409Conflict);
                    break;
                default:
#if DEBUG

                    throw new ArgumentOutOfRangeException();
#else
                    actionResult = new StatusCodeResult(Status500InternalServerError);
                    break;
#endif
            }

            return actionResult;
        }
    }
}
