namespace Identity.CQRS.Commands.v1;

using Identity.DTO;
using Identity.DTO.v1;

using MediatR;

using Optional;

using System;


/// <summary>
/// A command to request new tokens to signin a user.
/// </summary>
public record LoginCommand : IRequest<Option<BearerTokenInfo>>
{
    public (LoginInfo LoginInfos, JwtInfos JwtInfos, string Location) Data { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommand"/> class.
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public LoginCommand((LoginInfo Login, JwtInfos JwtOptions, string Location) data)
    {
        Data = data;
    }
}