using System.Diagnostics.CodeAnalysis;

namespace EventAggregation.Logging
{
    /// <summary>
    /// Abstract base class for a message that should be logged when sent via the <see cref="EventAggregator"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class LoggedMessage : IAmLogged
    {
        private readonly string _actionPerformed;
        private readonly MessageCode _code;

        /// <summary>
        /// Construct a new instance of the <see cref="LoggedMessage"/> class.
        /// </summary>
        /// <param name="code">The action code for the message.</param>
        /// <param name="actionPerformed">The description of the action being performed.</param>
        protected LoggedMessage(MessageCode code, string actionPerformed)
        {
            _code = code;
            _actionPerformed = actionPerformed;
        }

        /// <summary>
        /// Ensure the action is written to the logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public void Log(ICanLog logger)
        {
            logger.Write(_code, _actionPerformed);
        }
    }

    /// <summary>
    /// Abstract base class for a message of type {T} that should be logged when sent via the <see cref="EventAggregator"/>.
    /// </summary>
    /// <typeparam name="T">The type of item being passed with the message.</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class LoggedMessage<T> : LoggedMessage
    {
        /// <summary>
        /// Get the item from the message.
        /// </summary>
        protected T MessageItem { get; }

        /// <summary>
        /// Construct a new instance of the <see cref="LoggedMessage{T}"/> class.
        /// </summary>
        /// <param name="messageItem">The item the being passed by the message.</param>
        /// <param name="code">The action code for the message.</param>
        /// <param name="actionPerformed">The description of the action being performed.</param>
        protected LoggedMessage(T messageItem, MessageCode code, string actionPerformed) 
            : base(code, actionPerformed)
        {
            MessageItem = messageItem;
        }
    }
}
