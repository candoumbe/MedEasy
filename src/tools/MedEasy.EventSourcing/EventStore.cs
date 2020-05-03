using System;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using MedEasy.CQRS.Core.Events;

namespace MedEasy.EventSourcing
{
    public class Eventstore : IEventStore
    {
        private readonly IEventStoreConnection _connection;

        public Eventstore(IEventStoreConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task Publish(NotificationBase @event)
        {

            await _connection.ConnectAsync()
                .ConfigureAwait(false);

            string json = @event.Jsonify();
            EventData eventData = new EventData(@event.Id, @event.GetType().Name, true, Encoding.UTF8.GetBytes(json), Array.Empty<byte>());

            await _connection.AppendToStreamAsync(@event.GetType().Name, ExpectedVersion.Any, eventData)
                .ConfigureAwait(false);

        }
    }
}
