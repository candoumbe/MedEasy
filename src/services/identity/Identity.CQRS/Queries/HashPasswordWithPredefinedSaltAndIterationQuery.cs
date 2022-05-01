namespace Identity.CQRS.Queries
{
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.ValueObjects;

    using System;

    /// <summary>
    /// Request hashing a password with a preconfigured salt/iteration
    /// </summary>
    public class HashPasswordWithPredefinedSaltAndIterationQuery : QueryBase<Guid, (Password password, string salt, int iteration), Password>
    {
        /// <summary>
        /// Creates a new <see cref="HashPasswordWithPredefinedSaltAndIterationQuery"/> instance
        /// </summary>
        /// <param name="data">The string to hash</param>
        public HashPasswordWithPredefinedSaltAndIterationQuery((Password password, string salt, int iteration) data) : base(Guid.NewGuid(), data)
        {
            if (data == default)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}
