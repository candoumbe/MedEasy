using Identity.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Queries
{
    public class HandleHashPasswordWithPredefinedSaltAndIterationQuery : IRequestHandler<HashPasswordWithPredefinedSaltAndIterationQuery, string>
    {
        public  async Task<string> Handle(HashPasswordWithPredefinedSaltAndIterationQuery query, CancellationToken cancellationToken) =>
            // generate a 128-bit salt using a secure PRNG

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: query.Data.password,
                salt: Convert.FromBase64String(query.Data.salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: query.Data.iteration,
                numBytesRequested: 256 / 8));
    }
}
