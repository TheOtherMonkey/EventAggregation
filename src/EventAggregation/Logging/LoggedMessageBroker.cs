using System;
using System.Collections.Generic;
using System.Linq;

namespace EventAggregation.Logging
{
    /// <summary>
    /// Class to save <see cref="IAmLogged"/> to multiple <see cref="ICanLog"/> implementations.
    /// </summary>    
    public sealed class LoggedMessageBroker : 
        IListenFor<IAmLogged>, 
        IDisposable
    {
        private readonly IList<ICanLog> _loggers;
        
        /// <summary>
        /// Construct a new instance of the <see cref="LoggedMessageBroker"/> class.
        /// </summary>
        /// <param name="loggers">The list of loggers.</param>
        public LoggedMessageBroker(IEnumerable<ICanLog> loggers)
        {
            _loggers = loggers.ToList();
        }

        ~LoggedMessageBroker()
        {
            Dispose(false);
        }

        /// <summary>
        /// Enable the <see cref="LoggedMessageBroker"/> and open all <see cref="ICanLog"/>'s.
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;
            foreach (var logger in _loggers)
            {
                logger.Open();
            }
        }

        /// <summary>
        /// Disable the <see cref="LoggedMessageBroker"/> and close all <see cref="ICanLog"/>'s.
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
            foreach (var logger in _loggers)
            {
                logger.Close();
            }            
        }

        /// <summary>
        /// Returns true if the <see cref="LoggedMessageBroker"/> is enabled; otherwise false.
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Save the <see cref="IAmLogged"/> to the persistent store.
        /// </summary>        
        public void Handle(IAmLogged message)
        {
            if (!IsEnabled) return;

            foreach (var logger in _loggers)
            {
                if (logger.IsOpen) message.Log(logger);
            }
        }

        /// <summary>
        /// Dispose of any managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            foreach (var logger in _loggers)
            {
                if (logger.IsOpen) logger.Close();
            }

            _loggers.Clear();
        }
    }
}
