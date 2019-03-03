using MediatR;
using System;

namespace MedEasy.CQRS.Core.Events
{
    public abstract class NotificationBase : NotificationBase<Guid, object>
    {
        protected NotificationBase(Guid id, object data) : base(id, data)
        {
        }
    }

    public abstract class NotificationBase<TData> : NotificationBase<Guid, TData>
    {
        protected NotificationBase(Guid id, TData data) : base(id, data)
        {
        }
    }

    /// <summary>
    /// Base class for building events
    /// </summary>
    /// <typeparam name="TId">Type of the notification's <see cref="Id"/></typeparam>
    /// <typeparam name="TData">Type of data the notification carries</typeparam>
    public abstract class NotificationBase<TId, TData> : INotification, IEquatable<NotificationBase<TId, TData>>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Uniquely identifies a notification.
        /// </summary>
        public TId Id { get; }

        /// <summary>
        /// Data associated with the event
        /// </summary>
        public TData Data { get; }

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

        public override bool Equals(object obj) => Equals(obj as NotificationBase<TId, TData>);

        public override int GetHashCode() => Data.GetHashCode();

        public bool Equals(NotificationBase<TId, TData> other)
            => other != null && Data.Equals(other.Data);
    }
}
