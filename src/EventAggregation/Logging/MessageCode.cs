namespace EventAggregation.Logging
{
    /// <summary>
    /// Enumeration for codes for an <see cref="IAmLogged"/> event propagated by the <see cref="IEventAggregator"/>.
    /// </summary>
    public enum MessageCode
    {
        /// <summary>
        /// Unknown action performed.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Action was initiated by the System/Application.
        /// </summary>
        System = 1,

        /// <summary>
        /// Action was initiated by the User.
        /// </summary>
        User = 2
    }
}