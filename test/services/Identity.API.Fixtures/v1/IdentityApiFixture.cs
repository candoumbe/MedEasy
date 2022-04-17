namespace Identity.API.Fixtures.v1
{
    using Bogus;

    using Identity.API.Features.v1.Accounts;
    using Identity.DTO;
    using Identity.DTO.Auth;
    using Identity.DTO.v1;
    using Identity.Ids;
    using Identity.ValueObjects;

    using MedEasy.IntegrationTests.Core;

    using NodaTime;
    using NodaTime.Serialization.SystemTextJson;

    using System;
    using System.IdentityModel.Tokens.Jwt;
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
        public string Name { get; set; }

        /// <summary>
        /// The current token
        /// </summary>
        public BearerTokenInfo Tokens { get; private set; }

        private static readonly Faker Faker = new();

        public IdentityApiFixture()
        {
            Email = Email.From(Faker.Internet.Email(lastName: $"{Guid.NewGuid():n}"));
            Password = Faker.Internet.Password();
        }

        ///<inheritdoc/>
        public override async Task InitializeAsync()
        {
            
            await base.InitializeAsync().ConfigureAwait(false);
            await Register().ConfigureAwait(false);

            async Task Register(CancellationToken ct = default)
            {
                // Create account
                using HttpClient client = CreateClient();
                client.DefaultRequestHeaders.Add("api-version", "1.0");

                string uri = $"/{AccountsController.EndpointName}";

                NewAccountInfo newAccount = new()
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

            }
        }

        /// <summary>
        /// Connects a previously registered accouunt
        /// </summary>
        public async Task LogIn(CancellationToken ct = default)
        {
            using HttpClient client = CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");
            const string uri = "/auth/token";

            if (Tokens is null)
            {
                using HttpResponseMessage response = await client.PostAsJsonAsync(uri, new { Username = Email, Password }, SerializerOptions, ct)
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                Tokens = await response.Content.ReadFromJsonAsync<BearerTokenInfo>(SerializerOptions, ct)
                                                  .ConfigureAwait(false);

            }
            else
            {
                JwtSecurityToken jwt = new(Tokens.AccessToken);

                if (jwt.ValidTo <= DateTime.UtcNow)
                {
                    await RenewToken(UserName.From(Email.Value), new RefreshAccessTokenInfo { AccessToken = Tokens.AccessToken, RefreshToken = Tokens.RefreshToken }, ct)
                        .ConfigureAwait(false);

                }
            }

            async Task RenewToken(UserName username, RefreshAccessTokenInfo refreshTokenInfo, CancellationToken ct = default)
            {
                using HttpClient client = CreateClient();
                client.DefaultRequestHeaders.Add("api-version", "1.0");
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
}