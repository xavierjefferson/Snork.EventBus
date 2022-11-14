using System;

namespace Snork.EventBus
{
    public sealed class Subscription
    {
        private volatile bool _active;

        public Subscription(object subscriber, SubscriberMethod subscriberMethod)
        {
            Subscriber = subscriber;
            SubscriberMethod = subscriberMethod;
            Active = true;
        }

        public object? Subscriber { get; }
        public SubscriberMethod? SubscriberMethod { get; }

        /// <summary>
        ///     Becomes false as soon as <see cref="EventBus.Unregister"/> is called, which is checked by queued message delivery
        ///   <see cref="EventBus.InvokeSubscriber(PendingPost)"/>  to prevent race conditions.
        /// </summary>
        public bool Active
        {
            get => _active;
            set => _active = value;
        }

        public override bool Equals(object other)
        {
            if (other is Subscription otherSubscription)
                return Subscriber == otherSubscription.Subscriber
                       && SubscriberMethod.Equals(otherSubscription.SubscriberMethod);

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Subscriber, SubscriberMethod);
        }
    }
}