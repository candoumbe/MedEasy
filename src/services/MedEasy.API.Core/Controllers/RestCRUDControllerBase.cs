using MedEasy.API.Core.Controllers;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Objects;
using MedEasy.Queries;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Optional;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.APICore.Core.Controllers
{


    /// <summary>
    /// Base controller that can be used as a starting point for building RESTfull controllers.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <para>
    /// Type of the resource identifier.
    /// </para>
    /// The resource identifier is used in <see cref="RestReadControllerBase{TKey, TResource}.Get(TKey, CancellationToken)"/> operation.
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TCommandId">Type of the command identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource</typeparam>
    /// <typeparam name="TGetManyQuery">Type of query to get many resources</typeparam>
    /// <typeparam name="TGetByIdQuery">Type of queries to get a resource by its id</typeparam>
    /// <typeparam name="TPost">Type of <see cref="Create(TCreateCommand, CancellationToken)"/> operation input</typeparam>
    /// <typeparam name="TCreateCommand">Type of commands that create resource</typeparam>
    /// <typeparam name="TRunCreateCommand">Type of handler that can run commands to create a resource</typeparam>
    /// <typeparam name="TOptions">Type of options</typeparam>
    public abstract class RestCRUDControllerBase<TOptions, TKey, TEntity, TResource, TGetByIdQuery, TGetManyQuery, TCommandId, TPost, TCreateCommand, TRunCreateCommand> : RestReadControllerBase<TOptions, TKey, TResource>
        where TResource : class, IResource<TKey>
        where TKey : IEquatable<TKey>
        where TGetByIdQuery : IWantOneResource<Guid, TKey, TResource>
        where TGetManyQuery : IWantPageOfResources<Guid, TResource>
        where TEntity : IEntity<int>
        where TCommandId : IEquatable<TCommandId>
        where TCreateCommand : ICommand<TCommandId, TPost, TResource>
        where TRunCreateCommand : IRunCommandAsync<TCommandId, TPost, TResource, TCreateCommand>
        where TOptions : ApiOptions

    {
        private readonly TRunCreateCommand _iRunCreateCommand;


        /// <summary>
        /// Builds a new <see cref="RestCRUDControllerBase{TKey, TEntity, TResource, TGetByIdQuery, TGetManyQuery, TCommandId, TPost, TCreateCommand, TRunCreateCommand}"/> instance
        /// </summary>
        /// <param name="logger">logger to use</param>
        /// <param name="apiOptions">Options of the api</param>
        /// <param name="urlHelper">Helper to biuld URLs.</param>
        /// <param name="getOneResourceByIdHandler"><see cref="IHandleQueryOneAsync{TKey, TData, TResult, TQuery}"/> implementation to use when dealing with a "GET" one resource</param>
        /// <param name="getManyResourcesHandler"><see cref="IHandleQueryPageAsync{TKey, TData, TResult, TQuery}"/> implementation to use when dealing with a "GET" one resource</param>
        /// <param name="iRunCreateCommand"><see cref="IRunCommandAsync{TKey, TInput, TCreateCommand}"/> implementation to use when dealing with a "POST" resource</param>
        /// <exception cref="ArgumentNullException">if any arguments is <c>null</c></exception>
        protected RestCRUDControllerBase(
            ILogger logger,
            IOptionsSnapshot<TOptions> apiOptions,
            IHandleQueryOneAsync<Guid, TKey, TResource, IWantOneResource<Guid, TKey, TResource>> getOneResourceByIdHandler,
            IHandleQueryPageAsync<Guid, PaginationConfiguration, TResource, IWantPageOfResources<Guid, TResource>> getManyResourcesHandler,
            TRunCreateCommand iRunCreateCommand, IUrlHelper urlHelper) : base(logger, apiOptions, getOneResourceByIdHandler, getManyResourcesHandler, urlHelper)
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
        public async ValueTask<Option<TResource, CommandException>> Create(TCreateCommand createCommand, CancellationToken cancellationToken = default)
        {
            Option<TResource, CommandException> resource = await _iRunCreateCommand.RunAsync(createCommand);
            
            return resource;

        }

    }
}
