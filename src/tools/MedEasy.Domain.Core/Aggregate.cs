using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.Domain.Core
{
    public abstract class Aggregate<T> : IAggregate<T>
        where T : IEquatable<T>
    {
        private readonly Queue<IEvent<T>> _events;

        private readonly IDictionary<Type, Action<IEvent<T>>> _handlers;

        public T Id { get; }

        public IEnumerable<IEvent<T>> Pending => _events.AsEnumerable();

        protected Aggregate(T id)
        {
            Id = id;
            _events = new Queue<IEvent<T>>();
        }

        public void ClearUncommitedEvents() => _events.Clear();

        public void Apply(IEvent<T> evt)
        {
            if (_handlers.TryGetValue(evt.GetType(), out Action<IEvent<T>> handler))
            {
                handler.Invoke(evt);
            }
        }

        public void Raise<TEvent>(TEvent evt) where TEvent : IEvent<T>
        {
            if (_handlers.TryGetValue(evt.GetType(), out Action<IEvent<T>> handler))
            {
                _events.Enqueue(evt);
                handler.Invoke(evt);
            }
        }

        /// <summary>
        /// Adds a new handlers
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handler"></param>
        protected void On<TEvent>(Action<TEvent> handler) where TEvent : IEvent<T> => _handlers[typeof(TEvent)] = e => handler((TEvent)e);
    }
}
