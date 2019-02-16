using System;

namespace MedEasy.Abstractions
{
    /// <summary>
    /// Abstraction for static methods <see cref="DateTimeOffset.UtcNow"/> and <see cref="DateTime.UtcNow"/>
    /// </summary>
    public class DateTimeService : IDateTimeService
    {
        /// <summary>
        /// Gets the current <see cref="DateTimeOffset"/>
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset UtcNowOffset() => DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets the current <see cref="DateTime"/>
        /// </summary>
        /// <returns></returns>
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}
