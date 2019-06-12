namespace EventAggregation.Logging
{
    /// <summary>
    /// Interface for a message that should be logged when sent via the <see cref="EventAggregator"/>.
    /// </summary>
    public interface IAmLogged
    {
        /// <summary>
        /// Log the message with the logger.
        /// </summary>
        /// <param name="logger">The logger that will log the message.</param>
        void Log(ICanLog logger);
    }
}