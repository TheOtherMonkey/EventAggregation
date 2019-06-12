using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventAggregation.Async
{
    public class EventAggregator : IEventAggregator
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
        /// Send a message to all appropriate registered listeners.
        /// </summary>
        /// <typeparam name="T">The type of message being sent.</typeparam>
        /// <param name="message">The message to be sent.</param>
        public async Task SendMessage<T>(T message)
        {         
            await SendMessage(message, GenericListenerType<T>());
        }        

        private async Task SendMessage<T>(T message, Type listenerType)
        {
            var subscriptions = GetSubscriptions(listenerType);
            var subscriptionsToRemove = await SendMessage(message, subscriptions);
            RemoveReleasedSubscriptions(subscriptionsToRemove, subscriptions);
        }

        /// <summary>
        /// Send the <paramref name="message"/> to all <paramref name="subscriptions"/>.
        /// </summary>
        /// <typeparam name="T">The type of message being sent.</typeparam>
        /// <param name="message">The message to be sent.</param>
        /// <param name="subscriptions">The subscribers who have registered an interest in the message being sent.</param>
        /// <returns>The list of any subscriptions that have been garbage collected and need to be removed.</returns>
        private static async Task<IEnumerable<WeakReference>> SendMessage<T>(T message, IEnumerable<WeakReference> subscriptions)
        {
            var subscriptionsToRemove = new List<WeakReference>();

            foreach (var subscription in subscriptions)
            {
                if (subscription.IsAlive)
                {
                    await SendMessage(message, subscription);
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
        private static async Task SendMessage<T>(T message, WeakReference subscription)
        {
            var listener = (IListenFor<T>)subscription.Target;
            //var syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            //syncContext.Send(s => listener.Handle(message), null);
            await listener.Handle(message);
        }

        private static Type GenericListenerType<T>()
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
        private void RemoveReleasedSubscriptions(IEnumerable<WeakReference> subscriptionsToRemove, ICollection<WeakReference> subscriptions)
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
