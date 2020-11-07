using System;
using Xunit;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using EventStore.ClientAPI;
using FluentAssertions;
using System.Threading.Tasks;
using MedEasy.CQRS.Core.Events;
using Xunit.Categories;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Linq;

namespace MedEasy.EventSourcing.EventStore.UnitTests
{
    [UnitTest]
    [Feature("EventSourcing")]
    public class EventStoreTests : IDisposable
    {
        private Mock<IEventStoreConnection> _eventStoreConnectionMock;
        private Eventstore _sut;


        public class DataCreatedEvent : NotificationBase
        {
            public DataCreatedEvent(object data) : base(Guid.NewGuid(), data)
            {
            }
        }

        public EventStoreTests(ITestOutputHelper outputHelper)
        {
            _eventStoreConnectionMock = new Mock<IEventStoreConnection>(Strict);
            _sut = new Eventstore(_eventStoreConnectionMock.Object);
        }

        public void Dispose()
        {
            _eventStoreConnectionMock = null;
            _sut = null;
        }


        [Fact]
        public void Ctor_Throws_ArgumentException_When_Connection_IsNull()
        {
            // Act
            Action action = () => new Eventstore(null);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();

        }

        [Fact]
        public async Task PublishEvent_Relies_On_Connection()
        {
            // Arrange
            NotificationBase @event = new DataCreatedEvent(new {
                Firstname = "Jerome",
                Lastname = "Valeska"
            });
            ConcurrentStack<EventData> eventDatas = new ConcurrentStack<EventData>();
            _eventStoreConnectionMock.Setup(mock => mock.ConnectAsync())
                .Returns(Task.CompletedTask);
            _eventStoreConnectionMock.Setup(mock => mock.AppendToStreamAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<EventData>()))
                .Returns((string stream, long expectedVersion, IEnumerable<EventData> events) =>
                {
                    eventDatas.PushRange(events.ToArray());

                    return Task.FromResult(new WriteResult(ExpectedVersion.StreamExists, new Position()));
                });

            // Act
            await _sut.Publish(@event)
                .ConfigureAwait(false);

            // Assert
            _eventStoreConnectionMock.Verify(mock => mock.ConnectAsync(), Times.Once);
            _eventStoreConnectionMock.Verify(mock => mock.AppendToStreamAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<EventData[]>()), Times.Once);
            _eventStoreConnectionMock.VerifyNoOtherCalls();
            _eventStoreConnectionMock.Verify(mock => mock.AppendToStreamAsync(
                It.Is<string>(streamName => streamName == typeof(DataCreatedEvent).Name),
                It.Is<long>(expectedVersion => expectedVersion == ExpectedVersion.Any),
                It.Is<EventData[]>(events => events.Once() &&
                    events.Once(eventData => eventData.EventId == @event.Id 
                        && eventData.IsJson 
                        && eventData.Data.SequenceEqual(Encoding.UTF8.GetBytes(@event.Jsonify(null)))))), Times.Once);

            eventDatas.Should()
                .HaveCount(1).And
                .ContainSingle(eventData => eventData.EventId == @event.Id
                    && eventData.IsJson
                    && eventData.Type == typeof(DataCreatedEvent).Name
                    && eventData.Data.SequenceEqual(Encoding.UTF8.GetBytes(@event.Jsonify(null))));
        }
    }
}
