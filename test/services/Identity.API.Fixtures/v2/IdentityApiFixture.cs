namespace Identity.API.Fixtures.v2
{
    using Bogus;

    using Identity.API.Features.v1.Accounts;
    using Identity.DTO;
    using Identity.DTO.Auth;
    using Identity.DTO.v2;
    using Identity.Ids;
    using Identity.ValueObjects;

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

        private static readonly Faker Faker = new();

        /// <summary>
        /// Gets/sets the email to use to create an account or to log in
        /// </summary>
        public Email Email { get; private set; }

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
            Email = Email.From(Faker.Internet.Email(lastName: $"{Guid.NewGuid():n}"));
            Password = Faker.Internet.Password();
        }

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="newAccount">Account to register</param>
        private async Task Register(CancellationToken ct = default)
        {
            // Create account
            using HttpClient client = CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "2");
            
            string uri = $"/{AccountsController.EndpointName}";

            NewAccountInfo newAccount = new ()
            {
                Email = Email,
                Password = Password,
                ConfirmPassword = Password,
                Name = Email.Value,
                Username = UserName.From(Email.Value),
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
        /// <remarks>
        /// You can get the account's <see cref="Email"/> and <see cref="Password"/>.
        /// </remarks>
        public async Task LogIn(CancellationToken ct = default)
        {
            using HttpClient client = CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "2");
            const string uri = "/auth/token";

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
                await RenewToken(UserName.From(Email.Value), new RefreshAccessTokenInfo { AccessToken = Tokens.AccessToken.Token, RefreshToken = Tokens.RefreshToken.Token }, ct)
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
        public async Task RenewToken(UserName username, RefreshAccessTokenInfo refreshTokenInfo, CancellationToken ct = default)
        {
            using HttpClient client = CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "2");
            string uri = $"/auth/token/{username}/refresh";



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