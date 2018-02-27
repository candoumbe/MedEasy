using MediatR;
using System;

namespace MedEasy.CQRS.Core.Events
{
    /// <summary>
    /// Base class for building events
    /// </summary>
    /// <typeparam name="T">Type of the notification's <see cref="Id"/></typeparam>
    public abstract class NotificationBase<T> : INotification
        where T : IEquatable<T>
    {
        /// <summary>
        /// Uniquely identifies a notification.
        /// </summary>
        public T Id { get; }

        /// <summary>
        /// Builds a new <see cref="NotificationBase{T}"/> instance
        /// </summary>
        /// <param name="id">id of the notification</param>
        /// <exception cref="ArgumentException"><paramref name="id"/> equals <c>default(T)</c></exception>
        protected NotificationBase(T id)
        {
            if (Equals(id, default))
            {
                throw new ArgumentException($"{nameof(id)} must not be {default(T)}");
            }
            Id = id;
        }
    }
}
