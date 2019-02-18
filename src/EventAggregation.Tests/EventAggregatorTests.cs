using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EventAggregation.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming

namespace EventAggregation.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="EventAggregator"/>
    /// </summary>
    [TestClass]
    public class EventAggregatorTests
    {
        private EventAggregator _target;

        private ListenerA _listenerA;
        private ListenerAB _listenerAB;

        /// <summary>
        /// Set up the test objects.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _target = new EventAggregator();

            _listenerA = new ListenerA();
            _listenerAB = new ListenerAB();    
        }

        /// <summary>
        /// Verify that a listener can be added to the <see cref="EventAggregator"/>.
        /// </summary>
        [TestMethod]
        public void AddListener_Adds_Listener()
        {
            _target.AddListener(_listenerA);            

            _target.Subscriptions.TryGetValue(typeof(IListenFor<ObservableA>), out var subscribers);

            Assert.IsTrue(subscribers.Count == 1);
        }

        /// <summary>
        /// Verify that a listener is only added to the <see cref="EventAggregator"/> once.
        /// </summary>
        [TestMethod]
        public void AddListener_SameListener_AddedOnce()
        {
            _target.AddListener(_listenerA);
            _target.AddListener(_listenerA);

            _target.Subscriptions.TryGetValue(typeof(IListenFor<ObservableA>), out var subscribers);

            Assert.IsTrue(subscribers.Count == 1);
        }

        /// <summary>
        /// Verify that all of the <see cref="IListenFor{T}"/> interfaces implemented by an object are
        /// subscribed to.
        /// </summary>
        [TestMethod]
        public void AddListener_All_Subscriptions_Added()
        {
            _target.AddListener(_listenerAB);            

            Assert.IsTrue(_target.Subscriptions.Keys.Count == 2);
        }

        /// <summary>
        /// Verify that a listener that does not implement the <see cref="IListenFor{T}"/> iinterface is not added.
        /// </summary>
        [TestMethod]
        public void AddListener_Does_Not_Add_Non_IListener()
        {
            _target.AddListener(new ObservableB());

            Assert.IsFalse(_target.Subscriptions.Any());
        }

        /// <summary>
        /// Verify that when a new listener is added to the <see cref="EventAggregator"/> 
        /// the <see cref="EventAggregator.HasListener(object)"/> returns true for that object.
        /// </summary>
        [TestMethod]
        public void HasListener_True_Test()
        {
            _target.AddListener(_listenerA);

            Assert.IsTrue(_target.HasListener(_listenerA));
        }

        /// <summary>
        /// Verify that the <see cref="EventAggregator.HasListener(object)"/> returns false for objects
        /// that have not been subscribed to.
        /// </summary>
        [TestMethod]
        public void HasListener_False_Test()
        {
            _target.AddListener(_listenerA);

            Assert.IsFalse(_target.HasListener(_listenerAB));
        }

        
        
        /// <summary>
        /// Verify the <see cref="EventAggregator.SendMessage{T}(T)"/> sends to all subscribers.
        /// </summary>
        [TestMethod]
        public void SendMessage_Sends_To_All_Subscribers()
        {
            _target.AddListener(_listenerA);
            _target.AddListener(_listenerAB);

             _target.SendMessage(new ObservableA());

            Assert.AreEqual(1, _listenerA.MessageAReceived, "ListenerA");
            Assert.AreEqual(1, _listenerAB.MessageAReceived, "ListenerAB");
        }

        /// <summary>
        /// Verify message is sent to all registered <see cref="IListenFor{T}"/> interfaces.
        /// </summary>
        [TestMethod]
        public void SendMessage_Sends_To_All_Registered_Interfaces()
        {
            _target.AddListener(_listenerAB);

            _target.SendMessage(new ObservableA());
            _target.SendMessage(new ObservableB());

            Assert.AreEqual(1, _listenerAB.MessageAReceived);
            Assert.AreEqual(1, _listenerAB.MessageBReceived);
        }

        /// <summary>
        /// Verify the <see cref="EventAggregator.SendMessage{T}(T)"/> automatically removes zombie
        /// references.
        /// </summary>
        [TestMethod]
        public void SendMessage_Removes_Zombie_References()
        {
            _target.AddListener(_listenerA);
            _target.AddListener(_listenerAB);

            _listenerA = null; // De-refernece the listener
            GC.Collect(0); // Force the Garbage Collector into action.

            _target.SendMessage(new ObservableA());

            _target.Subscriptions.TryGetValue(typeof(IListenFor<ObservableA>), out var subscribers);

            Assert.AreEqual(1, subscribers.Count);
        }

        /// <summary>
        /// Verify a message that implements the <see cref="IAmLogged"/> interface is sent twice. 
        /// Once as an <see cref="IAmLogged"/> and once as its base type.
        /// </summary>
        [TestMethod]
        public void SendMessage_Logs_IAmLogged_Types()
        {
            var logger = new Logger();
            _target.AddListener(_listenerA);
            _target.AddListener(logger);

            _target.SendMessage(new ObservableA());

            Assert.AreEqual(1, logger.MessageLoggedCount, "Logger");
            Assert.AreEqual(1, _listenerA.MessageAReceived, "Listener");
        }
    }

    #region Internal Test Classes
    [ExcludeFromCodeCoverage]
    internal class ObservableA : IAmLogged
    {
        public string ActionPerformed => throw new NotImplementedException();

        public MessageCode Code => throw new NotImplementedException();
        public void Log(ICanLog logger)
        {}
    }

    [ExcludeFromCodeCoverage]
    internal class ObservableB
    { }

    [ExcludeFromCodeCoverage]
    internal class ListenerA : IListenFor<ObservableA>
    {
        public int MessageAReceived { get; private set; }

        public void Handle(ObservableA message)
        {
            MessageAReceived++;
        }
    }

    [ExcludeFromCodeCoverage]
    internal class ListenerAB : IListenFor<ObservableA>, IListenFor<ObservableB>
    {

        public int MessageAReceived { get; private set; }

        public int MessageBReceived { get; private set; }

        public void Handle(ObservableA message)
        {
            MessageAReceived++;
        }

        public void Handle(ObservableB message)
        {
            MessageBReceived++;
        }
    }

    [ExcludeFromCodeCoverage]
    internal class Logger : IListenFor<IAmLogged>
    {
        public int MessageLoggedCount { get; private set; }

        public void Handle(IAmLogged message)
        {
            MessageLoggedCount++;
        }
    }
    #endregion
}
