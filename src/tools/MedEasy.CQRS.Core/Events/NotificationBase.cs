using MediatR;
using System;

namespace MedEasy.CQRS.Core.Events
{
    /// <summary>
    /// Base class for building events
    /// </summary>
    /// <typeparam name="TId">Type of the notification's <see cref="Id"/></typeparam>
    public abstract class NotificationBase<TId, TData> : INotification
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Uniquely identifies a notification.
        /// </summary>
        public TId Id { get; }

        /// <summary>
        /// Data associated with the event
        /// </summary>
        public TData Data { get; set; }

        /// <summary>
        /// Builds a new <see cref="NotificationBase{T}"/> instance
        /// </summary>
        /// <param name="id">id of the notification</param>
        /// <param name="data">data of the event</param>
        /// <exception cref="ArgumentException"><paramref name="id"/> equals <c>default(T)</c></exception>
        protected NotificationBase(TId id, TData data)
        {
            if (Equals(id, default))
            {
                throw new ArgumentException($"{nameof(id)} must not be {default(TId)}");
            }
            Id = id;
            Data = data;
        }
    }
}
