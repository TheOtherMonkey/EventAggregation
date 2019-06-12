using StructureMap;
using StructureMap.Building.Interception;
using StructureMap.TypeRules;

namespace EventAggregation.Async.StructureMap
{
    /// <inheritdoc />
    /// <summary>
    /// StructureMap registry for the EventAggregation.Async objects.
    /// </summary>    
    /// <remarks>
    /// 1. Registers <see cref="IEventAggregator" /> of type <see cref="EventAggregator" /> as a singleton.
    /// 2. Registers <see cref="LoggedMessageBroker"/> as a singleton.
    /// 3. Creates a type interceptor so any object that implements the <see cref="IListenFor{T}" /> interface
    /// that is created via StructureMap will be added to the <see cref="IEventAggregator" />.
    /// </remarks>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class EventAggregationRegistry : Registry
    {
        /// <summary>
        /// Construct a new instance of the <see cref="EventAggregationRegistry"/>
        /// </summary>
        public EventAggregationRegistry()
        {
            ForSingletonOf<IEventAggregator>().Use<EventAggregator>();

            var interceptor = new ActivatorInterceptor<object>((c, s) => RegisterWithEventAggregator(c, s));
            Policies.Interceptors(interceptor.ToPolicy());
        }

        private static void RegisterWithEventAggregator(IContext context, object subject)
        {
            var subjectType = subject.GetType();
            if (!subjectType.ImplementsInterfaceTemplate(typeof(IListenFor<>))) return;

            var eventAggregator = context.GetInstance<IEventAggregator>();
            eventAggregator.AddListener(subject);
        }
    }
}
