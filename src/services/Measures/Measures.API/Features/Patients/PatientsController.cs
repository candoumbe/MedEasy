using Measures.API.Features.BloodPressures;
using Measures.API.Features.Patients;
using Measures.API.Routing;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Commands.Patients;
using Measures.CQRS.Queries.BloodPressures;
using Measures.CQRS.Queries.Patients;
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
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Measures.API.Features.Patients
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="PatientInfo"/> resources
    /// </summary>
    [Controller]
    [Route("measures/[controller]")]
    public class PatientsController
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(PatientsController).Replace("Controller", string.Empty);

        /// <summary>
        /// Helper to build URLs
        /// </summary>
        public IUrlHelper UrlHelper { get; }

        /// <summary>
        /// Options of the API
        /// </summary>
        public IOptionsSnapshot<MeasuresApiOptions> ApiOptions { get; }

        private readonly ClaimsPrincipal _claimsPrincipal;
        


        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance
        /// </summary>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="mediator"></param>
        /// <param name="claimsPrincipal"></param>
        /// <param name="urlHelper">Helper class to build URL strings.</param>
        public PatientsController(IUrlHelper urlHelper, IOptionsSnapshot<MeasuresApiOptions> apiOptions, IMediator mediator, ClaimsPrincipal claimsPrincipal)
        {
            UrlHelper = urlHelper;
            ApiOptions = apiOptions;
            _mediator = mediator;
            _claimsPrincipal = claimsPrincipal;
        }



        /// <summary>
        /// Gets all the resources of the endpoint
        /// </summary>
        /// <param name="pagination">index of the page of resources to get</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of the request</param>
        /// <remarks>
        /// Resources are returned as pages. The <paramref name="pagination"/>'s value is used has a hint by the server
        /// and there's no garanty that the size of page of result will be equal to the <paramref name="pagination"/>' property <see cref="PaginationConfiguration.PageSize"/> set in the query.
        /// In particular, the number of resources on a page may be caped by the server.
        /// </remarks>
        /// <response code="200">items of the page</response>
        /// <response code="400"><paramref name="pagination"/><see cref="PaginationConfiguration.Page"/> or <paramref name="pagination"/><see cref="PaginationConfiguration.PageSize"/> is negative or zero</response>
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<PatientInfo>>), 200)]
        public async Task<IActionResult> Get([FromQuery, RequireNonDefault] PaginationConfiguration pagination, CancellationToken cancellationToken = default)
        {

            pagination.PageSize = Math.Min(pagination.PageSize, ApiOptions.Value.MaxPageSize);
            Page<PatientInfo> result = await _mediator.Send(new GetPageOfPatientInfoQuery(pagination), cancellationToken)
                .ConfigureAwait(false);

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && pagination.Page > 1;

            string firstPageUrl = UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page - 1 })
                    : null;

            string nextPageUrl = pagination.Page < result.Count
                    ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page + 1 })
                    : null;
            string lastPageUrl = result.Count > 0
                    ? UrlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = result.Count })
                    : firstPageUrl;


            IEnumerable<BrowsableResource<PatientInfo>> resources = result.Entries
                .Select(x => new BrowsableResource<PatientInfo>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = LinkRelation.Self,
                            Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, x.Id})
                        }
                    }
                });

            GenericPagedGetResponse<BrowsableResource<PatientInfo>> response = new GenericPagedGetResponse<BrowsableResource<PatientInfo>>(
                resources,
                firstPageUrl,
                previousPageUrl,
                nextPageUrl,
                lastPageUrl,
                result.Total);


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
        [HttpOptions("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BrowsableResource<PatientInfo>), 200)]
        public async Task<IActionResult> Get([RequireNonDefault] Guid id, CancellationToken cancellationToken = default)
        {

            Option<PatientInfo> result = await _mediator.Send(new GetPatientInfoByIdQuery(id), cancellationToken)
                .ConfigureAwait(false);

            IActionResult actionResult = result.Match<IActionResult>(
                some: resource =>
                {
                    BrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
                    {
                        Resource = resource,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = LinkRelation.Self,
                                Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id }),
                                Method = "GET"
                            },
                            new Link
                            {
                                Relation = "delete",
                                Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id }),
                                Method = "DELETE"
                            },
                            new Link
                            {
                                Relation = BloodPressuresController.EndpointName.ToLowerKebabCase(),
                                Method = "GET",
                                Href =  UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = BloodPressuresController.EndpointName, patientId = resource.Id })
                            }
                        }
                    };
                    return new OkObjectResult(browsableResource);
                },
                none: () => new NotFoundResult()
            );

            return actionResult;

        }

        /// <summary>
        /// Search doctors resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of the request.</param>
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
        /// <response code="404">The requested page is out of results page count bounds.</response>
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [HttpOptions("[action]")]
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<PatientInfo>>), Status200OK)]
        [ProducesResponseType(typeof(ErrorObject), Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery, RequireNonDefault]SearchPatientInfo search, CancellationToken cancellationToken = default)
        {

            IList<IDataFilter> filters = new List<IDataFilter>();
            if (!string.IsNullOrEmpty(search.Firstname))
            {
                filters.Add($"{(nameof(search.Firstname))}={search.Firstname}".ToFilter<PatientInfo>());
            }

            if (!string.IsNullOrEmpty(search.Lastname))
            {
                filters.Add($"{(nameof(search.Lastname))}={search.Lastname}".ToFilter<PatientInfo>());
            }


            SearchQueryInfo<PatientInfo> searchQuery = new SearchQueryInfo<PatientInfo>
            {
                Filter = filters.Count() == 1
                    ? filters.Single()
                    : new DataCompositeFilter { Logic = DataFilterLogic.And, Filters = filters },
                Page = search.Page,
                PageSize = search.PageSize,
                Sorts = (search.Sort ?? $"-{nameof(PatientInfo.UpdatedDate)}").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x =>
                        {
                            x = x.Trim();
                            Sort sort;
                            if (x.StartsWith("-"))
                            {
                                x = x.Substring(1);
                                sort = new Sort { Direction = MedEasy.Data.SortDirection.Descending, Expression = x.ToLambda<PatientInfo>() };
                            }
                            else
                            {
                                sort = new Sort { Direction = MedEasy.Data.SortDirection.Ascending, Expression = x.ToLambda<PatientInfo>() };
                            }

                            return sort;
                        })
            };
            Page<PatientInfo> page = await _mediator.Send(new SearchQuery<PatientInfo>(searchQuery), cancellationToken)
                .ConfigureAwait(false);
            IActionResult actionResult;

            if (searchQuery.Page <= page.Count)
            {
                GenericPagedGetResponse<BrowsableResource<PatientInfo>> response = new GenericPagedGetResponse<BrowsableResource<PatientInfo>>(
                        items: page.Entries.Select(x => new BrowsableResource<PatientInfo>
                        {
                            Resource = x,
                            Links = new[] {
                            new Link
                            {
                                Method = "GET",
                                Relation = LinkRelation.Self,
                                Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, x.Id })
                            }
                            }
                        }),
                        count: page.Total,
                        first: UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
                        {
                            controller = EndpointName,
                            search.Firstname,
                            search.Lastname,
                            search.BirthDate,
                            search.Sort,
                            page = 1,
                            search.PageSize
                        }),
                        previous: search.Page > 1
                            ? UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
                            {
                                controller = EndpointName,
                                search.Firstname,
                                search.Lastname,
                                search.BirthDate,
                                search.Sort,
                                page = search.Page - 1,
                                search.PageSize
                            })
                            : null,
                        next: page.Count > search.Page
                            ? UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
                            {
                                controller = EndpointName,
                                search.Firstname,
                                search.Lastname,
                                search.BirthDate,
                                search.Sort,
                                page = search.Page + 1,
                                search.PageSize
                            })
                            : null,
                        last: UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
                        {
                            controller = EndpointName,
                            search.Firstname,
                            search.Lastname,
                            search.BirthDate,
                            search.Sort,
                            page = Math.Max(page.Count, 1),
                            search.PageSize
                        })
                    );
                actionResult = new OkObjectResult(response);
            }
            else
            {
                actionResult = new NotFoundResult();
            }

            return actionResult;

        }

        /// <summary>
        /// Delete a patient resource by its id
        /// </summary>
        /// <param name="id">id of the patient resource to delete.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(typeof(ErrorObject), Status400BadRequest)]
        public async Task<IActionResult> Delete([RequireNonDefault] Guid id, CancellationToken ct = default)
        {
            DeleteCommandResult result = await _mediator.Send(new DeletePatientInfoByIdCommand(id), ct)
                .ConfigureAwait(false);

            IActionResult actionResult;
            switch (result)
            {
                case DeleteCommandResult.Done:
                    actionResult = new NoContentResult();
                    break;
                case DeleteCommandResult.Failed_Unauthorized:
                    actionResult = new UnauthorizedResult();
                    break;
                case DeleteCommandResult.Failed_NotFound:
                    actionResult = new NotFoundResult();
                    break;
                case DeleteCommandResult.Failed_Conflict:
                    actionResult = new StatusCodeResult(Status409Conflict);
                    break;
                default:
#if DEBUG
                    throw new ArgumentOutOfRangeException($"Unexpected value <{result}> for {nameof(DeleteCommandResult)}");
#else
                    actionResult = new StatusCodeResult(Status500InternalServerError);
                    break;
#endif
            }
            return actionResult;
        }

        /// <summary>
        /// Retrieves a page of blood pressures for a patient
        /// </summary>
        /// <param name="id">id of the patient which</param>
        /// <param name="pagination">pagination</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{id}/bloodpressures")]
        [HttpHead("{id}/bloodpressures")]
        public async Task<IActionResult> GetBloodPressures([FromQuery, RequireNonDefault] Guid id, [FromQuery, RequireNonDefault]PaginationConfiguration pagination, CancellationToken ct = default)
        {

            GetPatientInfoByIdQuery query = new GetPatientInfoByIdQuery(id);
            Option<PatientInfo> result = await _mediator.Send(query, ct)
                .ConfigureAwait(false);

            return result.Match<IActionResult>(
                some: (patient) =>
                {
                    pagination.PageSize = Math.Min(pagination.PageSize, ApiOptions.Value.MaxPageSize);
                    return new RedirectToRouteResult(RouteNames.DefaultSearchResourcesApi, new
                    {
                        controller = BloodPressuresController.EndpointName,
                        patientId = id,
                        pagination.Page,
                        pagination.PageSize

                    })
                    { PreserveMethod = true };

                },
                none: () => new NotFoundResult()
            );

        }

        /// <summary>
        /// Create a new blood pressure measure linked to the patient resource with the specified id
        /// </summary>
        /// <param name="id">Patient's id</param>
        /// <param name="newResource">measure of the blood pressure</param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns></returns>
        /// <response code="400"><paramref name="id"/> or body was not provided</response>
        /// <response code="404">unknown <paramref name="id"/> was provided</response>
        [HttpPost("{id}/bloodpressures")]
        [ProducesResponseType(typeof(BrowsableResource<BloodPressureInfo>), Status201Created)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(typeof(ErrorObject), Status400BadRequest)]
        public async Task<IActionResult> PostBloodPressure([RequireNonDefault] Guid id, [FromBody, RequireNonDefault]NewBloodPressureModel newResource, CancellationToken ct = default)
        {
            CreateBloodPressureInfo createBloodPressureInfo = new CreateBloodPressureInfo
            {
                PatientId = id,
                DateOfMeasure = newResource.DateOfMeasure,
                DiastolicPressure = newResource.DiastolicPressure,
                SystolicPressure = newResource.SystolicPressure
            };


            Option<BloodPressureInfo, CreateCommandResult> optionalResource = await _mediator.Send(new CreateBloodPressureInfoForPatientIdCommand(createBloodPressureInfo), ct)
                .ConfigureAwait(false);


            return optionalResource.Match(
                some: (resource) =>
                {
                    BrowsableResource<BloodPressureInfo> browsableResource = new BrowsableResource<BloodPressureInfo>
                    {
                        Resource = resource,
                        Links = new[]
                        {
                            new Link { Relation = "patient", Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, id = resource.PatientId }), Method = "GET"},
                            new Link { Relation = LinkRelation.Self, Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = BloodPressuresController.EndpointName, resource.Id }), Method = "GET"},
                            new Link { Relation = "delete-bloodpressure", Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = BloodPressuresController.EndpointName, resource.Id }), Method = "DELETE"},
                        }
                    };

                    return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = BloodPressuresController.EndpointName, resource.Id }, browsableResource);
                },
                none: (createResult) =>
                {
                   IActionResult actionResult;
                   switch (createResult)
                   {
                       case CreateCommandResult.Failed_Conflict:
                           actionResult = new StatusCodeResult(Status409Conflict);
                           break;
                       case CreateCommandResult.Failed_NotFound:
                           actionResult = new NotFoundResult();
                           break;
                       case CreateCommandResult.Failed_Unauthorized:
                           actionResult = new UnauthorizedResult();
                           break;
                       default:
                           throw new ArgumentOutOfRangeException($"Unexpected result <{createResult}> when creating a blood pressure resource");
                   }

                   return actionResult;
                }
            );


        }

        /// <summary>
        /// Creates a new patient resource
        /// </summary>
        /// <param name="newPatient">data for the resource to create</param>
        /// <param name="ct"></param>
        /// <response code="400">data provided does not allow to create the resource</response>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BrowsableResource<PatientInfo>), Status201Created)]
        [ProducesResponseType(typeof(ErrorObject), Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] NewPatientInfo newPatient, CancellationToken ct = default)
        {


            CreatePatientInfoCommand cmd = new CreatePatientInfoCommand(newPatient);

            PatientInfo resource = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            BrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
            {
                Resource = resource,
                Links = new[]
                {
                    new Link { Relation = LinkRelation.Self, Method = "GET", Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {resource.Id}) },
                    new Link { Relation = "bloodpressures", Method = "GET", Href = UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new {controller = BloodPressuresController.EndpointName, patientId = resource.Id, page = 1, pageSize = ApiOptions.Value.DefaultPageSize }) }
                }

            };

            return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { resource.Id }, browsableResource);
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
        [ProducesResponseType(typeof(ErrorObject), Status400BadRequest)]
        public async Task<IActionResult> Patch([RequireNonDefault]Guid id, [FromBody] JsonPatchDocument<PatientInfo> changes, CancellationToken ct = default)
        {
            PatchInfo<Guid, PatientInfo> patchInfo = new PatchInfo<Guid, PatientInfo>
            {
                Id = id,
                PatchDocument = changes
            };
            PatchCommand<Guid, PatientInfo> cmd = new PatchCommand<Guid, PatientInfo>(patchInfo);

            ModifyCommandResult result = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);
            IActionResult actionResult = null;
            switch (result)
            {
                case ModifyCommandResult.Done:
                    actionResult = new NoContentResult();
                    break;
                case ModifyCommandResult.Failed_Unauthorized:
                    break;
                case ModifyCommandResult.Failed_NotFound:
                    actionResult = new NotFoundResult();
                    break;
                case ModifyCommandResult.Failed_Conflict:
                    break;
                default:
                    break;
            }
            return actionResult;
        }
    }
}
