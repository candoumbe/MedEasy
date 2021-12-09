namespace Identity.API.Fixtures.v2
{
    using Bogus;

    using Identity.API.Features.v1.Accounts;
    using Identity.DTO;
    using Identity.DTO.Auth;
    using Identity.DTO.v2;
    using Identity.Ids;

    using MedEasy.IntegrationTests.Core;

    using NodaTime;
    using NodaTime.Serialization.SystemTextJson;

    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public class IdentityApiFixture : IntegrationFixture<Startup>
    {
        public static JsonSerializerOptions SerializerOptions
        {
            get
            {
                JsonSerializerOptions options = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                options.PropertyNameCaseInsensitive = true;
                return options;
            }
        }

        private static readonly Faker _faker = new();

        /// <summary>
        /// Gets/sets the email to use to create an account or to log in
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Password to use to create an account or to login
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        ///  Name of the account
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The current token
        /// </summary>
        public BearerTokenInfo Tokens { get; private set; }

        public IdentityApiFixture()
        {
            Email = _faker.Internet.Email(lastName: $"{Guid.NewGuid():n}");
            Password = _faker.Internet.Password();
        }

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="newAccount">Account to register</param>
        private async Task Register(CancellationToken ct = default)
        {
            // Create account
            using HttpClient client = CreateClient();
            string uri = $"/v2/{AccountsController.EndpointName}";

            NewAccountInfo newAccount = new ()
            {
                Email = Email,
                Password = Password,
                ConfirmPassword = Password,
                Name = Email,
                Username = Email,
                Id = AccountId.New()
            };

            using HttpResponseMessage response = await client.PostAsJsonAsync(uri, newAccount, SerializerOptions, ct)
                                                             .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            // Get Token
            await LogIn(ct) .ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            await Register().ConfigureAwait(false);
        }

        /// <summary>
        /// Connects a previously registered accouunt
        /// </summary>
        public async Task LogIn(CancellationToken ct = default)
        {
            using HttpClient client = CreateClient();
            const string uri = "/v2/auth/token";

            if (Tokens is null)
            {
                using HttpResponseMessage response = await client.PostAsJsonAsync(uri, new { Username = Email, Password }, SerializerOptions, ct)
                                                                 .ConfigureAwait(false);


                response.EnsureSuccessStatusCode();

                Tokens = await response.Content.ReadFromJsonAsync<BearerTokenInfo>(SerializerOptions, ct)
                                                .ConfigureAwait(false);

            }
            else if (Tokens.AccessToken.Expires <= DateTime.UtcNow)
            {
                await RenewToken(Email, new RefreshAccessTokenInfo { AccessToken = Tokens.AccessToken.Token, RefreshToken = Tokens.RefreshToken.Token }, ct)
                    .ConfigureAwait(false);
            }

        }

        /// <summary>
        /// Ren
        /// </summary>
        /// <param name="username"></param>
        /// <param name="refreshTokenInfo"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task RenewToken(string username, RefreshAccessTokenInfo refreshTokenInfo, CancellationToken ct = default)
        {
            using HttpClient client = CreateClient();
            string uri = $"/v2/auth/token/{username}/refresh";

            using HttpResponseMessage response = await client.PostAsJsonAsync(uri, refreshTokenInfo, SerializerOptions, ct)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                Tokens = await response.Content.ReadFromJsonAsync<BearerTokenInfo>(SerializerOptions, ct)
                                         .ConfigureAwait(false);
            }

        }
    }
}