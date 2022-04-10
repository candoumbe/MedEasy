namespace Identity.CQRS.Handlers.Commands.v1;

using Identity.CQRS.Commands;
using Identity.CQRS.Commands.v1;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO.v1;
using MediatR;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Primitives;

using Optional;

using System.IdentityModel.Tokens.Jwt;
using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.DTO;
using System.Collections.Generic;


/// <summary>
/// Handler for <see cref="LoginCommand"/>s.
/// </summary>
public class HandleLoginCommand : IRequestHandler<LoginCommand, Option<BearerTokenInfo>>
{
    private readonly IMediator _mediator;

    public HandleLoginCommand(IMediator mediator)
    {
        _mediator = mediator;
    }

    ///<inheritdoc/>
    public async Task<Option<BearerTokenInfo>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {

        LoginInfo loginInfo = new ()
        {
            UserName = request.Data.LoginInfos.UserName,
            Password = request.Data.LoginInfos.Password
        };
        Option<AccountInfo> optionalUser = await _mediator.Send(new GetOneAccountByUsernameAndPasswordQuery(loginInfo), cancellationToken)
                                                          .ConfigureAwait(false);

        return await optionalUser.Match(
            some: async accountInfo =>
            {
                JwtInfos jwtInfos = request.Data.JwtInfos;
                AuthenticationInfo authenticationInfo = new() { Location = request.Data.Location ?? string.Empty };
                
                AuthenticationTokenInfo token = await _mediator.Send(new CreateAuthenticationTokenCommand((authenticationInfo, accountInfo, jwtInfos)), cancellationToken)
                                                               .ConfigureAwait(false);
                JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

                string accessTokenString = token.AccessToken switch
                {
                    JwtSecurityToken jwtToken => jwtSecurityTokenHandler.WriteToken(jwtToken),
                    _ => throw new NotSupportedException("Unhandled access token type"),
                };
                string refreshTokenString = token.RefreshToken switch
                {
                    JwtSecurityToken jwtToken => jwtSecurityTokenHandler.WriteToken(jwtToken),
                    _ => throw new NotSupportedException("Unhandled refresh token type"),
                };

                return new BearerTokenInfo
                {
                    AccessToken = accessTokenString,
                    RefreshToken = refreshTokenString
                }.Some();
            },
            none: () => Task.FromResult(Option.None<BearerTokenInfo>())).ConfigureAwait(false);
    }
}
