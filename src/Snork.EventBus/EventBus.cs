using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Snork.EventBus.Interfaces;

namespace Snork.EventBus
{
    /// <summary>
    ///     EventBus is a central publish/subscribe event system for C#.
    ///     Events are posted (<see cref="Post" />) to the bus, which delivers it to subscribers that have a matching handler
    ///     method for the event type.
    ///     To receive events, subscribers must register themselves to the bus using <see cref="Register" />.
    ///     Once registered, subscribers receive events until <see cref="Unregister" /> is called.
    ///     Event handling methods must be annotated by <see cref="Subscribe" />, must be public, return nothing (void),
    ///     and have exactly one parameter (the event).
    /// </summary>
    public class EventBus
    {
        private readonly object _mutex = new object();
        private static readonly EventBusBuilder DefaultBuilder = new EventBusBuilder();
        private static readonly Dictionary<Type, List<Type>> EventTypesCache = new Dictionary<Type, List<Type>>();

        internal static EventBus? _default;
        private readonly AsyncPoster? _asyncPoster;
        private readonly BackgroundPoster? _backgroundPoster;

        private readonly ThreadLocal<PostingThreadState> _currentPostingThreadState =
            new ThreadLocal<PostingThreadState>(
                () => new PostingThreadState());

        private readonly bool _eventInheritance;
        public IExecutor? Executor { get; }
        private readonly int _indexCount;
        private readonly bool _logNoSubscriberEvents;
        private readonly bool _logSubscriberExceptions;
        private readonly IPoster? _mainThreadPoster;
        private readonly IMainThreadSupport? _mainThreadSupport;
        private readonly bool _sendNoSubscriberEvent;
        private readonly bool _sendSubscriberExceptionEvent;
        private readonly Dictionary<Type, object> _stickyEvents;
        private readonly SubscriberMethodFinder? _subscriberMethodFinder;
        private readonly Dictionary<Type, ConcurrentList<Subscription>> _subscriptionsByEventType;
        private readonly bool _throwSubscriberException;
        private readonly Dictionary<object, List<Type>> _typesBySubscriber;

        /// <summary>
        ///     Creates a new EventBus instance; each instance is a separate scope in which events are delivered.To use a
        ///     central bus, consider <see cref="Default" />.
        /// </summary>
        public EventBus() : this(DefaultBuilder)
        {
        }

        public EventBus(EventBusBuilder builder)
        {
            Logger = builder.Logger;
            _subscriptionsByEventType = new Dictionary<Type, ConcurrentList<Subscription>>();
            _typesBySubscriber = new Dictionary<object, List<Type>>();
            _stickyEvents = new Dictionary<Type, object>();
            _mainThreadSupport = builder.MainThreadSupport;
            _mainThreadPoster = _mainThreadSupport?.CreatePoster(this);
            _backgroundPoster = new BackgroundPoster(this);
            _asyncPoster = new AsyncPoster(this);
            _indexCount = builder.SubscriberInfoIndexes.Count();
            _subscriberMethodFinder = new SubscriberMethodFinder(builder.SubscriberInfoIndexes,
                builder.StrictMethodVerification, builder.IgnoreGeneratedIndex);
            _logSubscriberExceptions = builder.LogSubscriberExceptions;
            _logNoSubscriberEvents = builder.LogNoSubscriberEvents;
            _sendSubscriberExceptionEvent = builder.SendSubscriberExceptionEvent;
            _sendNoSubscriberEvent = builder.SendNoSubscriberEvent;
            _throwSubscriberException = builder.ThrowSubscriberException;
            _eventInheritance = builder.EventInheritance;
            Executor = builder.Executor;
        }

        public ILogger Logger { get; }

        public static EventBus? Default
        {
            get
            {
                var instance = _default;
                if (instance != null) return instance;
                lock (typeof(EventBus))
                {
                    instance = _default ??= new EventBus();
                }

                return instance;
            }
        }

        public static EventBusBuilder Builder()
        {
            return new EventBusBuilder();
        }

        /// <summary>
        ///     For unit test primarily.
        /// </summary>
        public static void ClearCaches()
        {
            SubscriberMethodFinder.ClearCaches();
            lock (EventTypesCache)
            {
                EventTypesCache.Clear();
            }
        }

