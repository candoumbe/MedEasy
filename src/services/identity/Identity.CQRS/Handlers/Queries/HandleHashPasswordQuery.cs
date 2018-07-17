using Identity.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Queries
{
    public class HandleHashPasswordQuery : IRequestHandler<HashPasswordQuery, (string salt, string passwordHash)>
    {
        public Task<(string salt, string passwordHash)> Handle(HashPasswordQuery query, CancellationToken cancellationToken)
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);

                // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: query.Data,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                return Task.FromResult<(string salt, string passwordHash)>((Convert.ToBase64String(salt), query.Data));
            }

        }
    }
}
