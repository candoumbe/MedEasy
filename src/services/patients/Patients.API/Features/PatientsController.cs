using AutoMapper.QueryableExtensions;

using DataFilters;

using FluentValidation.Results;

using MedEasy.Attributes;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.DTO.Search;
using MedEasy.RestObjects;

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Optional;

using Patients.API.Routing;
using Patients.DTO;
using Patients.Ids;
using Patients.Objects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Patients.API.Controllers
{
    /// <summary>
    /// Endpoint for <see cref="PatientInfo"/> resources.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class PatientsController : AbstractBaseController<Patient, PatientInfo, PatientId>
    {
        private IOptionsSnapshot<PatientsApiOptions> ApiOptions { get; }

        protected override string ControllerName => EndpointName;

        public static string EndpointName => nameof(PatientsController).Replace("Controller", string.Empty);

        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="urlHelper"></param>
        /// <param name="apiOptions"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="uowFactory"></param>
        public PatientsController(ILogger<PatientsController> logger, LinkGenerator urlHelper, IOptionsSnapshot<PatientsApiOptions> apiOptions, IExpressionBuilder expressionBuilder, IUnitOfWorkFactory uowFactory)
            : base(logger, uowFactory, expressionBuilder, urlHelper) => ApiOptions = apiOptions;

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
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration pagination, CancellationToken cancellationToken = default)
        {
            using IUnitOfWork uow = UowFactory.NewUnitOfWork();
            pagination.PageSize = Math.Min(pagination.PageSize, ApiOptions.Value.MaxPageSize);
            Expression<Func<Patient, PatientInfo>> selector = ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
            Page<PatientInfo> result = await uow.Repository<Patient>()
                                                .ReadPageAsync(
                                                    selector,
                                                    pagination.PageSize,
                                                    pagination.Page,
                                                    new Sort<PatientInfo>(nameof(PatientInfo.Lastname), SortDirection.Descending),
                                                    cancellationToken).ConfigureAwait(false);

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && pagination.Page > 1;

            string firstPageUrl = UrlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? UrlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page - 1 })
                    : null;

            string nextPageUrl = pagination.Page < result.Count
                    ? UrlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = pagination.Page + 1 })
                    : null;
            string lastPageUrl = result.Count > 0
                    ? UrlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, pagination.PageSize, Page = result.Count })
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
                                Href = UrlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, x.Id})
                            }
                    }
                });

            GenericPagedGetResponse<Browsable<PatientInfo>> response = new(
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
                using IUnitOfWork uow = UowFactory.NewUnitOfWork();
                Expression<Func<Patient, PatientInfo>> selector = ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
                Option<PatientInfo> result = await uow.Repository<Patient>()
                    .SingleOrDefaultAsync(selector, (Patient x) => x.Id == id, cancellationToken).ConfigureAwait(false);

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
                                        Href = UrlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, Id = resource.Id.Value }),
                                        Method = "GET"
                                    },
                                    new Link
                                    {
                                        Relation = "delete",
                                        Href = UrlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, Id = resource.Id.Value }),
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
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CreatePatientInfo newPatient, CancellationToken ct = default)
        {
            if (newPatient.Id == default)
            {
                newPatient.Id = PatientId.New();
            }
            using IUnitOfWork uow = UowFactory.NewUnitOfWork();

            Patient patient = new Patient(newPatient.Id, newPatient.Firstname, newPatient.Lastname)
                .WasBornIn(newPatient.BirthPlace)
                .WasBornOn(newPatient.BirthDate);

            patient = uow.Repository<Patient>().Create(patient);

            await uow.SaveChangesAsync(ct).ConfigureAwait(false);
            Expression<Func<Patient, PatientInfo>> mapEntityToResource = ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
            PatientInfo resource = mapEntityToResource.Compile()(patient);

            return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, Id = resource.Id.Value }, new Browsable<PatientInfo>
            {
                Resource = resource,
                Links = new[]
                {
                        new Link
                        {
                            Relation = "delete",
                            Method = "DELETE",
                            Href = UrlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, Id = resource.Id.Value })
                        }
                    }
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
            IActionResult actionResult;
            if (PatientId.Empty == id)
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                using IUnitOfWork uow = UowFactory.NewUnitOfWork();
                uow.Repository<Patient>().Delete(x => x.Id == id);
                await uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                actionResult = new NoContentResult();
            }

            return actionResult;
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
            IActionResult actionResult;

            IDictionary<string, Action<Patient, object>> patchActions = new Dictionary<string, Action<Patient, object>>
            {
                [$"/{nameof(Patient.Firstname)}"] = (patient, value) => patient.ChangeFirstnameTo(value?.ToString()),
                [$"/{nameof(Patient.Lastname)}"] = (patient, value) => patient.ChangeLastnameTo(value?.ToString()),
                [$"/{nameof(Patient.BirthPlace)}"] = (patient, value) => patient.WasBornIn(value?.ToString()),
            };


            if (id == default)
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                using IUnitOfWork uow = UowFactory.NewUnitOfWork();
                Option<Patient> optionalPatient = await uow.Repository<Patient>()
                    .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);

                actionResult = await optionalPatient.Match(
                    some: async (patient) =>
                    {
                        IActionResult patchActionResult = new NoContentResult();

                        Func<Operation<PatientInfo>, Operation<Patient>> converter = ExpressionBuilder.GetMapExpression<Operation<PatientInfo>, Operation<Patient>>().Compile();
                        Operation<Patient>[] operations =
                             changes.Operations
                             .AsParallel()
                             .Select(converter)
                             .ToArray();

                        bool canKeepPatching = true;
                        int currentOperationIndex = 0;

                        while (canKeepPatching && currentOperationIndex < operations.Count())
                        {
                            Operation<Patient> currentOperation = operations[currentOperationIndex];

                            switch (currentOperation.OperationType)
                            {
                                case OperationType.Add:
                                case OperationType.Replace:
                                    if (patchActions.TryGetValue(currentOperation.path, out Action<Patient, object> actionToTake))
                                    {
                                        actionToTake.Invoke(patient, currentOperation.value);
                                    }
                                    break;
                                default:
                                    canKeepPatching = false;
                                    actionResult = new BadRequestObjectResult($"Unsupported {currentOperation.OperationType} on {currentOperation.path}");
                                    break;
                            }

                            currentOperationIndex++;
                        }

                        await uow.SaveChangesAsync(cancellationToken)
                            .ConfigureAwait(false);

                        return patchActionResult;
                    },
                    none: () => Task.FromResult((IActionResult)new NotFoundResult())
                )
                .ConfigureAwait(false);
            }

            return actionResult;
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
        public async Task<IActionResult> Search([FromQuery] SearchPatientInfo search, CancellationToken cancellationToken = default)
        {
            IList<IFilter> filters = new List<IFilter>();
            if (!string.IsNullOrEmpty(search.Firstname))
            {
                filters.Add($"{nameof(search.Firstname)}={search.Firstname}".ToFilter<PatientInfo>());
            }

            if (!string.IsNullOrEmpty(search.Lastname))
            {
                filters.Add($"{nameof(search.Lastname)}={search.Lastname}".ToFilter<PatientInfo>());
            }

            SearchQueryInfo<PatientInfo> searchQuery = new()
            {
                Filter = filters.Count == 1
                    ? filters.Single()
                    : new MultiFilter { Logic = FilterLogic.And, Filters = filters },
                Page = search.Page,
                PageSize = search.PageSize,
                Sort = search.Sort?.ToSort<PatientInfo>() ?? new Sort<PatientInfo>(nameof(PatientInfo.Lastname), SortDirection.Descending)
            };

            Page<PatientInfo> pageOfResources = await Search(searchQuery, cancellationToken).ConfigureAwait(false);
            IActionResult actionResult;
            if (pageOfResources.Count < searchQuery.Page)
            {
                actionResult = new NotFoundResult();
            }
            else
            {
                GenericPagedGetResponse<Browsable<PatientInfo>> page = new(
                        items: pageOfResources.Entries.Select(x => new Browsable<PatientInfo>
                        {
                            Resource = x,
                            Links = new[] {
                            new Link
                            {
                                Method = "GET",
                                Relation = LinkRelation.Self,
                                Href = UrlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = ControllerName, Id = x.Id.Value })
                            }
                            }
                        }),
                        first: UrlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
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
                            ? UrlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
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
                            ? UrlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
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
                        last: UrlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new
                        {
                            controller = ControllerName,
                            search.Firstname,
                            search.Lastname,
                            search.BirthDate,
                            search.Sort,
                            page = Math.Max(pageOfResources.Count, 1),
                            search.PageSize
                        })
,
                        total: pageOfResources.Total);

                actionResult = new OkObjectResult(page);
            }

            return actionResult;
        }
    }
}
