using MedEasy.CQRS.Core.Queries;

using System;

namespace Identity.CQRS.Queries
{
    /// <summary>
    /// Request hashing a password with a preconfigured salt/iteration
    /// </summary>
    public class HashPasswordWithPredefinedSaltAndIterationQuery : QueryBase<Guid, (string password, string salt, int iteration), string>
    {
        /// <summary>
        /// Creates a new <see cref="HashPasswordWithPredefinedSaltAndIterationQuery"/> instance
        /// </summary>
        /// <param name="data">The string to hash</param>
        public HashPasswordWithPredefinedSaltAndIterationQuery((string password, string salt, int iteration) data) : base(Guid.NewGuid(), data)
        {
            if (data == default)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}
