using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.CQRS.Core.Events
{
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
