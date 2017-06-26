using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MedEasy.RestObjects;
using MedEasy.DAL.Repositories;
using MedEasy.Queries;
using MedEasy.DTO;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using MedEasy.Handlers.Core.Queries;
using System.Threading;
using Optional;

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
        /// Handler for "I want one resource" queries.
        /// </summary>
        protected IHandleQueryOneAsync<Guid, TKey, TResource, IWantOneResource<Guid, TKey, TResource>> GetByIdQueryHandler { get; }

        /// <summary>
        /// Handler for "I want several resources" queries
        /// </summary>
        protected IHandleQueryPageAsync<Guid, PaginationConfiguration, TResource, IWantPageOfResources<Guid, TResource>> GetManyQueryHandler { get; }

        /// <summary>
        /// Options associated with the API
        /// </summary>
        protected IOptionsSnapshot<MedEasyApiOptions> ApiOptions { get; }

        /// <summary>
        /// Helper to build URLs throughout the controler
        /// </summary>
        protected IUrlHelper UrlHelper { get; }


        /// <summary>
        /// Builds a new <see cref="RestReadControllerBase{TKey, TResource}"/> instance
        /// </summary>
        /// <param name="logger">logger to use</param>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="urlHelper">actory for creating <see cref="IUrlHelper"/> instances</param>
        /// <param name="getByIdHandler">handler to use to lookup for a single resource</param>
        /// <param name="getPageQueryHandler">handler to use to lookup for a page of resources</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="logger"/> or <paramref name="getByIdHandler"/> is <code>null</code></exception>
        protected RestReadControllerBase(
            ILogger logger,
            IOptionsSnapshot<MedEasyApiOptions> apiOptions,
            IHandleQueryOneAsync<Guid, TKey, TResource, IWantOneResource<Guid, TKey, TResource>> getByIdHandler,
            IHandleQueryPageAsync<Guid, PaginationConfiguration, TResource, IWantPageOfResources<Guid, TResource>> getPageQueryHandler,
            IUrlHelper urlHelper) : base(logger)
        {
            GetByIdQueryHandler = getByIdHandler ?? throw new ArgumentNullException(nameof(getByIdHandler), "Handler cannot be null");
            GetManyQueryHandler = getPageQueryHandler ?? throw new ArgumentNullException(nameof(getPageQueryHandler), "GET many queryHandler cannot be null");
            UrlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper), $"{nameof(urlHelper)} cannot be null");
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
            Option<TResource> resource = await GetByIdQueryHandler.HandleAsync(new GenericGetOneResourceByIdQuery<TKey, TResource>(id), cancellationToken);

            return resource.Match<IActionResult>(
            none: () => new NotFoundResult(),
            some: x =>
               {
                   IEnumerable<Link> links = new List<Link>
                   {
                        new Link
                        {
                            Relation = "self",
                            Href = UrlHelper.Action(nameof(Get), ControllerName, new {x.Id })
                        }
                   };
                   IEnumerable<Link> additionalLinks = BuildAdditionalLinksForResource(x);

                   Debug.Assert(additionalLinks != null, "Implementation cannot return null");

                   return new OkObjectResult(new BrowsableResource<TResource>
                   {
                       Resource = x,
                       Links = links.Concat(additionalLinks)
                   });
               });


        }

        /// <summary>
        /// Builds links that can be used to get resource related to the resource retrieved by calling <see cref="Get(TKey, CancellationToken)"/>
        /// </summary>
        /// <param name="resource">The resource to build links for</param>
        /// <returns></returns>
        protected virtual IEnumerable<Link> BuildAdditionalLinksForResource(TResource resource) => Enumerable.Empty<Link>();

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
                    PageSize = ApiOptions.Value.DefaultPageSize
                };
            }

            query.PageSize = Math.Min(query.PageSize, ApiOptions.Value.MaxPageSize);

            return await GetManyQueryHandler.HandleAsync(new GenericGetPageOfResourcesQuery<TResource>(query), cancellationToken);
        }
    }
}
