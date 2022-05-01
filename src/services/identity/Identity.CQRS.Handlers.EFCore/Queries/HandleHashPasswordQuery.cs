namespace Identity.CQRS.Handlers.Queries
{
    using Identity.CQRS.Queries;

    using MedEasy.ValueObjects;

    using MediatR;

    using Microsoft.AspNetCore.Cryptography.KeyDerivation;

    using System;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles queries to hash <see cref="Password"/>s.
    /// </summary>
    public class HandleHashPasswordQuery : IRequestHandler<HashPasswordQuery, (string salt, Password passwordHash)>
    {
        ///<inheritdoc/>
        public Task<(string salt, Password passwordHash)> Handle(HashPasswordQuery query, CancellationToken cancellationToken)
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];

            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: query.Data.Value,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return Task.FromResult<(string salt, Password passwordHash)>((Convert.ToBase64String(salt), Password.From(hashed)));
        }
    }
}
