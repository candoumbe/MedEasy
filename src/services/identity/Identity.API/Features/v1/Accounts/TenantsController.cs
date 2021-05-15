namespace Identity.API.Features.Accounts
{
    using Identity.API.Features.v1.Accounts;
    using Identity.API.Routing;
    using Identity.CQRS.Commands.Accounts;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DTO;
    using Identity.Ids;
    using Identity.Objects;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DTO;
    using MedEasy.Ids;

    using MediatR;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Options;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using static Microsoft.AspNetCore.Http.StatusCodes;

    /// <summary>
    /// Handles <see cref="Account"/>s resources
    /// </summary>
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    public class TenantsController
    {
        public static string EndpointName => nameof(TenantsController)
            .Replace(nameof(Controller), string.Empty);
        private readonly LinkGenerator _urlHelper;
        private readonly IOptionsSnapshot<IdentityApiOptions> _apiOptions;
        private readonly IMediator _mediator;

        public TenantsController(LinkGenerator urlHelper, IOptionsSnapshot<IdentityApiOptions> apiOptions, IMediator mediator)
        {
            _urlHelper = urlHelper;
            _apiOptions = apiOptions;
            _mediator = mediator;
        }

        /// <summary>
        /// Delete the account with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the resource to delete.</param>
        /// <param name="ct">Notifies to abort the request execution.</param>
        /// <returns></returns>
        /// <response code="204">The resource was successfully deleted.</response>
        /// <response code="404">Resource to delete was not found</response>
        /// <response code="409">Resource cannot be deleted</response>
        /// <response code="403"></response>
        [HttpDelete("{id}")]
        [ProducesResponseType(Status409Conflict)]
        [ProducesResponseType(Status409Conflict)]
        [ProducesResponseType(Status204NoContent)]
        public async Task<IActionResult> Delete(AccountId id, CancellationToken ct = default)
        {
            DeleteAccountInfoByIdCommand cmd = new(id);
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
        /// Get the specified account
        /// </summary>
        /// <param name="id">id of the tenant to delete</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [HttpHead("{id}")]
        public async Task<IActionResult> Get(TenantId id, CancellationToken ct = default)
        {
            bool isTenant = await _mediator.Send(new IsTenantQuery(id), ct)
                .ConfigureAwait(false);

            return isTenant
                ? new RedirectToRouteResult(
                           RouteNames.DefaultGetOneByIdApi, new { controller = AccountsController.EndpointName, id = id.Value },
                           permanent: false,
                           preserveMethod: true)
                : new NotFoundResult();
        }

        /// <summary>
        /// Partially update an account resource.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        /// </para>
        /// <para>    // HTTP 1.1 PATCH /api/tenants/3594c436-8595-444d-9e6b-2686c4904725</para>
        /// <example>
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/Email",
        ///             "from": "string",
        ///             "value": "bruce@wayne-entreprise.com"
        ///       }
        ///     ]
        /// </example>
        /// <para>The set of changes to apply will be applied atomically. </para>
        /// 
        /// </remarks>
        /// <param name="id">id of the resource to update.</param>
        /// <param name="changes">set of changes to apply to the resource.</param>
        /// <param name="ct">Notifies lower layers about the request abortion</param>
        /// <response code="204">The resource was successfully patched.</response>
        /// <response code="400">Changes are not valid for the selected resource.</response>
        /// <response code="404">Resource to "PATCH" not found</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<AccountInfo> changes, CancellationToken ct = default)
        {
            PatchInfo<Guid, AccountInfo> data = new()
            {
                Id = id,
                PatchDocument = changes
            };
            PatchCommand<Guid, AccountInfo> cmd = new(data);

            ModifyCommandResult cmdResult = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            return cmdResult switch
            {
                ModifyCommandResult.Done => new NoContentResult(),
                ModifyCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                ModifyCommandResult.Failed_NotFound => new NotFoundResult(),
                ModifyCommandResult.Failed_Conflict => new StatusCodeResult(Status409Conflict),
                _ => throw new ArgumentOutOfRangeException($"Unexpected <{cmdResult}> patch result"),
            };
        }
    }
}
