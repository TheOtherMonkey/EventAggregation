namespace EventAggregation.Logging
{
    /// <summary>
    /// Interface for a logger that the <see cref="IAmLogged"/> messages are sent to.
    /// </summary>
    public interface ICanLog
    {
        /// <summary>
        /// Open the log and make ready to receive logged actions.
        /// </summary>
        void Open();

        /// <summary>
        /// Close the log and perform any tidy/clean-up actions.
        /// </summary>
        void Close();

        /// <summary>
        /// Returns true when the log is open; otherwise false.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Write to the log.
        /// </summary>
        /// <param name="code">The <see cref="MessageCode"/>.</param>
        /// <param name="actionPerformed">The text to be recorded in the log.</param>
        void Write(MessageCode code, string actionPerformed);
    }
}
