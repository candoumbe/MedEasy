namespace MedEasy.Wasm.Services;

using Blazored.LocalStorage;

using System.Net.Http.Headers;
using MedEasy.Wasm.Apis.Identity.v2;
using NodaTime;
using Optional;
using Refit;

/// <summary>
/// A <see cref="DelegatingHandler"/> implementation that attach authorization header to outgoing HTTP requests
/// </summary>
public class AuthorizationHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly IClock _clock;
    private readonly IIdentityApi _identityApi;
    private readonly AuthenticationStore _authenticationStore;

    public AuthorizationHeaderHandler(IIdentityApi identityApi, AuthenticationStore authenticationStore, IClock clock)
    {
        _clock = clock;
        _identityApi = identityApi;
        _authenticationStore = authenticationStore;
    }

    ///<inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Option<BearerTokenModel> optionToken = await _authenticationStore.GetToken(cancellationToken).ConfigureAwait(false);

        await optionToken.Match(
            async (token) =>
            {
                if (Instant.FromDateTimeUtc(token.AccessToken.Expires) < _clock.GetCurrentInstant())
                {
                    Option<string> optionUserName = await _authenticationStore.GetUserName(cancellationToken).ConfigureAwait(false);
                    await optionUserName.Match(
                        some: async userName =>
                       {

                           Apis.Identity.v1.RefreshAccessTokenModel refreshToken = new() { AccessToken = token.AccessToken.Token, RefreshToken = token.RefreshToken.Token };
                           IApiResponse<BearerTokenModel> newTokenResult = await _identityApi.Refresh(userName, token.AccessToken.Token, refreshToken, cancellationToken)
                                                                                             .ConfigureAwait(false);

                           if (newTokenResult.IsSuccessStatusCode)
                           {
                               await _authenticationStore.SetToken(newTokenResult.Content).ConfigureAwait(false);
                               request.Headers.Authorization = new ("Bearer", newTokenResult.Content.AccessToken.Token);
                           }
                       },
                    none: () => Task.CompletedTask);
                }
                else
                {
                    request.Headers.Authorization = new ("Bearer", token.AccessToken.Token);
                }
            },
        none: () => Task.CompletedTask);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

    }
}