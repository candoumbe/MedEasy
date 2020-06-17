using MedEasy.CQRS.Core.Queries;
using System;

namespace Identity.CQRS.Queries
{
    /// <summary>
    /// Request hashing a password
    /// </summary>
    public class HashPasswordQuery : QueryBase<Guid, string, (string salt, string hashedPassword)>
    {
        /// <summary>
        /// Creates a new <see cref="HashPasswordQuery"/> instance
        /// </summary>
        /// <param name="data">The string to hash</param>
        public HashPasswordQuery(string data) : base(Guid.NewGuid(), data)
        {
            if (data == default)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}
