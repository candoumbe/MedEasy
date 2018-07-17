using Measures.API.Features.Patients;
using Measures.API.Routing;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Queries.BloodPressures;
using Measures.DTO;
using MedEasy.Core.Attributes;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO;
using MedEasy.DTO.Search;
using MedEasy.RestObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Optional;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MedEasy.Data.DataFilterLogic;
using static MedEasy.Data.DataFilterOperator;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Measures.API.Features.BloodPressures
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="BloodPressureInfo"/> resources
    /// </summary>
    [Route("measures/[controller]")]
    public class BloodPressuresController
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(BloodPressuresController).Replace("Controller", string.Empty);

        /// <summary>
        /// Helper to build URLs
        /// </summary>
        private IUrlHelper _urlHelper;

        /// <summary>
        /// Options of the API
        /// </summary>
        public IOptionsSnapshot<MeasuresApiOptions> ApiOptions { get; }



        /// <summary>
        /// Builds a new <see cref="BloodPressuresController"/> instance
        /// </summary>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="mediator"></param>
        /// <param name="urlHelper">Helper class to build URL strings.</param>
        public BloodPressuresController(IUrlHelper urlHelper, IOptionsSnapshot<MeasuresApiOptions> apiOptions,
            IMediator mediator)
        {
            _urlHelper = urlHelper;
            ApiOptions = apiOptions;
            _mediator = mediator;
        }



        /// <summary>
        /// Gets all the resources of the endpoint
        /// </summary>
        /// <param name="pagination">index of the page of resources to get</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of request</param>
        /// <remarks>
        /// Resources are returned as pages. The <paramref name="pagination"/>'s <see cref="PaginationConfiguration.PageSize"/> value is used has a hint by the server
        /// and there's no garanty that the size of page of result will be equal to the <see cref="PaginationConfiguration.PageSize"/> set in the query.
        /// In particular, the number of resources on a page may be caped by the server.
        /// </remarks>
        /// <response code="200">items of the page</response>
        /// <response code="400"><paramref name="pagination"/> is not correct.</response>
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>), 200)]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration pagination, CancellationToken cancellationToken = default)
        {

            pagination.PageSize = Math.Min(pagination.PageSize, ApiOptions.Value.MaxPageSize);
            Page<BloodPressureInfo> result = await _mediator.Send(new GetPageOfBloodPressureInfoQuery(pagination), cancellationToken)
                .ConfigureAwait(false);

            Debug.Assert(result != null, $"{nameof(result)} cannot be null");

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && pagination.Page > 1;

            string firstPageUrl = _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page - 1 })
                    : null;

            string nextPageUrl = pagination.Page < result.Count
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page + 1 })
                    : null;
            string lastPageUrl = result.Count > 0
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = result.Count })
                    : firstPageUrl;


            IEnumerable<BrowsableResource<BloodPressureInfo>> resources = result.Entries
                .Select(x => new BrowsableResource<BloodPressureInfo>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = LinkRelation.Self,
                            Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, x.Id})
                        }
                    }
                });

            IGenericPagedGetResponse<BrowsableResource<BloodPressureInfo>> response = new GenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>(
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
        [ProducesResponseType(typeof(BrowsableResource<BloodPressureInfo>), 200)]
        public async Task<IActionResult> Get([RequireNonDefault] Guid id, CancellationToken cancellationToken = default)
        {
            Option<BloodPressureInfo> result = await _mediator.Send(new GetBloodPressureInfoByIdQuery(id), cancellationToken)
                    .ConfigureAwait(false);

            IActionResult actionResult = result.Match<IActionResult>(
                some: bloodPressure =>
                {
                    BrowsableResource<BloodPressureInfo> browsableResource = new BrowsableResource<BloodPressureInfo>
                    {
                        Resource = bloodPressure,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = LinkRelation.Self,
                                Method = "GET",
                                Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, bloodPressure.Id })
                            },
                            new Link
                            {
                                Relation = "delete",
                                Method = "DELETE",
                                Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, bloodPressure.Id })
                            },
                            new Link
                            {
                                Relation = "patient",
                                Method = "GET",
                                Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = PatientsController.EndpointName, id = bloodPressure.PatientId })
                            }
                        }
                    };
                    return new OkObjectResult(browsableResource);
                },
                none: () => new NotFoundResult()
            );
            return actionResult;
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
        //                    new Link { Relation = "patient", Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { id = resource.PatientId }) },
        //                    new Link { Relation = "delete", Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { resource.Id }), Method = "DELETE"}
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
        public async Task<IActionResult> Delete([RequireNonDefault] Guid id, CancellationToken cancellationToken = default)
        {
            DeleteCommandResult result = await _mediator.Send(new DeleteBloodPressureInfoByIdCommand(id), cancellationToken)
                .ConfigureAwait(false);

            IActionResult actionResult;
            switch (result)
            {
                case DeleteCommandResult.Done:
                    actionResult = new NoContentResult();
                    break;
                case DeleteCommandResult.Failed_NotFound:
                    actionResult = new NotFoundResult();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unexpected delete result");
            }

            return actionResult;


        }

        /// <summary>
        /// Search patients resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <param name="cancellationToken">Notfies to cancel the search operation</param>
        /// <remarks>
        /// All criteria are combined as a AND.
        /// 
        /// Advanded search :
        /// Several operators that can be used to make an advanced search :
        /// '*' : match zero or more characters in a string property.
        /// 
        ///     // GET api/BloodPressures/Search?Firstname=Bruce
        ///     will match all resources which have exactly 'Bruce' in the Firstname property
        ///     
        ///     // GET api/BloodPressures/Search?Firstname=B*e
        ///     will match match all resources which starts with 'B' and ends with 'e'.
        /// 
        /// '?' : match exactly one charcter in a string property.
        /// 
        /// '!' : negate a criteria
        /// 
        ///     // GET api/BloodPressures/Search?Firstname=!Bruce
        ///     will match all resources where Firstname is not "Bruce"
        ///     
        /// </remarks>
        /// <response code="200">Array of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="400">one the search criteria is not valid</response>
        /// <response code="404">requested page is out of result bound</response>
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>), 200)]
        [ProducesResponseType(typeof(ErrorObject), 400)]
        public async Task<IActionResult> Search([FromQuery, RequireNonDefault] SearchBloodPressureInfo search, CancellationToken cancellationToken = default)
        {
            IList<IDataFilter> filters = new List<IDataFilter>();

            if (search.From.HasValue)
            {
                filters.Add(new DataCompositeFilter
                {
                    Logic = Or,
                    Filters = new[] {
                        new DataFilter(field: nameof(BloodPressureInfo.DateOfMeasure), @operator: EqualTo, value: search.From.Value),
                        new DataFilter(field: nameof(BloodPressureInfo.DateOfMeasure), @operator: GreaterThan, value: search.From.Value)
                    }
                });
            }
            if (search.To.HasValue)
            {
                filters.Add(new DataCompositeFilter
                {
                    Logic = Or,
                    Filters = new[] {
                        new DataFilter(field: nameof(BloodPressureInfo.DateOfMeasure), @operator: EqualTo, value: search.To.Value),
                        new DataFilter(field: nameof(BloodPressureInfo.DateOfMeasure), @operator: LessThan, value: search.To.Value)
                    }
                });
            }

            if (search.PatientId.HasValue)
            {
                filters.Add(new DataFilter(field: nameof(BloodPressureInfo.PatientId), @operator: EqualTo, value: search.PatientId.Value));
            }

            SearchQueryInfo<BloodPressureInfo> searchQueryInfo = new SearchQueryInfo<BloodPressureInfo>
            {
                Page = search.Page,
                PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize),
                Filter = filters.Count() == 1
                    ? filters.Single()
                    : new DataCompositeFilter { Logic = And, Filters = filters },
                Sorts = (search.Sort ?? $"-{nameof(BloodPressureInfo.UpdatedDate)}").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x =>
                        {
                            x = x.Trim();
                            Sort sort;
                            if (x.StartsWith("-"))
                            {
                                x = x.Substring(1);
                                sort = new Sort { Direction = MedEasy.Data.SortDirection.Descending, Expression = x.ToLambda<BloodPressureInfo>() };
                            }
                            else
                            {
                                sort = new Sort { Direction = MedEasy.Data.SortDirection.Ascending, Expression = x.ToLambda<BloodPressureInfo>() };
                            }
                            return sort;
                        })
            };

            Page<BloodPressureInfo> pageOfResult = await _mediator.Send(new SearchQuery<BloodPressureInfo>(searchQueryInfo), cancellationToken)
                .ConfigureAwait(false);

            IActionResult actionResult;
            if (search.Page <= pageOfResult.Count)
            {
                int count = pageOfResult.Entries.Count();
                bool hasPreviousPage = count > 0 && search.Page > 1;

                string firstPageUrl = _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, 
                    new { controller = EndpointName, search.From, search.To, Page = 1, search.PageSize, search.Sort, search.PatientId });
                string previousPageUrl = hasPreviousPage
                        ? _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.From, search.To, Page = search.Page - 1, search.PageSize, search.Sort, search.PatientId })
                        : null;
                string nextPageUrl = search.Page < pageOfResult.Count
                        ? _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.From, search.To, Page = search.Page + 1, search.PageSize, search.Sort, search.PatientId })
                        : null;

                string lastPageUrl = _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.From, search.To, Page = pageOfResult.Count, search.PageSize, search.Sort, search.PatientId });

                IEnumerable<BrowsableResource<BloodPressureInfo>> resources = pageOfResult.Entries
                    .Select(
                        x => new BrowsableResource<BloodPressureInfo>
                        {
                            Resource = x,
                            Links = new[]
                            {
                                new Link
                                {
                                    Relation = LinkRelation.Self,
                                    Method = "GET",
                                    Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { x.Id })
                                }
                            }
                        });

                IGenericPagedGetResponse<BrowsableResource<BloodPressureInfo>> reponse = new GenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>(
                    resources,
                    first: firstPageUrl,
                    previous: previousPageUrl,
                    next: nextPageUrl,
                    last: lastPageUrl,
                    count: pageOfResult.Total);

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
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        ///
        ///     // PATCH api/BloodPressures/3594c436-8595-444d-9e6b-2686c4904725
        ///     
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/PatientId",
        ///             "from": "string",
        ///             "value": "e1aa24f4-69a8-4d3a-aca9-ec15c6910dc9"
        ///       }
        ///     ]
        /// 
        /// The set of changes to apply will be applied atomically. 
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
        public async Task<IActionResult> Patch([RequireNonDefault] Guid id, [FromBody] JsonPatchDocument<BloodPressureInfo> changes, CancellationToken cancellationToken = default)
        {
            PatchInfo<Guid, BloodPressureInfo> info = new PatchInfo<Guid, BloodPressureInfo>
            {
                Id = id,
                PatchDocument = changes
            };

            PatchCommand<Guid, BloodPressureInfo> cmd = new PatchCommand<Guid, BloodPressureInfo>(info);

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
