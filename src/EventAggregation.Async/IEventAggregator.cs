using System;
using System.Threading.Tasks;

namespace EventAggregation.Async
{
    /// <summary>
    /// Interface for a centralised Event Aggregator capable of sending Asynchronous messages. 
    /// See https://martinfowler.com/eaaDev/EventAggregator.html for a description of the pattern.
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// Add a new listener to the event aggregator. Each listener must implement the <see cref="IListenFor{T}"/> at least once
        /// to be added to the list of subscribers.
        /// </summary>
        /// <param name="listener">The listener to be added.</param>
        void AddListener(object listener);

        /// <summary>
        /// Asynchronously Send a message through the system.
        /// </summary>
        /// <typeparam name="T">The type of message being raised.</typeparam>
        /// <param name="message">The message being sent.</param>        
        Task SendMessage<T>(T message);
    }
}
