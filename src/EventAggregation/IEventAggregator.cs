namespace EventAggregation
{
    /// <summary>
    /// Interface for a centralised Event Aggregator. 
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
        /// Determine whether a particular object has been registered with the aggregator.
        /// </summary>
        /// <param name="listener">The listener being tested.</param>
        /// <returns>True if the <paramref name="listener"/> has been registered, otherwise False.</returns>
        bool HasListener(object listener);

        /// <summary>
        /// Send a message through the system.
        /// </summary>
        /// <typeparam name="T">The type of message being raised.</typeparam>
        /// <param name="message">The message being sent.</param>        
        void SendMessage<T>(T message);
    }
}