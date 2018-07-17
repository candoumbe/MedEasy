using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.CQRS.Queries
{
    /// <summary>
    /// Request hashing a password
    /// </summary>
    public class HashPasswordQuery : QueryBase<Guid, string, (string salt, string passwordHash)>
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