        /// <summary>
        ///     Registers the given subscriber to receive events. Subscribers must call <see cref="Unregister" /> once they
        ///     are no longer interested in receiving events.
        ///     Subscribers have event handling methods that must be annotated by <see cref="SubscribeAttribute" />.
        ///     The <see cref="SubscribeAttribute" /> annotation also allows configuration like <see cref="ThreadModeEnum" /> and
        ///     priority.
        /// </summary>
        public void Register(object subscriber)
        {
            var subscriberType = subscriber.GetType();
            var subscriberMethods = _subscriberMethodFinder.FindSubscriberMethods(subscriberType);
            lock (_mutex)
            {
                foreach (var subscriberMethod in subscriberMethods) Subscribe(subscriber, subscriberMethod);
            }
        }


        /// <summary>
        ///Must be called in synchronized block 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="subscriberMethod"></param>
        /// <exception cref="EventBusException"></exception>
        private void Subscribe(object subscriber, SubscriberMethod subscriberMethod)
        {
            var eventType = subscriberMethod.EventType;
            var newSubscription = new Subscription(subscriber, subscriberMethod);
            var subscriptions = _subscriptionsByEventType.ContainsKey(eventType)
                ? _subscriptionsByEventType[eventType]
                : default;
            if (subscriptions == null)
            {
                subscriptions = new ConcurrentList<Subscription>();
                _subscriptionsByEventType[eventType] = subscriptions;
            }
            else
            {
                if (subscriptions.Contains(newSubscription))
                    throw new EventBusException(
                        $"Subscriber {subscriber.GetType().FullName} is already registered to event {eventType.FullName}");
            }

            if (!subscriptions.Any())
            {
                subscriptions.Add(newSubscription);
            }
            else
            {
 
                var count = subscriptions.Count();
                //insert in priority order.
                for (int i = 0; i <= count; i++)
                {
                    if (i == count || subscriberMethod.Priority < subscriptions[i].SubscriberMethod.Priority)
                    {
                        subscriptions.Insert(i, newSubscription);
                        break;
                    }
                }
                
            }

            List<Type>? subscribedEventTypes;
            if (_typesBySubscriber.ContainsKey(subscriber))
            {
                subscribedEventTypes = _typesBySubscriber[subscriber];
            }
            else
            {
                subscribedEventTypes = new List<Type>();
                _typesBySubscriber[subscriber] = subscribedEventTypes;
            }

            subscribedEventTypes.Add(eventType);

            if (subscriberMethod.Sticky)
            {
                if (_eventInheritance)
                {
                    // Existing sticky events of all subclasses of eventType have to be considered.
                    // Note: Iterating over all events may be inefficient with lots of sticky events,
                    // thus data structure should be changed to allow a more efficient lookup
                    // (e.g. an additional map storing sub classes of super classes: Type -> List<Type>).

                    foreach (var entry in _stickyEvents)
                    {
                        var candidateEventType = entry.Key;
                        if (eventType.IsAssignableFrom(candidateEventType))
                        {
                            var stickyEvent = entry.Value;
                            CheckPostStickyEventToSubscription(newSubscription, stickyEvent);
                        }
                    }
                }
                else
                {
                    var stickyEvent = _stickyEvents.ContainsKey(eventType) ? _stickyEvents[eventType] : default;
                    CheckPostStickyEventToSubscription(newSubscription, stickyEvent);
                }
            }
        }

        private void CheckPostStickyEventToSubscription(Subscription newSubscription, object? stickyEvent)
        {
            if (stickyEvent != null)
                // If the subscriber is trying to abort the event, it will fail (event is not tracked in posting state)
                // --> Strange corner case, which we don't take care of here.
                PostToSubscription(newSubscription, stickyEvent, IsMainThread());
        }

        /// <summary>
        ///     Checks if the current thread is running in the main thread.
        ///     If there is no configured implementation of implementation of <see cref="Interfaces.IMainThreadSupport"/>, "true" is always returned. In that case MAIN thread
        ///     subscribers are always called in posting thread, and BACKGROUND subscribers are always called from a background
        ///     poster.
        /// </summary>

