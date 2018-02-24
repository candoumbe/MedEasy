using AutoMapper.QueryableExtensions;
using FluentValidation.Results;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO.Search;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Optional;
using Patients.API.Routing;
using Patients.DTO;
using Patients.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Patients.API.Controllers
{
    /// <summary>
    /// Endpoint for <see cref="PatientInfo"/> resources.
    /// </summary>
    [Route("api/[controller]")]
    public class PatientsController : AbstractBaseController<Patient, PatientInfo, Guid>
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
        public PatientsController(ILogger<PatientsController> logger, IUrlHelper urlHelper, IOptionsSnapshot<PatientsApiOptions> apiOptions, IExpressionBuilder expressionBuilder, IUnitOfWorkFactory uowFactory)
            : base(logger, uowFactory, expressionBuilder, urlHelper)
        {
            ApiOptions = apiOptions;
        }

        /// <summary>
        /// Gets all the resources of the endpoint
        /// </summary>
        /// <param name="pagination">index of the page of resources to get</param>
        /// <remarks>
        /// Resources are returned as pages. The <paramref name="pagination"/>'s value is used has a hint by the server
        /// and there's no garanty that the size of page of result will be equal to the <paramref name="pageSize"/> set in the query.
        /// In particular, the number of resources on a page may be caped by the server.
        /// </remarks>
        /// <response code="200">items of the page</response>
        /// <response code="400"><paramref name="page"/> or <paramref name="pageSize"/> is negative or zero</response>
        [HttpGet]
        [HttpHead]
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<PatientInfo>>), 200)]
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration pagination, CancellationToken cancellationToken = default)
        {
            using (IUnitOfWork uow = UowFactory.NewUnitOfWork())
            {
                pagination.PageSize = Math.Min(pagination.PageSize, ApiOptions.Value.MaxPageSize);
                Expression<Func<Patient, PatientInfo>> selector = ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
                Page<PatientInfo> result = await uow.Repository<Patient>()
                    .ReadPageAsync(
                        selector,
                        pagination.PageSize,
                        pagination.Page,
                        new[] { OrderClause<PatientInfo>.Create(x => x.UpdatedDate) },
                        cancellationToken);

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

                IGenericPagedGetResponse<BrowsableResource<PatientInfo>> response = new GenericPagedGetResponse<BrowsableResource<PatientInfo>>(
                    resources,
                    firstPageUrl,
                    previousPageUrl,
                    nextPageUrl,
                    lastPageUrl,
                    result.Total);


                return new OkObjectResult(response);
            }
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
        [ProducesResponseType(typeof(BrowsableResource<PatientInfo>), 200)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default)
        {
            IActionResult actionResult;

            if (id == default)
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                using (IUnitOfWork uow = UowFactory.NewUnitOfWork())
                {
                    Expression<Func<Patient, PatientInfo>> selector = ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
                    Option<PatientInfo> result = await uow.Repository<Patient>()
                        .SingleOrDefaultAsync(selector, x => x.Id == id);

                    actionResult = result.Match<IActionResult>(
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
                                    }
                                }
                            };
                            return new OkObjectResult(browsableResource);
                        },
                        none: () => new NotFoundResult()
                    );
                }
            }

            return actionResult;

        }


        /// <summary>
        /// Creates a new <see cref="PatientInfo"/> resource.
        /// </summary>
        /// <param name="newPatient">data used to create the resource</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="201">the resource was created successfully</response>
        /// <response code="400"><paramref name="newPatient"/> is not valid</response>
        [HttpPost]
        [ProducesResponseType(typeof(BrowsableResource<PatientInfo>), 201)]
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        public async Task<IActionResult> Post([FromBody] CreatePatientInfo newPatient, CancellationToken cancellationToken = default)
        {
            if (newPatient.Id == default)
            {
                newPatient.Id = Guid.NewGuid();
            }
            using (IUnitOfWork uow = UowFactory.NewUnitOfWork())
            {
                Expression<Func<CreatePatientInfo, Patient>> mapFunc = ExpressionBuilder.GetMapExpression<CreatePatientInfo, Patient>();
                Func<CreatePatientInfo, Patient> funcConverter = mapFunc.Compile();
                Patient patient = funcConverter(newPatient);

                patient = uow.Repository<Patient>().Create(patient);

                await uow.SaveChangesAsync();
                Expression<Func<Patient, PatientInfo>> mapEntityToResource = ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
                PatientInfo resource = mapEntityToResource.Compile()(patient);

                return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id }, new BrowsableResource<PatientInfo>
                {
                    Resource = resource,
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = "delete",
                            Method = "DELETE",
                            Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id })
                        }
                    }
                });
            }
        }


        ///// <summary>
        ///// Updates the specified resource
        ///// </summary>
        ///// <remarks>
        ///// The resource's current value will completely be replace
        ///// </remarks>
        ///// <param name="id">identifier of the resource to update</param>
        ///// <param name="info">new values to set</param>
        ///// <returns></returns>
        ///// <response code="200">the operation succeed</response>
        ///// <response code="400">Submitted values contains an error</response>
        ///// <response code="404">Resource not found</response>
        //[HttpPut("{id}")]
        //[ProducesResponseType(typeof(PatientInfo), 200)]
        //public Task<IActionResult> Put(Guid id, [FromBody] CreatePatientInfo info)
        //{
        //    throw new NotImplementedException();
        //}

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
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            IActionResult actionResult;
            if (id == default)
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                using (IUnitOfWork uow = UowFactory.NewUnitOfWork())
                {
                    uow.Repository<Patient>().Delete(x => x.UUID == id);
                    await uow.SaveChangesAsync(cancellationToken);
                    actionResult = new NoContentResult();
                }
            }

            return actionResult;
        }

        /// <summary>
        /// Partially update a patient resource.
        /// </summary>
        /// <remarks>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        ///
        ///     // PATCH api/Patients/3594c436-8595-444d-9e6b-2686c4904725
        ///     
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/Firstname",
        ///             "from": "string",
        ///             "value": "John"
        ///       }
        ///     ]
        /// 
        /// The set of changes to apply will be applied atomically and in the order they're declared. 
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
        public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<PatientInfo> changes, CancellationToken cancellationToken = default)
        {
            IActionResult actionResult;

            if (id == default)
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                using (IUnitOfWork uow = UowFactory.NewUnitOfWork())
                {
                    Option<Patient> optionalPatient = await uow.Repository<Patient>().SingleOrDefaultAsync(x => x.UUID == id, cancellationToken)
                        .ConfigureAwait(false);
                    
                    actionResult = await optionalPatient.Match(
                        some: async (patient) =>
                        {
                            IActionResult patchActionResult = new NoContentResult();
                            Func<Operation<PatientInfo>, Operation<Patient>> converter = ExpressionBuilder.GetMapExpression<Operation<PatientInfo>, Operation<Patient>>().Compile();
                            IEnumerable<Operation<Patient>> operations =
                                 changes.Operations
                                 .AsParallel()
                                 .Select(converter)
                                 .ToArray();

                            JsonPatchDocument<Patient> patch = new JsonPatchDocument<Patient>();
                            patch.Operations.AddRange(operations);

                            patch.ApplyTo(patient, (error) => {
                                patchActionResult = new BadRequestObjectResult(new { message = error.ErrorMessage });
                            });
                            
                            await uow.SaveChangesAsync(cancellationToken)
                                .ConfigureAwait(false);

                            return patchActionResult;

                        },
                        none: () => Task.FromResult((IActionResult) new NotFoundResult())
                    )
                    .ConfigureAwait(false);

                }
            }

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
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<PatientInfo>>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        public async Task<IActionResult> Search([FromQuery]SearchPatientInfo search, CancellationToken cancellationToken = default)
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
                                sort = new Sort
                                {
                                    Direction = MedEasy.Data.SortDirection.Descending,
                                    Expression = x.ToLambda<PatientInfo>()
                                };
                            }
                            else
                            {
                                sort = new Sort
                                {
                                    Direction = MedEasy.Data.SortDirection.Ascending,
                                    Expression = x.ToLambda<PatientInfo>()
                                };
                            }

                            return sort;
                        })
            };

            Page<PatientInfo> resources = await Search(searchQuery, cancellationToken);

            GenericPagedGetResponse<BrowsableResource<PatientInfo>> page = new GenericPagedGetResponse<BrowsableResource<PatientInfo>>(
                    items: resources.Entries.Select(x => new BrowsableResource<PatientInfo>
                    {
                        Resource = x,
                        Links = new[] {
                            new Link
                            {
                                Method = "GET",
                                Relation = LinkRelation.Self,
                                Href = UrlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = ControllerName, x.Id })
                            }
                        }
                    }),
                    count: resources.Total,
                    first: UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
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
                        ? UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
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
                    next: resources.Count > search.Page
                        ? UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
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
                    last: UrlHelper.Link(RouteNames.DefaultSearchResourcesApi, new
                    {
                        controller = ControllerName,
                        search.Firstname,
                        search.Lastname,
                        search.BirthDate,
                        search.Sort,
                        page = Math.Max(resources.Count, 1),
                        search.PageSize
                    })
                );


            return new OkObjectResult(page);

        }

    }
}
