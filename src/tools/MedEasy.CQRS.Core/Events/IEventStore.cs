namespace MedEasy.CQRS.Core.Events
{
    using System.Threading.Tasks;

    public interface IEventStore
    {
        /// <summary>
        /// Publish the 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        Task Publish(NotificationBase @event);
    }
}
