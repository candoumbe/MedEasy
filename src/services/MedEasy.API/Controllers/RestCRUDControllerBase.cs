using MedEasy.DAL.Repositories;
using MedEasy.Objects;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MedEasy.Queries;
using MedEasy.DTO;
using MedEasy.Commands;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Queries;
using System.Threading;

namespace MedEasy.API.Controllers
{


    /// <summary>
    /// Base controller that can be used as a starting point for building RESTfull controllers.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <para>
    /// Type of the resource identifier.
    /// </para>
    /// The resource identifier is used in <see cref="RestReadControllerBase{TKey, TResource}.Get(TKey, System.Threading.CancellationToken)"/> operation.
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TCommandId">Type of the command identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource</typeparam>
    /// <typeparam name="TGetManyQuery">Type of query to get many resources</typeparam>
    /// <typeparam name="TGetByIdQuery">Type of queries to get a resource by its id</typeparam>
    /// <typeparam name="TPost">Type of <see cref="Create(TCreateCommand, CancellationToken)"/> operation input</typeparam>
    /// <typeparam name="TCreateCommand">Type of commands that create resource</typeparam>
    /// <typeparam name="TRunCreateCommand">Type of handler that can run commands to create a resource</typeparam>
    public abstract class RestCRUDControllerBase<TKey, TEntity, TResource, TGetByIdQuery, TGetManyQuery, TCommandId, TPost, TCreateCommand, TRunCreateCommand> : RestReadControllerBase<TKey, TResource>
        where TResource : class, IResource<TKey>
        where TKey : IEquatable<TKey>
        where TGetByIdQuery : IWantOneResource<Guid, TKey, TResource>
        where TGetManyQuery : IWantManyResources<Guid, TResource>
        where TEntity : IEntity<int>
        where TCommandId : IEquatable<TCommandId>
        where TCreateCommand : ICommand<TCommandId, TPost>
        where TRunCreateCommand : IRunCommandAsync<TCommandId, TPost, TResource, TCreateCommand>

    {
        private readonly TRunCreateCommand _iRunCreateCommand;
        

        /// <summary>
        /// Builds a new <see cref="RestCRUDControllerBase{TKey, TEntity, TResource, TGetByIdQuery, TGetManyQuery, TCommandId, TPost, TCreateCommand, TRunCreateCommand}"/> instance
        /// </summary>
        /// <param name="logger">logger to use</param>
        /// <param name="apiOptions">Options of the api</param>
        /// <param name="urlHelperFactory">factory to create <see cref="IUrlHelper"/> instance</param>
        /// <param name="actionContextAccessor">Gives access to the current <see cref="ActionContext"/> instance</param>
        /// <param name="getOneResourceByIdHandler"><see cref="IHandleQueryAsync{TKey, TData, TResult, TQuery}"/> implementation to use when dealing with a "GET" one resource</param>
        /// <param name="getManyResourcesHandler"><see cref="IHandleQueryAsync{TKey, TData, TResult, TQuery}"/> implementation to use when dealing with a "GET" one resource</param>
        /// <param name="iRunCreateCommand"><see cref="IRunCommandAsync{TKey, TInput, TCreateCommand}"/> implementation to use when dealing with a "POST" resource</param>
        /// <exception cref="ArgumentNullException">if any arguments is <c>null</c></exception>
        protected RestCRUDControllerBase(
            ILogger logger,
            IOptions<MedEasyApiOptions> apiOptions,
            IHandleQueryAsync<Guid, TKey, TResource, IWantOneResource<Guid, TKey, TResource>> getOneResourceByIdHandler,
            IHandleQueryAsync<Guid, PaginationConfiguration, IPagedResult<TResource>, IWantManyResources<Guid, TResource>> getManyResourcesHandler,
            TRunCreateCommand iRunCreateCommand, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor) : base(logger, apiOptions, getOneResourceByIdHandler, getManyResourcesHandler, urlHelperFactory, actionContextAccessor )
        {
            _iRunCreateCommand = iRunCreateCommand;


        }



        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="createCommand">The command which create the resource</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        [NonAction]
        public async ValueTask<TResource> Create(TCreateCommand createCommand, CancellationToken cancellationToken = default(CancellationToken))
        {
            TResource resource = await _iRunCreateCommand.RunAsync(createCommand);
            IUrlHelper urlHelper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);
            resource.Meta = new Link
            {
                Href = urlHelper.Action(nameof(Get), new  { resource.Id }),
                Relation = "self",
            };
            
            return resource;

        }

    }
}
