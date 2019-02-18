using System.Collections.Generic;
using EventAggregation.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EventAggregation.Tests.Logging
{
    /// <summary>
    /// Unit tests for the <see cref="LoggedMessageBroker"/> class.
    /// </summary>
    [TestClass]
    public class LoggedMessageBrokerTests
    {
        private Mock<ICanLog> _mockLogger;

        /// <summary>
        /// Verify the <see cref="LoggedMessageBroker.Enable"/> sets the <see cref="LoggedMessageBroker.IsEnabled"/>
        /// to true.
        /// </summary>
        [TestMethod]
        public void Enable_Sets_IsEnable_To_True()
        {
            using (var target = GetTarget())
            {
                target.Enable();
                Assert.IsTrue(target.IsEnabled);
            }
        }

        /// <summary>
        /// Verify the <see cref="LoggedMessageBroker.Enable"/> opens all <see cref="ICanLog"/>.
        /// </summary>
        [TestMethod]
        public void Enable_Opens_ICanLog()
        {
            using (var target = GetTarget())
            {
                target.Enable();
                _mockLogger.Verify(m => m.Open(), Times.Exactly(2));
            }
        }

        /// <summary>
        /// Verify the <see cref="LoggedMessageBroker.Disable"/> sets the <see cref="LoggedMessageBroker.IsEnabled"/>
        /// to false.
        /// </summary>
        [TestMethod]
        public void Disable_Sets_IsEnable_To_False()
        {
            using (var target = GetTarget())
            {
                target.Enable();
                target.Disable();
                Assert.IsFalse(target.IsEnabled);
            }
        }

        /// <summary>
        /// Verify the <see cref="LoggedMessageBroker.Disable"/> closes all <see cref="ICanLog"/>.
        /// </summary>
        [TestMethod]
        public void Disable_Closes_ILoggedMessageRepository()
        {
            using (var target = GetTarget())
            {
                target.Disable();
                _mockLogger.Verify(m => m.Close(), Times.Exactly(2));
            }
        }

        /// <summary>
        /// Verify the <see cref="LoggedMessageBroker.Handle(IAmLogged)"/> writes to all
        /// loggers when <see cref="LoggedMessageBroker.IsEnabled"/> is true.
        /// </summary>
        [TestMethod]
        public void Handle_LoggedMessage_Writes_To_All_Loggers_When_Enabled()
        {
            using (var target = GetTarget())
            {
                const MessageCode expectedActionCode = MessageCode.System;
                const string expectedText = "Action Performed Text";

                target.Enable();
                _mockLogger.SetupGet(m => m.IsOpen).Returns(true);
                target.Handle(new TestLoggedMessage(expectedActionCode, expectedText));
                _mockLogger.Verify(m => m.Write(expectedActionCode, expectedText), Times.Exactly(2));
            }
        }

        /// <summary>
        /// Verify the <see cref="LoggedMessageBroker.Handle(IAmLogged)"/> does not write to the
        /// repository when <see cref="LoggedMessageBroker.IsEnabled"/> is false.
        /// </summary>
        [TestMethod]
        public void Handle_LoggedMessage_Does_Not_Writes_To_Repository_When_Not_Enabled()
        {
            using (var target = GetTarget())
            {
                target.Handle(new TestLoggedMessage(MessageCode.User, "Any Text"));
                _mockLogger.Verify(m => m.Write(It.IsAny<MessageCode>(), It.IsAny<string>()), Times.Never);
            }
        }        

        private LoggedMessageBroker GetTarget()
        {
            _mockLogger = new Mock<ICanLog>();
            return new LoggedMessageBroker(new List<ICanLog>{_mockLogger.Object, _mockLogger.Object});
        }
    }

    internal class TestLoggedMessage : LoggedMessage
    {
        public TestLoggedMessage(MessageCode code, string actionPerformed) : base(code, actionPerformed)
        {}
    }
}
