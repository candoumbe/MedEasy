using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// A range of date
    /// </summary>
    public sealed class DateTimeOffsetRange
    {

        /// <summary>
        /// Start of the date range
        /// </summary>
        public DateTimeOffset? From { get; set; }


        /// <summary>
        /// End of the date range
        /// </summary>
        public DateTimeOffset? To { get; set; }
    }
}
