namespace Identity.CQRS.Queries
{
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.ValueObjects;

    using System;

    /// <summary>
    /// Request hashing a password
    /// </summary>
    public class HashPasswordQuery : QueryBase<Guid, Password, (string salt, Password hashedPassword)>
    {
        /// <summary>
        /// Creates a new <see cref="HashPasswordQuery"/> instance
        /// </summary>
        /// <param name="data">The string to hash</param>
        public HashPasswordQuery(Password data) : base(Guid.NewGuid(), data)
        {
            if (data == default)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}
