using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EventAggregation.Async.Tests
{
    internal class ListenerA : IListenFor<ObservableA>
    {
        public int MessageAReceived { get; private set; }

        public Task Handle(ObservableA message)
        {
            MessageAReceived++;
            return Task.CompletedTask;
        }
    }

    [ExcludeFromCodeCoverage]
    internal class ListenerAB : IListenFor<ObservableA>, IListenFor<ObservableB>
    {

        public int MessageAReceived { get; private set; }

        public int MessageBReceived { get; private set; }

        public Task Handle(ObservableA message)
        {
            MessageAReceived++;
            return Task.CompletedTask;
        }

        public Task Handle(ObservableB message)
        {
            MessageBReceived++;
            return Task.CompletedTask;
        }
    }
}
