using Identity.CQRS.Commands;
using Identity.DTO;
using MedEasy.Abstractions;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Commands
{
    /// <summary>
    /// Handles creation of <see cref="SecurityToken"/>.
    /// </summary>
    public interface IHandleCreateSecurityTokenCommand : IRequestHandler<CreateSecurityTokenCommand, SecurityToken>
    {
    }
}
