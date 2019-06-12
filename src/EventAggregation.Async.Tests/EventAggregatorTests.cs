using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventAggregation.Async.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="EventAggregator"/> class.
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
        /// Verify the <see cref="EventAggregator.SendMessage{T}(T)"/> sends to all subscribers.
        /// </summary>
        [TestMethod]
        public async Task SendMessage_Sends_To_All_Subscribers()
        {
            _target.AddListener(_listenerA);
            _target.AddListener(_listenerAB);

            await _target.SendMessage(new ObservableA());

            Assert.AreEqual(1, _listenerA.MessageAReceived, "ListenerA");
            Assert.AreEqual(1, _listenerAB.MessageAReceived, "ListenerAB");
        }

        /// <summary>
        /// Verify message is sent to all registered <see cref="IListenFor{T}"/> interfaces.
        /// </summary>
        [TestMethod]
        public async Task SendMessage_Sends_To_All_Registered_Interfaces()
        {
            _target.AddListener(_listenerAB);

            await _target.SendMessage(new ObservableA());
            await _target.SendMessage(new ObservableB());

            Assert.AreEqual(1, _listenerAB.MessageAReceived);
            Assert.AreEqual(1, _listenerAB.MessageBReceived);
        }

        /// <summary>
        /// Verify the <see cref="EventAggregator.SendMessage{T}(T)"/> automatically removes zombie
        /// references.
        /// </summary>
        [TestMethod]
        public async Task SendMessage_Removes_Zombie_References()
        {
            _target.AddListener(_listenerA);
            _target.AddListener(_listenerAB);

            _listenerA = null; // De-refernece the listener
            GC.Collect(0); // Force the Garbage Collector into action.

            await _target.SendMessage(new ObservableA());

            _target.Subscriptions.TryGetValue(typeof(IListenFor<ObservableA>), out var subscribers);

            Assert.AreEqual(1, subscribers.Count);
        }
    }
}
