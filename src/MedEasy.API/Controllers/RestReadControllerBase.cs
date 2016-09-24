using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MedEasy.RestObjects;
using MedEasy.DAL.Repositories;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using MedEasy.DTO;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace MedEasy.API.Controllers
{


    /// <summary>
    /// Base controller that can be used as a starting point for building REST controllers
    /// </summary>
    /// <typeparam name="TKey">Type of the resource identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource</typeparam>
    public abstract class RestReadControllerBase<TKey, TResource> : AbstractBaseController, IRestController<TKey, TResource>
        where TKey : IEquatable<TKey>
        where TResource : IResource<TKey>
        
    {
        
        /// <summary>
        /// Handler for "I want one resource" queries
        /// </summary>
        private readonly IHandleQueryAsync<Guid, TKey, TResource, IWantOneResource<Guid, TKey, TResource>> _genericGetByIdQueryHandler;

        /// <summary>
        /// Handler for "I want several resources" queries
        /// </summary>
        private readonly IHandleQueryAsync<Guid, GenericGetQuery, IPagedResult<TResource>, IWantManyResources<Guid, TResource>> _genericGetManyQueryHandler;

                
        protected IUrlHelperFactory UrlHelperFactory { get; }

        protected IActionContextAccessor ActionContextAccessor { get; }

        /// <summary>
        /// Builds a new <see cref="RestReadControllerBase{TKey, TResource}"/> instance
        /// </summary>
        /// <param name="logger">logger to use</param>
        /// <param name="actionContextAccessor">Accessor to the <see cref="ActionContext"/></param>
        /// <param name="urlHelperFactory">actory for creating <see cref="IUrlHelper"/> instances</param>
        /// <param name="getByIdHandler">handler to use to lookup for a single resource</param>
        /// <param name="getManyQueryHandler">handler to use to lookup for many resources</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="logger"/> or <paramref name="getByIdHandler"/> is <code>null</code></exception>
        protected RestReadControllerBase(
            ILogger logger, 
            IHandleQueryAsync<Guid, TKey, TResource, IWantOneResource<Guid, TKey, TResource> > getByIdHandler,
            IHandleQueryAsync<Guid, GenericGetQuery, IPagedResult<TResource>, IWantManyResources<Guid, TResource>> getManyQueryHandler, 
            IUrlHelperFactory urlHelperFactory, 
            IActionContextAccessor actionContextAccessor) : base(logger)
        {
            if (getByIdHandler == null)
            {
                throw new ArgumentNullException(nameof(getByIdHandler), "Handler cannot be null");
            }

            if (getManyQueryHandler  == null)
            {
                throw new ArgumentNullException(nameof(getManyQueryHandler), "GET many queryHandler cannot be null");
            }

            if (actionContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(actionContextAccessor), $"{nameof(actionContextAccessor)} cannot be null");
            }

            if (urlHelperFactory == null)
            {
                throw new ArgumentNullException(nameof(urlHelperFactory), $"{nameof(urlHelperFactory)} cannot be null");
            }
            _genericGetByIdQueryHandler = getByIdHandler;
            _genericGetManyQueryHandler = getManyQueryHandler;
            UrlHelperFactory = urlHelperFactory;
            ActionContextAccessor = actionContextAccessor;
        }

        /// <summary>
        /// Gets the resource specified by the <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the resource to get</param>
        /// 
        /// <returns><see cref="Task{TEntityInfo}"/></returns>
        public async virtual Task<IActionResult> Get(TKey id)
        {
            TResource resource = await _genericGetByIdQueryHandler.HandleAsync(new GenericGetOneResourceByIdQuery<TKey, TResource>(id));
            IActionResult actionResult;
            if (resource == null)
            {
                actionResult = new NotFoundResult();
            }
            else
            {
                IUrlHelper urlHelper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);
                actionResult = new OkObjectResult(new BrowsableResource<TResource>
                {
                    Resource = resource,
                    Location = new Link { 
                        Rel = "self",
                        Href = urlHelper.Action(nameof(Get), ControllerName,  new { id = resource.Id })
                    }
                });
            }
            return actionResult;


        }

        /// <summary>
        /// Name of the controller
        /// </summary>
        protected abstract string ControllerName { get; }

        /// <summary>
        /// Gets all the entries
        /// </summary>
        /// <param name="query">GET many resources query</param>
        /// <returns><see cref="Task{GenericPagedGetResponse}"/></returns>
        [NonAction]
        public async Task<IPagedResult<TResource>> GetAll(GenericGetQuery query) 
            =>  await _genericGetManyQueryHandler.HandleAsync(new GenericGetManyResourcesQuery<TResource>(query));
    }
}
