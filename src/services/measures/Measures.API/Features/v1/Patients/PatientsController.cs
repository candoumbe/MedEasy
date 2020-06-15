using DataFilters;

using Forms;

using Measures.API.Features.Patients;
using Measures.API.Features.v1.BloodPressures;
using Measures.API.Routing;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Commands.Patients;
using Measures.CQRS.Queries.Patients;
using Measures.DTO;

using MedEasy.Attributes;
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

using static Microsoft.AspNetCore.Http.StatusCodes;
using static Forms.LinkRelation;
using System.ComponentModel.DataAnnotations;
using Measures.Models.v1;
using Measures.Objects;
using MedEasy.Core.Results;
using System.Reflection.Metadata.Ecma335;

namespace Measures.API.Features.v1.Patients
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="PatientInfo"/> resources
    /// </summary>
    [Route("/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
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
        private readonly LinkGenerator _urlHelper;

        /// <summary>
        /// Current version of the endpoint
        /// </summary>
        private readonly ApiVersion _apiVersion;

        /// <summary>
        /// Options of the API
        /// </summary>
        private readonly IOptionsSnapshot<MeasuresApiOptions> _apiOptions;

        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance
        /// </summary>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="urlHelper">Helper class to build URL strings.</param>
        /// <param name="mediator"></param>
        public PatientsController(LinkGenerator urlHelper, IOptionsSnapshot<MeasuresApiOptions> apiOptions, IMediator mediator, ApiVersion apiVersion)
        {
            _urlHelper = urlHelper;
            _apiOptions = apiOptions;
            _mediator = mediator;
            _apiVersion = apiVersion;
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
        [ProducesResponseType(typeof(GenericPageModel<Browsable<PatientInfo>>), Status200OK)]
        public async Task<GenericPageModel<Browsable<PatientInfo>>> Get([FromQuery, RequireNonDefault] PaginationConfiguration pagination, CancellationToken cancellationToken = default)
        {
            pagination.PageSize = Math.Min(pagination.PageSize, _apiOptions.Value.MaxPageSize);
            Page<PatientInfo> result = await _mediator.Send(new GetPageOfPatientInfoQuery(pagination), cancellationToken)
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

            IEnumerable<Browsable<PatientInfo>> resources = result.Entries
                .Select(x => new Browsable<PatientInfo>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = LinkRelation.Self,
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, x.Id, version})
                        }
                    }
                });

            GenericPageModel<Browsable<PatientInfo>> response = new GenericPageModel<Browsable<PatientInfo>>
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

            return response;
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
        [ProducesResponseType(typeof(Browsable<PatientInfo>), Status200OK)]
        public async Task<ActionResult<Browsable<PatientInfo>>> Get([RequireNonDefault] Guid id, CancellationToken cancellationToken = default)
        {
            Option<PatientInfo> result = await _mediator.Send(new GetPatientInfoByIdQuery(id), cancellationToken)
                .ConfigureAwait(false);

            return result.Match<ActionResult<Browsable<PatientInfo>>>(
                some: resource =>
                {
                    string version = _apiVersion.ToString();
                    return new Browsable<PatientInfo>
                    {
                        Resource = resource,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = LinkRelation.Self,
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
                            },
                            new Link
                            {
                                Relation = BloodPressuresController.EndpointName.Slugify(),
                                Method = "GET",
                                Href =  _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                                                 new {
                                                                     controller = BloodPressuresController.EndpointName,
                                                                     patientId = resource.Id,
                                                                     version
                                                                 })
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
        /// Retrieves a page of blood pressures for a patient
        /// </summary>
        /// <param name="id">id of the patient which</param>
        /// <param name="pagination">pagination</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{id}/bloodpressures")]
        [HttpHead("{id}/bloodpressures")]
        public async Task<IActionResult> GetBloodPressures([RequireNonDefault] Guid id, [FromQuery, RequireNonDefault] PaginationConfiguration pagination, CancellationToken ct = default)
        {
            GetPatientInfoByIdQuery query = new GetPatientInfoByIdQuery(id);
            Option<PatientInfo> result = await _mediator.Send(query, ct)
                .ConfigureAwait(false);

            return result.Match<IActionResult>(
                some: _ =>
                {
                    pagination.PageSize = Math.Min(pagination.PageSize, _apiOptions.Value.MaxPageSize);
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
        /// <param name="cancellationToken">Notifies to cancel the execution of the request</param>
        /// <returns></returns>
        /// <response code="400"><paramref name="id"/> or body was not provided</response>
        /// <response code="404">unknown <paramref name="id"/> was provided</response>
        [HttpPost("{id}/bloodpressures")]
        [ProducesResponseType(typeof(Browsable<BloodPressureInfo>), Status201Created)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> PostBloodPressure([RequireNonDefault] Guid id, [FromBody] NewBloodPressureModel newResource, CancellationToken cancellationToken = default)
        {
            CreateBloodPressureInfo createBloodPressureInfo = new CreateBloodPressureInfo
            {
                PatientId = id,
                DateOfMeasure = newResource.DateOfMeasure,
                DiastolicPressure = newResource.DiastolicPressure,
                SystolicPressure = newResource.SystolicPressure
            };

            Option<BloodPressureInfo, CreateCommandResult> optionalResource = await _mediator.Send(new CreateBloodPressureInfoForPatientIdCommand(createBloodPressureInfo), cancellationToken)
                .ConfigureAwait(false);

            return optionalResource.Match(
                some: (resource) =>
                {
                    string version = _apiVersion.ToString();
                    Browsable<BloodPressureInfo> browsableResource = new Browsable<BloodPressureInfo>
                    {
                        Resource = resource,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = "patient",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, id = resource.PatientId, version }),
                                Method = "GET"
                            },
                            new Link
                            {
                                Relation = LinkRelation.Self,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = BloodPressuresController.EndpointName, resource.Id, version }),
                                Method = "GET"
                            },
                            new Link
                            {
                                Relation = "delete-bloodpressure",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = BloodPressuresController.EndpointName, resource.Id, version }),
                                Method = "DELETE"
                            },
                        }
                    };

                    return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = BloodPressuresController.EndpointName, resource.Id, version }, browsableResource);
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
        /// Creates a new patient resource
        /// </summary>
        /// <param name="newPatient">data for the resource to create</param>
        /// <param name="ct"></param>
        /// <response code="400">data provided does not allow to create the resource</response>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Browsable<PatientInfo>), Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] NewPatientInfo newPatient, CancellationToken ct = default)
        {
            CreatePatientInfoCommand cmd = new CreatePatientInfoCommand(newPatient);

            PatientInfo resource = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            string version = _apiVersion.ToString();
            Browsable<PatientInfo> browsableResource = new Browsable<PatientInfo>
            {
                Resource = resource,
                Links = new[]
                {
                    new Link
                    {
                        Relation = LinkRelation.Self,
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, resource.Id, version})
                    },
                    new Link
                    {
                        Relation = "bloodpressures",
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
        public async Task<IActionResult> Patch([RequireNonDefault] Guid id, [FromBody] JsonPatchDocument<PatientInfo> changes, CancellationToken ct = default)
        {
            PatchInfo<Guid, PatientInfo> patchInfo = new PatchInfo<Guid, PatientInfo>
            {
                Id = id,
                PatchDocument = changes
            };
            PatchCommand<Guid, PatientInfo> cmd = new PatchCommand<Guid, PatientInfo>(patchInfo);

            ModifyCommandResult result = await _mediator.Send(cmd, ct)
                                                        .ConfigureAwait(false);

            return result switch
            {
                ModifyCommandResult.Done => new NoContentResult(),
                ModifyCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                ModifyCommandResult.Failed_NotFound => new NotFoundResult(),
                ModifyCommandResult.Failed_Conflict => new ConflictResult(),
                _ => throw new ArgumentOutOfRangeException(nameof(result), $"Unsupported {result} when patching a patient resource")
            };
        }

        /// <summary>
        /// Gets all measures fot the patient with the specified id
        /// </summary>
        /// <param name="id">id of the patient resource</param>
        /// <param name="measure">name of the measure to retrieve</param>
        /// <param name="page">1-based index of the page of result</param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{id}/{measure}")]
        [HttpHead("{id}/{measure}")]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(typeof(GenericPageModel<Browsable<GenericMeasureModel>>), Status200OK)]
        [ProducesResponseType(typeof(GenericPageModel<Browsable<GenericMeasureModel>>), Status206PartialContent)]
        public async Task<ActionResult<GenericPageModel<Browsable<GenericMeasureModel>>>> GetMeasures([RequireNonDefault] Guid id,
                                                                                                      [RequireNonDefault] string measure,
                                                                                                      [Minimum(1)] int page,
                                                                                                      [Minimum(1)] int pageSize,
                                                                                                      CancellationToken ct = default)
        {
            MeasuresApiOptions options = _apiOptions.Value;
            PaginationConfiguration pagination = new PaginationConfiguration
            {
                Page = page,
                PageSize = Math.Min(pageSize, options.MaxPageSize)
            };
            GetPageOfMeasuresInfoByPatientIdQuery request = new GetPageOfMeasuresInfoByPatientIdQuery((id, measure, pagination));
            Option<Page<GenericMeasureInfo>> maybeResult = await _mediator.Send(request, ct)
                                                                     .ConfigureAwait(false);

            return maybeResult.Match(
                some: page =>
               {
                   string version = _apiVersion.ToString();
                   GenericPageModel<Browsable<GenericMeasureModel>> response = new GenericPageModel<Browsable<GenericMeasureModel>>
                   {
                       Items = page.Entries.Select(m => new Browsable<GenericMeasureModel>
                       {
                           Resource = new GenericMeasureModel
                           {
                               Id = m.Id
                           },
                           Links = new[] {
                               new Link
                               {
                                   Relation = Self,
                                   Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi, new { controller = EndpointName, patientId = m.PatientId, measure, measureId = m.Id, version })
                               },
                               new Link
                               {
                                   Relation = "patient",
                                   Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, id = m.PatientId, version })
                               }
                           }
                       }),
                       Links = new PageLinksModel
                       {
                           First = new Link
                           {
                               Relation = First,
                               Href = _urlHelper.GetPathByName(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, new { controller = EndpointName, id, measure, page = 1, pagination.PageSize, version })
                           },
                           Previous = page.Count > 1 && pagination.Page > 1
                                ? new Link
                                {
                                    Relation = Previous,
                                    Href = _urlHelper.GetPathByName(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, new { controller = EndpointName, id, measure, page = pagination.Page - 1, pagination.PageSize, version })
                                }
                                : null,
                           Last = new Link
                           {
                               Relation = Last,
                               Href = _urlHelper.GetPathByName(RouteNames.DefaultGetAllSubResourcesByResourceIdApi, new { controller = EndpointName, id, measure, page = page.Total, pagination.PageSize, version })
                           }
                       },
                       Total = page.Total
                   };

                   return page.Count > 1
                       ? new PartialObjectResult(response)
                       : (ActionResult<GenericPageModel<Browsable<GenericMeasureModel>>>)response;
               },
                none: () => new NotFoundResult());
        }

        /// <summary>
        /// Gets one measuremeant by its id.
        /// </summary>
        /// <param name="id">id of the patient</param>
        /// <param name="measure">name of the type of measurement to look for</param>
        /// <param name="measureId">id of the measure</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <response code="200">Hurray ! You found it</response>
        /// <response code="404">The think you're looking for is not here.</response>
        [HttpGet("{id}/{measure}/{measureId}")]
        [HttpHead("{id}/{measure}/{measureId}")]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(typeof(Browsable<GenericMeasureModel>), Status200OK)]
        public async Task<ActionResult<Browsable<GenericMeasureModel>>> GetOneMeasurementByPatientId([RequireNonDefault] Guid id,
                                                                                              [RequireNonDefault] string measure,
                                                                                              [RequireNonDefault] Guid measureId,
                                                                                              CancellationToken ct = default)
        {
            Option<GenericMeasureInfo> maybeMeasure = await _mediator.Send(new GetOneMeasureInfoByPatientIdQuery((id, measure, measureId)), ct)
                                                                     .ConfigureAwait(false);

            return maybeMeasure.Match<ActionResult<Browsable<GenericMeasureModel>>>(
                some: m =>
                {
                    string version = _apiVersion.ToString();
                    return new Browsable<GenericMeasureModel>
                    {
                        Resource = new GenericMeasureModel
                        {
                            Id = m.Id,
                            DateOfMeasure = m.DateOfMeasure,
                            PatientId = m.PatientId,
                            Values = m.Data
                        },
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = Self,
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneSubResourcesByResourceIdAndSubresourceIdApi,
                                                                new { controller = EndpointName, id = m.PatientId, measureId = m.Id, measure, version })
                            },
                            new Link
                            {
                                Relation = "patient",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi,
                                                                new { controller = EndpointName, id = m.PatientId, version })
                            }
                        }
                    };
                },
                none: () => new NotFoundResult());
        }

        /// <summary>
        /// Deletes the measurement with the specified <paramref name="measureId"/>
        /// </summary>
        /// <param name="id">id of the patient</param>
        /// <param name="measure">name of the measure</param>
        /// <param name="measureId">id of the measure</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete("{id}/{measure}/{measureId}")]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status204NoContent)]
        public Task<ActionResult> DeleteOneMeasurementByPatientId([RequireNonDefault] Guid id,
                                                                  [RequireNonDefault] string measure,
                                                                  [RequireNonDefault] Guid measureId,
                                                                  CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
