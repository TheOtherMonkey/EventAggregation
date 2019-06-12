using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;
using System.Threading.Tasks;

namespace EventAggregation.Async.StructureMap.Tests
{
    [TestClass]
    public class EventAggregationRegistryTests
    {
        private static readonly Container Container = new Container(x => x.Scan(y =>
        {
            y.AssemblyContainingType<EventAggregationRegistry>();
            y.LookForRegistries();
            y.WithDefaultConventions();
        }));

        /// <summary>
        /// Verify the <see cref="IEventAggregator"/> is registered as a singleton in the <see cref="EventAggregationRegistry"/>.
        /// </summary>
        [TestMethod]
        public void IEventAggregator_Is_Singleton()
        {
            var aggregator1 = Container.GetInstance<IEventAggregator>();
            var aggregator2 = Container.GetInstance<IEventAggregator>();

            Assert.AreSame(aggregator1, aggregator2);
        }

        /// <summary>
        /// Verify that the Type Interceptor automatically registers an object with the <see cref="IEventAggregator"/>
        /// </summary>
        [TestMethod]
        public void IListener_TypeInterceptor_Test()
        {
            var listener = Container.GetInstance<TestListener>();
            var eventAggregator = Container.GetInstance<IEventAggregator>();

            eventAggregator.SendMessage(10);

            Assert.AreEqual(10, listener.Value);
        }        
    }

    /// <summary>
    /// Test class to unit test the type interceptor automatically registers an object with the <see cref="IEventAggregator"/> 
    /// </summary>
    public class TestListener : IListenFor<int>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public Task Handle(int message)
        {
            Value = message;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Value { get; set; }
    }
}
