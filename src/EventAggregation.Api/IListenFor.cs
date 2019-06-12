namespace EventAggregation
{
    /// <summary>
    /// Generic interface to identify a class's interest in a particular message that can be propagated
    /// by the <see cref="IEventAggregator"/>. Objects can implement multiple <see cref="IListenFor{T}"/>
    /// interfaces if they are interested in multiple messages.    
    /// </summary>
    /// <typeparam name="T">The type of message being listened for.</typeparam>
    /// <remarks>
    /// To be automatically register new objects with an application's <see cref="IEventAggregator"/>,
    /// the <see cref="IEventAggregator"/> needs to be registered as a singleton object and a
    /// type interceptor is required (assuming StructureMap is the IoC of choice).
    /// </remarks>
    public interface IListenFor<T>
    {
        /// <summary>
        /// Respond to the particular message being raised.
        /// </summary>
        /// <param name="message">The message that was raised.</param>
        void Handle(T message);
    }
}
