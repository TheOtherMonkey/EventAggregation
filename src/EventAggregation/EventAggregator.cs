using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EventAggregation.Logging;

namespace EventAggregation
{
    /// <summary>
    /// A centralised in-memory event aggregator for utilising weak references to avoid the need for making the class implement <see cref="IDisposable"/>.
    /// In order for this component to be used successfully, there needs to be one and only one instance of the <see cref="EventAggregator"/> in existence
    /// during the lifetime of the application where the <see cref="EventAggregator"/> is being used. The easiest way to enforce the singleton requirement
    /// for this class is to utilise a Dependency Injection (DI) framework and declare the lifetime of the
    /// <see cref="EventAggregator"/> as a singleton.
    /// <para>
    /// In order to get the most out of this component, the DI framework should identify objects that implement the <see cref="IListenFor{T}"/> interface
    /// and automatically register the newly constructed object with the <see cref="EventAggregator"/> by making a call to the <see cref="AddListener"/> method
    /// prior to returning the new instance.
    /// </para>
    /// <para>
    /// Of course, this automagic registration of objects constructed by a DI framework is not a requirement for the use of the <see cref="EventAggregator"/> but
    /// it does make using it a lot more convenient. If no DI framework is being used (or the DI framework does not support custom type interception) the
    /// <see cref="AddListener"/> method needs to be called in order to register an object as a listener.
    /// </para>
    /// </summary>        
    public sealed class EventAggregator : IEventAggregator
    {
        /// <summary>
        /// Dictionary pairing <see cref="IListenFor{T}"/> to the collection of <see cref="object"/>'s that implement that interface
        /// </summary>
        internal readonly ConcurrentDictionary<Type, ICollection<WeakReference>> Subscriptions = new ConcurrentDictionary<Type, ICollection<WeakReference>>();
        private readonly object _locker = new object();

        /// <inheritdoc />
        /// <summary>
        /// Add a new listener to the collection of subscriptions using a <see cref="T:System.WeakReference" />.
        /// </summary>
        /// <param name="listener">The listener to be registered.</param>
        public void AddListener(object listener)
        {
            var reference = new WeakReference(listener);
            var listenerTypes = GetListenerTypes(listener);

            foreach (var listenerType in listenerTypes)
            {
                var subscribers = GetSubscriptions(listenerType);
                AddSubscription(reference, subscribers);
            }
        }

        private void AddSubscription(WeakReference reference, ICollection<WeakReference> subscribers)
        {
            lock (_locker)
            {
                if (!subscribers.Any(s => s.ReferenceEquals(reference.Target)))
                {
                    subscribers.Add(reference);
                }
            }
        }

        /// <summary>
        /// Determine whether a particular listener has been registered.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <returns>True if the listener has been registered, otherwise false.</returns>
        public bool HasListener(object listener)
        {
            var result = Subscriptions.Values.Any(s => s.Any(r => r.ReferenceEquals(listener)));
            return result;
        }

        /// <summary>
        /// Send a message to all appropriate registered listeners.
        /// </summary>
        /// <typeparam name="T">The type of message being sent.</typeparam>
        /// <param name="message">The message to be sent.</param>
        public void SendMessage<T>(T message)
        {
            LogMessage(message);
            SendMessage(message, GenericListnerType(message));
        }

        private void LogMessage<T>(T message)
        {
            if (!(message is IAmLogged loggedMessage)) return;
            SendMessage(loggedMessage, typeof(IListenFor<IAmLogged>));
        }

        private void SendMessage<T>(T message, Type listenerType)
        {
            var subscriptions = GetSubscriptions(listenerType);
            var zombieListeners = SendMessage(message, subscriptions);
            RemoveZombieSubscriptions(zombieListeners, subscriptions);
        }

        /// <summary>
        /// Send the <paramref name="message"/> to all <paramref name="subscriptions"/>.
        /// </summary>
        /// <typeparam name="T">The type of message being sent.</typeparam>
        /// <param name="message">The message to be sent.</param>
        /// <param name="subscriptions">The subscribers who have registered an interest in the message being sent.</param>
        /// <returns>The list of any subscriptions that have been garbage collected and need to be removed.</returns>
        private static IEnumerable<WeakReference> SendMessage<T>(T message, IEnumerable<WeakReference> subscriptions)
        {
            var subscriptionsToRemove = new List<WeakReference>();

            foreach (var subscription in subscriptions)
            {
                if (subscription.IsAlive)
                {
                    SendMessage(message, subscription);
                }
                else
                {
                    subscriptionsToRemove.Add(subscription);
                }
            }

            return subscriptionsToRemove;
        }

        /// <summary>
        /// Sends the message to the correct object.
        /// </summary>
        /// <typeparam name="T">The type of message being sent.</typeparam>
        /// <param name="message">The message being sent.</param>
        /// <param name="subscription">The weak link to the listener object.</param>
        private static void SendMessage<T>(T message, WeakReference subscription)
        {
            var listener = (IListenFor<T>)subscription.Target;
            var syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            syncContext.Send(s => listener.Handle(message), null);
        }        

        private static Type GenericListnerType<T>(T message)
        {
            return typeof(IListenFor<>).MakeGenericType(typeof(T));
        }
       
        /// <summary>
        /// Find all declarations of the <see cref="IListenFor{T}"/> interface that the <paramref name="listener"/> 
        /// implements.
        /// </summary>
        /// <param name="listener">The listener being registered.</param>
        /// <returns>The list of <see cref="IListenFor{T}"/> types the <paramref name="listener"/> implements.</returns>
        private static IEnumerable<Type> GetListenerTypes(object listener)
        {
            var listenerTypes = listener.GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IListenFor<>));

            return listenerTypes;
        }

        /// <summary>
        /// Find the collection of <see cref="WeakReference"/>'s from the <see cref="Subscriptions"/>
        /// for the <paramref name="listenerType"/>. If the type has not been seen before, a new entry
        /// for it is created in the <see cref="Subscriptions"/>.
        /// </summary>
        /// <param name="listenerType">The <see cref="IListenFor{T}"/> type whose collection of <see cref="WeakReference"/>'s are required.</param>
        /// <returns>The <see cref="ICollection{T}"/> of <see cref="WeakReference"/>'s.</returns>
        private ICollection<WeakReference> GetSubscriptions(Type listenerType)
        {
            return Subscriptions.GetOrAdd(listenerType, new List<WeakReference>());
        }

        /// <summary>
        /// Remove the listeners that have been garbage collected from the list of subscribers.
        /// </summary>
        /// <param name="subscriptionsToRemove">The list of listeners to be removed.</param>
        /// <param name="subscriptions">The list the listeners are to be removed from.</param>
        private void RemoveZombieSubscriptions(IEnumerable<WeakReference> subscriptionsToRemove, ICollection<WeakReference> subscriptions)
        {
            lock (_locker)
            {
                foreach (var subscription in subscriptionsToRemove)
                {
                    subscriptions.Remove(subscription);
                }
            }
        }
    }
}
