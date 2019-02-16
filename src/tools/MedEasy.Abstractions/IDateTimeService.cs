using System;

namespace MedEasy.Abstractions
{
    /// <summary>
    /// Abstraction for static methods <see cref="DateTimeOffset.UtcNow"/> and <see cref="DateTime.UtcNow"/>
    /// </summary>
    public interface IDateTimeService
    {
        /// <summary>
        /// Gets the current <see cref="DateTimeOffset"/>
        /// </summary>
        /// <returns></returns>
        DateTimeOffset UtcNowOffset();

        /// <summary>
        /// Gets the current <see cref="DateTime"/>
        /// </summary>
        /// <returns></returns>
        DateTime UtcNow();
    }
}
