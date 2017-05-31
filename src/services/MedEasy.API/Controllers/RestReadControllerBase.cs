using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MedEasy.RestObjects;
using MedEasy.DAL.Repositories;
using MedEasy.Queries;
using MedEasy.DTO;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using MedEasy.Handlers.Core.Queries;
using System.Threading;

namespace MedEasy.API.Controllers
{


    /// <summary>
    /// Base controller that can be used as a starting point for building REST controllers
    /// </summary>
    /// <typeparam name="TKey">Type of the resource identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource</typeparam>
    public abstract class RestReadControllerBase<TKey, TResource> : AbstractBaseController, IRestReadController<TKey>
        where TKey : IEquatable<TKey>
        where TResource : IResource<TKey>

    {

        /// <summary>
        /// Handler for "I want one resource" queries
        /// </summary>
        protected IHandleQueryAsync<Guid, TKey, TResource, IWantOneResource<Guid, TKey, TResource>> GetByIdQueryHandler { get; }

        /// <summary>
        /// Handler for "I want several resources" queries
        /// </summary>
        protected IHandleQueryAsync<Guid, PaginationConfiguration, IPagedResult<TResource>, IWantManyResources<Guid, TResource>> GetManyQueryHandler { get; }

        /// <summary>
        /// Options associated with the API
        /// </summary>
        protected IOptions<MedEasyApiOptions> ApiOptions { get; }

        /// <summary>
        /// Factory to create <see cref="IUrlHelper"/> instances
        /// </summary>
        protected IUrlHelperFactory UrlHelperFactory { get; }

        /// <summary>
        /// Accessor for <see cref="ActionContext"/>
        /// </summary>
        protected IActionContextAccessor ActionContextAccessor { get; }

        /// <summary>
        /// Builds a new <see cref="RestReadControllerBase{TKey, TResource}"/> instance
        /// </summary>
        /// <param name="logger">logger to use</param>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="actionContextAccessor">Accessor to the <see cref="ActionContext"/></param>
        /// <param name="urlHelperFactory">actory for creating <see cref="IUrlHelper"/> instances</param>
        /// <param name="getByIdHandler">handler to use to lookup for a single resource</param>
        /// <param name="getManyQueryHandler">handler to use to lookup for many resources</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="logger"/> or <paramref name="getByIdHandler"/> is <code>null</code></exception>
        protected RestReadControllerBase(
            ILogger logger,
            IOptions<MedEasyApiOptions> apiOptions,
            IHandleQueryAsync<Guid, TKey, TResource, IWantOneResource<Guid, TKey, TResource>> getByIdHandler,
            IHandleQueryAsync<Guid, PaginationConfiguration, IPagedResult<TResource>, IWantManyResources<Guid, TResource>> getManyQueryHandler,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor) : base(logger)
        {
            GetByIdQueryHandler = getByIdHandler ?? throw new ArgumentNullException(nameof(getByIdHandler), "Handler cannot be null");
            GetManyQueryHandler = getManyQueryHandler ?? throw new ArgumentNullException(nameof(getManyQueryHandler), "GET many queryHandler cannot be null");
            UrlHelperFactory = urlHelperFactory ?? throw new ArgumentNullException(nameof(urlHelperFactory), $"{nameof(urlHelperFactory)} cannot be null");
            ActionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor), $"{nameof(actionContextAccessor)} cannot be null");
            ApiOptions = apiOptions;
        }

        /// <summary>
        /// Gets the resource specified by the <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the resource to get</param>
        /// <param name="cancellationToken"></param>
        /// 
        /// <returns><see cref="Task{TEntityInfo}"/></returns>
        public async virtual Task<IActionResult> Get(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            TResource resource = await GetByIdQueryHandler.HandleAsync(new GenericGetOneResourceByIdQuery<TKey, TResource>(id), cancellationToken);
            IActionResult actionResult;
            if (resource == null)
            {
                actionResult = new NotFoundResult();
            }
            else
            {
                IUrlHelper urlHelper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);
                IEnumerable<Link> links = new List<Link>
                {
                    new Link
                    {
                        Relation = "self",
                        Href = urlHelper.Action(nameof(Get), ControllerName, new {resource.Id })
                    }
                };
                IEnumerable<Link> additionalLinks = BuildAdditionalLinksForResource(resource, urlHelper);

                Debug.Assert(additionalLinks != null, "Implementation cannot return null");
                
                actionResult = new OkObjectResult(new BrowsableResource<TResource>
                {
                    Resource = resource,
                    Links = links.Concat(additionalLinks)
                });
            }
            return actionResult;


        }

        /// <summary>
        /// Builds links that can be used to get resource related to the resource retrieved by calling <see cref="Get(TKey, CancellationToken)"/>
        /// </summary>
        /// <param name="resource">The resource to build links for</param>
        /// <param name="urlHelper"><see cref="IUrlHelper"/> that can be used to generate additional links.</param>
        /// <returns></returns>
        protected virtual IEnumerable<Link> BuildAdditionalLinksForResource(TResource resource, IUrlHelper urlHelper) => Enumerable.Empty<Link>();

        /// <summary>
        /// Name of the controller
        /// </summary>
        protected abstract string ControllerName { get; }

        /// <summary>
        /// Gets all the entries
        /// </summary>
        /// <param name="query">GET many resources query</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="Task{GenericPagedGetResponse}"/></returns>
        [NonAction]
        public async ValueTask<IPagedResult<TResource>> GetAll(PaginationConfiguration query, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (query == null)
            {
                query = new PaginationConfiguration
                {
                    Page = 1,
                    PageSize = ApiOptions.Value.DefaulLimit
                };
            }

            query.PageSize = Math.Min(query.PageSize, ApiOptions.Value.MaxPageSize);

            return await GetManyQueryHandler.HandleAsync(new GenericGetManyResourcesQuery<TResource>(query), cancellationToken);
        }
    }
}