        private bool IsMainThread()
        {
            return _mainThreadSupport == null || _mainThreadSupport.IsMainThread();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsRegistered(object subscriber)
        {
            return _typesBySubscriber.ContainsKey(subscriber);
        }

        /// <summary>
        ///     Only updates subscriptionsByEventType, not typesBySubscriber! Caller must update <see cref="_typesBySubscriber"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void UnsubscribeByEventType(object subscriber, Type eventType)
        {
            lock (_mutex)
            {
                var subscriptions = _subscriptionsByEventType.ContainsKey(eventType)
                    ? _subscriptionsByEventType[eventType]
                    : default;
                if (subscriptions != null) subscriptions.RemoveAll(i => i.Subscriber == subscriber);
            }
        }

        /// <summary>
        ///     Unregisters the given subscriber from all event classes.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Unregister(object subscriber)
        {
            var subscribedTypes = _typesBySubscriber.ContainsKey(subscriber) ? _typesBySubscriber[subscriber] : null;
            if (subscribedTypes != null)
            {
                foreach (var eventType in subscribedTypes) UnsubscribeByEventType(subscriber, eventType);
                _typesBySubscriber.Remove(subscriber);
            }
            else
            {
                Logger.LogWarning($"Subscriber to unregister was not registered before: {subscriber.GetType().FullName}");
            }
        }

        /// <summary>
        ///     Posts the given events to the event bus.
        /// </summary>
        public void Post(params object[] events)
        {
            var postingState = _currentPostingThreadState.Value;
            var eventQueue = postingState.EventQueue;
            foreach (var @event in events)
                eventQueue.Enqueue(@event);


            if (postingState.IsPosting) return;

            postingState.IsMainThread = IsMainThread();
            postingState.IsPosting = true;
            if (postingState.Canceled) throw new EventBusException("Internal error. Abort state was not reset");
            try
            {
                while (eventQueue.Any()) PostSingleEvent(eventQueue.Dequeue(), postingState);
            }
            finally
            {
                postingState.IsPosting = false;
                postingState.IsMainThread = false;
            }
        }

        /// <summary>
        ///     Called from a subscriber's event handling method, further event delivery will be canceled. Subsequent
        ///     subscribers
        ///     won't receive the event. Events are usually canceled by higher priority subscribers (see
        ///     <see cref="SubscribeAttribute.Priority" />)
        ///     Canceling is restricted to event handling methods running in posting thread
        ///     <see cref="ThreadModeEnum.Posting" />.
        /// </summary>
        public void CancelEventDelivery(object? @event)
        {
            var postingState = _currentPostingThreadState.Value;
            if (!postingState.IsPosting)
                throw new EventBusException(
                    "This method may only be called from inside event handling methods on the posting thread");
            if (@event == null)
                throw new EventBusException("Event may not be null");
            if (postingState.Event != @event)
                throw new EventBusException("Only the currently handled event may be aborted");
            if (postingState.Subscription.SubscriberMethod.ThreadMode != ThreadModeEnum.Posting)
                throw new EventBusException(" event handlers may only abort the incoming event");

            postingState.Canceled = true;
        }

        /// <summary>
        ///     Posts the given event to the event bus and holds on to the event (because it is sticky). The most recent
        ///     sticky
        ///     event of an event's type is kept in memory for future access by subscribers using
        ///     <see cref="SubscribeAttribute.Sticky" />.
        /// </summary>
        public void PostSticky(object @event)
        {
            lock (_stickyEvents)
            {
                _stickyEvents[@event.GetType()] = @event;
            }

            // Should be posted after it is putted, in case the subscriber wants to Remove immediately
            Post(@event);
        }

        /// <summary>
        ///     Gets the most recent sticky event for the given type. See <see cref="PostSticky" />
        /// </summary>
        public object? GetStickyEvent(Type eventType)
        {
            lock (_stickyEvents)
            {
                return _stickyEvents.ContainsKey(eventType) ? _stickyEvents[eventType] : default;
            }
        }

        /// <summary>
        ///     Gets the most recent sticky event for the given type.
        ///     See <see cref="PostSticky" />
        /// </summary>
        public T GetStickyEvent<T>()
        {
            lock (_stickyEvents)
            {
                var eventType = typeof(T);
                return _stickyEvents.ContainsKey(eventType) ? (T)_stickyEvents[eventType] : default;
            }
        }

        /// <summary>
        ///     Remove and gets the recent sticky event for the given event type.
        ///     See <see cref="PostSticky" />
        /// </summary>
        public object? RemoveStickyEvent(Type eventType)
        {
            lock (_stickyEvents)
            {
                if (_stickyEvents.ContainsKey(eventType))
                {
                    var value = _stickyEvents[eventType];
                    _stickyEvents.Remove(eventType);
                    return value;
                }

                return null;
            }
        }

        /// <summary>
        ///     Remove and gets the recent sticky event for the given event type.
        ///     See <see cref="PostSticky" />
        /// </summary>
        public T RemoveStickyEvent<T>()
        {
            lock (_stickyEvents)
            {
                var eventType = typeof(T);
                if (_stickyEvents.ContainsKey(eventType))
                {
                    var result = _stickyEvents[eventType];
                    _stickyEvents.Remove(eventType);
                    return (T)result;
                }

                return default;
            }
        }

        /// <summary>
        ///     Removes the sticky event if it equals to the given event.
        ///     @return true if the events matched and the sticky event was removed.
        /// </summary>
        public bool RemoveStickyEvent(object @event)
        {
            lock (_mutex)
            {
                var eventType = @event.GetType();
                var existingEvent = _stickyEvents.ContainsKey(eventType) ? _stickyEvents[eventType] : default;
                if (@event.Equals(existingEvent))
                {
                    _stickyEvents.Remove(eventType);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Removes all sticky events.
        /// </summary>
        public void RemoveAllStickyEvents()
        {
            lock (_mutex)
            {
                _stickyEvents.Clear();
            }
        }

        public bool HasSubscriberForEvent<T>()
        {
            return HasSubscriberForEvent(typeof(T));
        }

        public bool HasSubscriberForEvent(Type eventType)
        {
            var eventTypes = LookupAllEventTypes(eventType);


            foreach (var type in eventTypes)
            {
                ConcurrentList<Subscription>? subscriptions;
                lock (_mutex)
                {
                    subscriptions = _subscriptionsByEventType.ContainsKey(type)
                        ? _subscriptionsByEventType[type]
                        : default;
                }

                if (subscriptions != null && subscriptions.Any()) return true;
            }


            return false;
        }

        private void PostSingleEvent(object @event, PostingThreadState postingState)
        {
            var eventType = @event.GetType();
            var subscriptionFound = false;
            if (_eventInheritance)
            {
                var eventTypes = LookupAllEventTypes(eventType);
                foreach (var type in eventTypes)
                    subscriptionFound |= PostSingleEventForEventType(@event, postingState, type);
            }
            else
            {
                subscriptionFound = PostSingleEventForEventType(@event, postingState, eventType);
            }

            if (!subscriptionFound)
            {
                if (_logNoSubscriberEvents)
                    Logger.LogDebug($"No subscribers registered for event type {eventType.FullName}");
                if (_sendNoSubscriberEvent && eventType != typeof(NoSubscriberEvent) &&
                    eventType != typeof(SubscriberExceptionEvent))
                    Post(new NoSubscriberEvent(this, @event));
            }
        }

        private bool PostSingleEventForEventType(object @event, PostingThreadState postingState, Type eventType)
        {
            ConcurrentList<Subscription>? subscriptions;
            lock (_mutex)
            {
                subscriptions = _subscriptionsByEventType.ContainsKey(eventType)
                    ? _subscriptionsByEventType[eventType]
                    : default;
            }

            if (subscriptions != null && subscriptions.Any())
            {
                foreach (var subscription in subscriptions)
                {
                    postingState.Event = @event;
                    postingState.Subscription = subscription;
                    bool aborted;
                    try
                    {
                        PostToSubscription(subscription, @event, postingState.IsMainThread);
                        aborted = postingState.Canceled;
                    }
                    finally
                    {
                        postingState.Event = null;
                        postingState.Subscription = null;
                        postingState.Canceled = false;
                    }

                    if (aborted) break;
                }

                return true;
            }

            return false;
        }

        private void PostToSubscription(Subscription subscription, object @event, bool isMainThread)
        {
            switch (subscription.SubscriberMethod.ThreadMode)
            {
                case ThreadModeEnum.Posting:
                    InvokeSubscriber(subscription, @event);
                    break;
                case ThreadModeEnum.Main:
                    if (isMainThread)
                        InvokeSubscriber(subscription, @event);
                    else
                        _mainThreadPoster.Enqueue(subscription, @event);
                    break;
                case ThreadModeEnum.MainOrdered:
                    if (_mainThreadPoster != null)
                        _mainThreadPoster.Enqueue(subscription, @event);
                    else
                        // temporary: technically not correct as poster not decoupled from subscriber
                        InvokeSubscriber(subscription, @event);
                    break;
                case ThreadModeEnum.Background:
                    if (isMainThread)
                        _backgroundPoster.Enqueue(subscription, @event);
                    else
                        InvokeSubscriber(subscription, @event);
                    break;
                case ThreadModeEnum.Async:
                    _asyncPoster.Enqueue(subscription, @event);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unknown thread mode: {subscription.SubscriberMethod.ThreadMode}");
            }
        }

        /// <summary>
        ///     Looks up all Type objects including super classes and interfaces.Should also work for interfaces.
        /// </summary>
        private static List<Type> LookupAllEventTypes(Type eventType)
        {
            lock (EventTypesCache)
            {
                var eventTypes = EventTypesCache.ContainsKey(eventType)
                    ? EventTypesCache[eventType]
                    : default;
                if (eventTypes == null)
                {
                    eventTypes = new List<Type>();
                    var type = eventType;
                    while (type != null)
                    {
                        eventTypes.Add(type);
                        AddInterfaces(eventTypes, type.GetInterfaces());
                        type = type.BaseType;
                    }

                    EventTypesCache[eventType] = eventTypes;
                }

                return eventTypes;
            }
        }

        /// <summary>
        ///     Recurses through super interfaces.
        /// </summary>
        private static void AddInterfaces(List<Type> eventTypes, Type[] interfaces)
        {
            foreach (var interfaceType in interfaces)
                if (!eventTypes.Contains(interfaceType))
                {
                    eventTypes.Add(interfaceType);
                    AddInterfaces(eventTypes, interfaceType.GetInterfaces());
                }
        }

        /// <summary>
        ///     Invokes the subscriber if the subscriptions is still active. Skipping subscriptions prevents race conditions
        ///     between <see cref="Unregister" /> and event delivery. Otherwise the event might be delivered after the
        ///     subscriber unregistered.This is particularly important for main thread delivery and registrations bound to the
        ///     live cycle of an Activity or Fragment.
        /// </summary>
        public object? InvokeSubscriber(PendingPost pendingPost)
        {
            var @event = pendingPost.Event;
            var subscription = pendingPost.Subscription;
            PendingPost.ReleasePendingPost(pendingPost);
            if (subscription != null && subscription.Active) return InvokeSubscriber(subscription, @event);
            return null;
        }

        public object? InvokeSubscriber(Subscription subscription, object @event)
        {
            try
            {
                return subscription.SubscriberMethod.MethodInfo.Invoke(subscription.Subscriber, new[] { @event });
            }
            catch (TargetInvocationException e)
            {
                HandleSubscriberException(subscription, @event, e.InnerException);
                return null;
            }
        }

        private void HandleSubscriberException(Subscription subscription, object @event, Exception cause)
        {
            if (@event is SubscriberExceptionEvent exceptionEvent)
            {
                if (_logSubscriberExceptions)
                {
                    // Don't send another SubscriberExceptionEvent to avoid infinite event recursion, just log
                    Logger.LogCritical(
                        $"SubscriberExceptionEvent subscriber {subscription.Subscriber.GetType().FullName} threw an exception",
                        cause);
                    Logger.LogCritical(
                        $"Initial event {exceptionEvent.OriginalEvent} caused exception in {exceptionEvent.OriginalSubscriber}, {exceptionEvent.Exception.Message}");
                }
            }
            else
            {
                if (_throwSubscriberException) throw new EventBusException("Invoking subscriber failed", cause);
                if (_logSubscriberExceptions)
                    Logger.LogCritical(
                        $"Could not dispatch event: {@event.GetType().FullName} to subscribing class {subscription.Subscriber.GetType().FullName}",
                        cause);
                if (_sendSubscriberExceptionEvent)
                {
                    var exEvent = new SubscriberExceptionEvent(this, cause, @event,
                        subscription.Subscriber);
                    Post(exEvent);
                }
            }
        }

        public override string ToString()
        {
            return $"EventBus[indexCount={_indexCount}, eventInheritance={_eventInheritance}]";
        }

        /// <summary>
        ///     For ThreadLocal, much faster to set (and get multiple values).
        /// </summary>
        private class PostingThreadState
        {
            public Queue<object> EventQueue { get; } = new Queue<object>();
            public bool Canceled { get; set; }
            public bool IsMainThread { get; set; }
            public bool IsPosting { get; set; }
            public object? Event { get; set; }
            public Subscription? Subscription { get; set; }
        }

        // Just an idea: we could provide a callback to post() to be notified, an alternative would be events, of course...
        //// public ////
        private interface PostCallback
        {
            void onPostCompleted(List<SubscriberExceptionEvent> exceptionEvents);
        }
    }
}