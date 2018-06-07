using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.CQRS.Commands.Accounts;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Optional;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static MedEasy.RestObjects.LinkRelation;
using Identity.API.Routing;
using System.Linq;
using MedEasy.CQRS.Core.Commands;
using MedEasy.DTO;

namespace Identity.API.Features.Accounts
{
    /// <summary>
    /// Handles <see cref="Account"/>s resources
    /// </summary>
    public class AccountsController
    {
        public static string EndpointName => nameof(AccountsController)
            .Replace(nameof(Controller), string.Empty);
        private readonly IUrlHelper _urlHelper;
        private readonly IOptionsSnapshot<IdentityApiOptions> _apiOptions;
        private readonly IMediator _mediator;

        public AccountsController(IUrlHelper urlHelper, IOptionsSnapshot<IdentityApiOptions> apiOptions, IMediator mediator)
        {
            _urlHelper = urlHelper;
            _apiOptions = apiOptions;
            _mediator = mediator;
        }

        /// <summary>
        /// Gets a subset of accounts resources
        /// </summary>
        /// <param name="paginationConfiguration">paging configuration</param>
        /// <param name="ct">Notification to abort request execution</param>
        /// <returns></returns>
        public async Task<IActionResult> Get(PaginationConfiguration paginationConfiguration, CancellationToken ct = default)
        {
            IdentityApiOptions apiOptions = _apiOptions.Value;
            paginationConfiguration.PageSize = Math.Min(paginationConfiguration.PageSize, apiOptions.MaxPageSize);

            GetPageOfAccountInfoQuery query = new GetPageOfAccountInfoQuery(paginationConfiguration);

            Page<AccountInfo> page = await _mediator.Send(query, ct)
                .ConfigureAwait(false);

            GenericPagedGetResponse<BrowsableResource<AccountInfo>> result = new GenericPagedGetResponse<BrowsableResource<AccountInfo>>(
                page.Entries.Select(resource => new BrowsableResource<AccountInfo>
                {
                    Resource = resource,
                    Links = new[]
                    {
                        new Link {}
                    }
                }),

                first: _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = 1, paginationConfiguration.PageSize }),
                previous: paginationConfiguration.Page > 1 && page.Count > 1
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = paginationConfiguration.Page - 1, paginationConfiguration.PageSize })
                    : null,
                next: paginationConfiguration.Page < page.Count
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = paginationConfiguration.Page + 1, paginationConfiguration.PageSize })
                    : null,
                last: _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = page.Count, paginationConfiguration.PageSize }),
                count: page.Total
            );

            return new OkObjectResult(result);

        }

        /// <summary>
        /// Delete the account with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the resource to delete.</param>
        /// <param name="ct">Notifies to abort the request execution.</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            DeleteAccountInfoByIdCommand cmd = new DeleteAccountInfoByIdCommand(id);
            DeleteCommandResult cmdResult = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            IActionResult actionResult;
            switch (cmdResult)
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
                    throw new ArgumentOutOfRangeException($"Unexpected <{cmdResult}> result");
            }
            return actionResult;
        }

        /// <summary>
        /// Delete the specified account
        /// </summary>
        /// <param name="id">id of the account to delete</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        {
            Option<AccountInfo> optionalAccount = await _mediator.Send(new GetAccountInfoByIdQuery(id), ct)
                .ConfigureAwait(false);

            return optionalAccount.Match(
                some: account =>
               {
                   BrowsableResource<AccountInfo> browsableResource = new BrowsableResource<AccountInfo>
                   {
                       Resource = account,
                       Links = new[]
                       {
                            new Link { Relation = Self, Method = "GET", Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, account.Id }) },
                            new Link { Relation = "delete",Method = "DELETE", Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, account.Id }) }
                       }
                   };

                   return new OkObjectResult(browsableResource);
               },
                none: () => (IActionResult)new NotFoundResult()
            );
        }

        /// <summary>
        /// Partially update an account resource.
        /// </summary>
        /// <remarks>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        ///
        ///     // PATCH api/accounts/3594c436-8595-444d-9e6b-2686c4904725
        ///     
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/Email",
        ///             "from": "string",
        ///             "value": "bruce@wayne-entreprise.com"
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
        /// <response code="404">Resource to "PATCH" not found</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ErrorObject), 400)]
        public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<AccountInfo> changes, CancellationToken ct = default)
        {

            PatchInfo<Guid, AccountInfo> data = new PatchInfo<Guid, AccountInfo>
            {
                Id = id,
                PatchDocument = changes
            };
            PatchCommand<Guid, AccountInfo> cmd = new PatchCommand<Guid, AccountInfo>(data);

            ModifyCommandResult cmdResult = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            IActionResult actionResult;
            switch (cmdResult)
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
                    throw new ArgumentOutOfRangeException($"Unexpected <{cmdResult}> patch result");
            }

            return actionResult;


        }
    }
}
