namespace Identity.CQRS.Handlers
{
    using Identity.CQRS.Commands;

    using MediatR;

    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Handles creation of <see cref="SecurityToken"/>.
    /// </summary>
    public interface IHandleCreateSecurityTokenCommand : IRequestHandler<CreateSecurityTokenCommand, SecurityToken>
    {
    }
}
