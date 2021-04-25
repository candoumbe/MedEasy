using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MedEasy.IntegrationTests.Core
{

    /// <summary>
    /// A dummy <see cref="AuthenticationHandler{AuthenticationSchemeOptions}"/> that always succeed.
    /// <para>
    /// The result will always consists of the provided claims and scheme.
    /// </para>
    /// </summary>
    public class DummyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly DummyClaimsProvider _claimsProvider;

        public DummyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            DummyClaimsProvider claimsProvider) : base(options, logger, encoder, clock)
        {
            _claimsProvider = claimsProvider;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            ClaimsIdentity identity = new(_claimsProvider.Claims);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, _claimsProvider.Scheme);

            AuthenticateResult result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }


    public class DummyClaimsProvider
    {
        public string Scheme { get; }

        public IEnumerable<Claim> Claims { get; }


        public DummyClaimsProvider(string scheme, IEnumerable<Claim> claims)
        {
            Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            Claims = claims ?? throw new ArgumentNullException(nameof(claims));
        }
    }


    public class DummyAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            context.Succeed(context.PendingRequirements.FirstOrDefault());

            return Task.CompletedTask;
        }
    }
}
