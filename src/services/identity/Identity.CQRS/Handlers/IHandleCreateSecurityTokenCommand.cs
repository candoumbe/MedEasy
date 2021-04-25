using Identity.CQRS.Commands;

using MediatR;

using Microsoft.IdentityModel.Tokens;

namespace Identity.CQRS.Handlers
{
    /// <summary>
    /// Handles creation of <see cref="SecurityToken"/>.
    /// </summary>
    public interface IHandleCreateSecurityTokenCommand : IRequestHandler<CreateSecurityTokenCommand, SecurityToken>
    {
    }
}
