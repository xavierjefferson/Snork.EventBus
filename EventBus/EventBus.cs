using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Snork.EventBus
{
    /// <summary>
    ///     EventBus is a central publish/subscribe message system for C#.
    ///     Events are posted (<see cref="Post" />) to the bus, which delivers it to subscribers that have a matching handler
    ///     method for the message type.
    ///     To receive events, subscribers must register themselves to the bus using <see cref="Register" />.
    ///     Once registered, subscribers receive events until <see cref="Unregister" /> is called.
    ///     Event handling methods must be annotated by <see cref="Subscribe" />, must be public, return nothing (void),
    ///     and have exactly one parameter (the message).
    ///     @author Markus Junginger, greenrobot
    /// </summary>
    public class EventBus
    {
        private static readonly EventBusBuilder DEFAULT_BUILDER = new EventBusBuilder();
        private static readonly Dictionary<Type, List<Type>> MessageTypesCache = new Dictionary<Type, List<Type>>();

        internal static EventBus? _default;
        private readonly AsyncPoster? _asyncPoster;
        private readonly BackgroundPoster? _backgroundPoster;

        private readonly ThreadLocal<PostingThreadState> _currentPostingThreadState =
            new ThreadLocal<PostingThreadState>(
                () => new PostingThreadState());

        private readonly bool _eventInheritance;
        private readonly ExecutorService? _executorService;

        private readonly int _indexCount;
        private readonly bool _logNoSubscriberMessages;

        private readonly bool _logSubscriberExceptions;

        // @Nullable
        private readonly IPoster? _mainThreadPoster;


        // @Nullable
        private readonly IMainThreadSupport? _mainThreadSupport;
        private readonly bool _sendNoSubscriberMessage;
        private readonly bool _sendSubscriberExceptionMessage;
        private readonly Dictionary<Type, object> _stickyEvents;
        private readonly SubscriberMethodFinder? _subscriberMethodFinder;

        private readonly Dictionary<Type, ConcurrentList<Subscription>> _subscriptionsByEventType;

        private readonly bool _throwSubscriberException;
        private readonly Dictionary<object, List<Type>> _typesBySubscriber;

        /// <summary>
        ///     Creates a new EventBus instance; each instance is a separate scope in which events are delivered.To use a
        ///     central bus, consider <see cref="Default" />.
        /// </summary>
        public EventBus() : this(DEFAULT_BUILDER)
        {
        }

        public EventBus(EventBusBuilder builder)
        {
            Logger = builder.GetLogger();
            _subscriptionsByEventType = new Dictionary<Type, ConcurrentList<Subscription>>();
            _typesBySubscriber = new Dictionary<object, List<Type>>();
            _stickyEvents = new Dictionary<Type, object>();
            _mainThreadSupport = builder.GetMainThreadSupport();
            _mainThreadPoster = _mainThreadSupport != null ? _mainThreadSupport.CreatePoster(this) : null;
            _backgroundPoster = new BackgroundPoster(this);
            _asyncPoster = new AsyncPoster(this);
            _indexCount = builder.SubscriberInfoIndexes.Count();
            _subscriberMethodFinder = new SubscriberMethodFinder(builder.SubscriberInfoIndexes,
                builder.StrictMethodVerification, builder.IgnoreGeneratedIndex);
            _logSubscriberExceptions = builder.LogSubscriberExceptions;
            _logNoSubscriberMessages = builder.LogNoSubscriberMessages;
            _sendSubscriberExceptionMessage = builder.SendSubscriberExceptionEvent;
            _sendNoSubscriberMessage = builder.SendNoSubscriberEvent;
            _throwSubscriberException = builder.ThrowSubscriberException;
            _eventInheritance = builder.EventInheritance;
            _executorService = builder._executorService;
        }

        public Logger Logger { get; }

        public static EventBus? Default
        {
            get
            {
                var instance = _default;
                if (instance == null)
                    lock (typeof(EventBus))
                    {
                        instance = _default;
                        if (instance == null) instance = _default = new EventBus();
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
            SubscriberMethodFinder.clearCaches();
            MessageTypesCache.Clear();
        }

        /// <summary>
        ///     Registers the given subscriber to receive events. Subscribers must call <see cref="Unregister" /> once they
        ///     are no longer interested in receiving events.
        ///     Subscribers have message handling methods that must be annotated by <see cref="SubscribeAttribute" />.
        ///     The <see cref="SubscribeAttribute" /> annotation also allows configuration like <see cref="ThreadModeEnum" /> and
        ///     priority.
        /// </summary>
        public void Register(object subscriber)
        {
            var subscriberClass = subscriber.GetType();
            var subscriberMethods = _subscriberMethodFinder.FindSubscriberMethods(subscriberClass);
            lock (this)
            {
                foreach (var subscriberMethod in subscriberMethods) Subscribe(subscriber, subscriberMethod);
            }
        }

        public void Register<T>(Action<T> method, ThreadModeEnum threadMode = ThreadModeEnum.Posting, int priority = 5, bool sticky = false)
        {
            
            SubscriberMethod m = new SubscriberMethod( method.Method, typeof(T), threadMode,priority, sticky);
            lock (this)
            {
                Subscribe(method.Method.DeclaringType, m);
            }
        }
        // Must be called in synchronized block
        private void Subscribe(object subscriber, SubscriberMethod subscriberMethod)
        {
            var messageType = subscriberMethod.EventType;
            var newSubscription = new Subscription(subscriber, subscriberMethod);
            var subscriptions = _subscriptionsByEventType.ContainsKey(messageType)
                ? _subscriptionsByEventType[messageType]
                : default;
            if (subscriptions == null)
            {
                subscriptions = new ConcurrentList<Subscription>();
                _subscriptionsByEventType[messageType] = subscriptions;
            }
            else
            {
                if (subscriptions.Contains(newSubscription))
                    throw new EventBusException(
                        $"Subscriber {subscriber.GetType().FullName} is already registered to message {messageType.FullName}");
            }

            if (!subscriptions.Any())
            {
                subscriptions.Add(newSubscription);
            }
            else
            {
                var size = subscriptions.Count();
                foreach (var item in subscriptions.Select((subscription, index) =>
                             new { Subscription = subscription, Index = index }))
                    if (item.Index == size - 1 ||
                        subscriberMethod.Priority > item.Subscription.SubscriberMethod.Priority)
                    {
                        subscriptions.Insert(item.Index, newSubscription);
                        break;
                    }
            }

            List<Type>? subscribedMessageTypes;
            if (_typesBySubscriber.ContainsKey(subscriber))
            {
                subscribedMessageTypes = _typesBySubscriber[subscriber];
            }
            else
            {
                subscribedMessageTypes = new List<Type>();
                _typesBySubscriber[subscriber] = subscribedMessageTypes;
            }

            subscribedMessageTypes.Add(messageType);

            if (subscriberMethod.Sticky)
            {
                if (_eventInheritance)
                {
                    // Existing sticky events of all subclasses of messageType have to be considered.
                    // Note: Iterating over all events may be inefficient with lots of sticky events,
                    // thus data structure should be changed to allow a more efficient lookup
                    // (e.g. an additional map storing sub classes of super classes: Type -> List<Type>).

                    foreach (var entry in _stickyEvents)
                    {
                        var candidateEventType = entry.Key;
                        if (messageType.IsAssignableFrom(candidateEventType))
                        {
                            var stickyEvent = entry.Value;
                            CheckPostStickyEventToSubscription(newSubscription, stickyEvent);
                        }
                    }
                }
                else
                {
                    var stickyEvent = _stickyEvents.ContainsKey(messageType) ? _stickyEvents[messageType] : default;
                    CheckPostStickyEventToSubscription(newSubscription, stickyEvent);
                }
            }
        }

        private void CheckPostStickyEventToSubscription(Subscription newSubscription, object? stickyEvent)
        {
            if (stickyEvent != null)
                // If the subscriber is trying to abort the message, it will fail (message is not tracked in posting state)
                // --> Strange corner case, which we don't take care of here.
                PostToSubscription(newSubscription, stickyEvent, IsMainThread());
        }

        /// <summary>
        ///     Checks if the current thread is running in the main thread.
        ///     If there is no main thread support (e.g. non-Android), "true" is always returned. In that case MAIN thread
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
        ///     Only updates subscriptionsByEventType, not typesBySubscriber! Caller must update typesBySubscriber.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void UnsubscribeByEventType(object subscriber, Type messageType)
        {
            lock (this)
            {
                var subscriptions = _subscriptionsByEventType.ContainsKey(messageType)
                    ? _subscriptionsByEventType[messageType]
                    : default;
                if (subscriptions != null) subscriptions.RemoveAll(i => i.Subscriber == subscriber);
            }
        }

        /// <summary>
        ///     Unregisters the given subscriber from all message classes.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Unregister(object subscriber)
        {
            var subscribedTypes = _typesBySubscriber[subscriber];
            if (subscribedTypes != null)
            {
                foreach (var messageType in subscribedTypes) UnsubscribeByEventType(subscriber, messageType);
                _typesBySubscriber.Remove(subscriber);
            }
            else
            {
                Logger.log(LogLevel.Warning,
                    $"Subscriber to unregister was not registered before: {subscriber.GetType().FullName}");
            }
        }

        /// <summary>
        ///     Posts the given message to the message bus.
        /// </summary>
        public void Post(params object[] messages)
        {
            var postingState = _currentPostingThreadState.Value;
            var eventQueue = postingState.EventQueue;
            foreach (var message in messages)
                eventQueue.Enqueue(message);


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
        ///     Called from a subscriber's message handling method, further message delivery will be canceled. Subsequent
        ///     subscribers
        ///     won't receive the message. Events are usually canceled by higher priority subscribers (see
        ///     <see cref="SubscribeAttribute.Priority" />)
        ///     Canceling is restricted to message handling methods running in posting thread
        ///     <see cref="ThreadModeEnum.Posting" />.
        /// </summary>
        public void CancelEventDelivery(object? message)
        {
            var postingState = _currentPostingThreadState.Value;
            if (!postingState.IsPosting)
                throw new EventBusException(
                    "This method may only be called from inside message handling methods on the posting thread");
            if (message == null)
                throw new EventBusException("Event may not be null");
            if (postingState.Message != message)
                throw new EventBusException("Only the currently handled message may be aborted");
            if (postingState.Subscription.SubscriberMethod.ThreadMode != ThreadModeEnum.Posting)
                throw new EventBusException(" message handlers may only abort the incoming message");

            postingState.Canceled = true;
        }

        /// <summary>
        ///     Posts the given message to the message bus and holds on to the message (because it is sticky). The most recent
        ///     sticky
        ///     message of an message's type is kept in memory for future access by subscribers using
        ///     <see cref="SubscribeAttribute.Sticky" />.
        /// </summary>
        public void PostSticky(object message)
        {
            lock (_stickyEvents)
            {
                _stickyEvents[message.GetType()] = message;
            }

            // Should be posted after it is putted, in case the subscriber wants to Remove immediately
            Post(message);
        }

        /// <summary>
        ///     Gets the most recent sticky message for the given type. See <see cref="PostSticky" />
        /// </summary>
        public object? GetStickyEvent(Type messageType)
        {
            lock (_stickyEvents)
            {
                return _stickyEvents.ContainsKey(messageType) ? _stickyEvents[messageType] : default;
            }
        }

        /// <summary>
        ///     Gets the most recent sticky message for the given type.
        ///     See <see cref="PostSticky" />
        /// </summary>
        public T GetStickyEvent<T>()
        {
            lock (_stickyEvents)
            {
                var messageType = typeof(T);
                return _stickyEvents.ContainsKey(messageType) ? (T)_stickyEvents[messageType] : default;
            }
        }

        /// <summary>
        ///     Remove and gets the recent sticky message for the given message type.
        ///     See <see cref="PostSticky" />
        /// </summary>
        public object? RemoveStickyEvent(Type messageType)
        {
            lock (_stickyEvents)
            {
                if (_stickyEvents.ContainsKey(messageType))
                {
                    var value = _stickyEvents[messageType];
                    _stickyEvents.Remove(messageType);
                    return value;
                }

                return null;
            }
        }

        /// <summary>
        ///     Remove and gets the recent sticky message for the given message type.
        ///     See <see cref="PostSticky" />
        /// </summary>
        public T RemoveStickyEvent<T>()
        {
            lock (_stickyEvents)
            {
                var messageType = typeof(T);
                if (_stickyEvents.ContainsKey(messageType))
                {
                    var result = _stickyEvents[messageType];
                    _stickyEvents.Remove(messageType);
                    return (T)result;
                }

                return default;
            }
        }

        /// <summary>
        ///     Removes the sticky message if it equals to the given message.
        ///     @return true if the events matched and the sticky message was removed.
        /// </summary>
        public bool RemoveStickyEvent(object message)
        {
            lock (_stickyEvents)
            {
                var messageType = message.GetType();
                var existingEvent = _stickyEvents.ContainsKey(messageType) ? _stickyEvents[messageType] : default;
                if (message.Equals(existingEvent))
                {
                    _stickyEvents.Remove(messageType);
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
            lock (_stickyEvents)
            {
                _stickyEvents.Clear();
            }
        }

        public bool HasSubscriberForEvent<T>()
        {
            return HasSubscriberForEvent(typeof(T));
        }

        public bool HasSubscriberForEvent(Type messageClass)
        {
            var messageTypes = LookupAllMessageTypes(messageClass);


            foreach (var type in messageTypes)
            {
                ConcurrentList<Subscription>? subscriptions;
                lock (this)
                {
                    subscriptions = _subscriptionsByEventType.ContainsKey(type)
                        ? _subscriptionsByEventType[type]
                        : default;
                }

                if (subscriptions != null && subscriptions.Any()) return true;
            }


            return false;
        }

        private void PostSingleEvent(object message, PostingThreadState postingState)
        {
            var messageType = message.GetType();
            var subscriptionFound = false;
            if (_eventInheritance)
            {
                var messageTypes = LookupAllMessageTypes(messageType);
                foreach (var type in messageTypes)
                    subscriptionFound |= PostSingleEventForEventType(message, postingState, type);
            }
            else
            {
                subscriptionFound = PostSingleEventForEventType(message, postingState, messageType);
            }

            if (!subscriptionFound)
            {
                if (_logNoSubscriberMessages)
                    Logger.log(LogLevel.Debug, $"No subscribers registered for message type {messageType.FullName}");
                if (_sendNoSubscriberMessage && messageType != typeof(NoSubscriberMessage) &&
                    messageType != typeof(SubscriberExceptionMessage))
                    Post(new NoSubscriberMessage(this, message));
            }
        }

        private bool PostSingleEventForEventType(object message, PostingThreadState postingState, Type messageType)
        {
            ConcurrentList<Subscription>? subscriptions;
            lock (this)
            {
                subscriptions = _subscriptionsByEventType.ContainsKey(messageType)
                    ? _subscriptionsByEventType[messageType]
                    : default;
            }

            if (subscriptions != null && subscriptions.Any())
            {
                foreach (var subscription in subscriptions.ToList())
                {
                    postingState.Message = message;
                    postingState.Subscription = subscription;
                    bool aborted;
                    try
                    {
                        PostToSubscription(subscription, message, postingState.IsMainThread);
                        aborted = postingState.Canceled;
                    }
                    finally
                    {
                        postingState.Message = null;
                        postingState.Subscription = null;
                        postingState.Canceled = false;
                    }

                    if (aborted) break;
                }

                return true;
            }

            return false;
        }

        private void PostToSubscription(Subscription subscription, object message, bool isMainThread)
        {
            switch (subscription.SubscriberMethod.ThreadMode)
            {
                case ThreadModeEnum.Posting:
                    InvokeSubscriber(subscription, message);
                    break;
                case ThreadModeEnum.Main:
                    if (isMainThread)
                        InvokeSubscriber(subscription, message);
                    else
                        _mainThreadPoster.Enqueue(subscription, message);
                    break;
                case ThreadModeEnum.MainOrdered:
                    if (_mainThreadPoster != null)
                        _mainThreadPoster.Enqueue(subscription, message);
                    else
                        // temporary: technically not correct as poster not decoupled from subscriber
                        InvokeSubscriber(subscription, message);
                    break;
                case ThreadModeEnum.Background:
                    if (isMainThread)
                        _backgroundPoster.Enqueue(subscription, message);
                    else
                        InvokeSubscriber(subscription, message);
                    break;
                case ThreadModeEnum.Async:
                    _asyncPoster.Enqueue(subscription, message);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unknown thread mode: {subscription.SubscriberMethod.ThreadMode}");
            }
        }

        /// <summary>
        ///     Looks up all Type objects including super classes and interfaces.Should also work for interfaces.
        /// </summary>
        private static List<Type> LookupAllMessageTypes(Type messageClass)
        {
            lock (MessageTypesCache)
            {
                var messageTypes = MessageTypesCache.ContainsKey(messageClass)
                    ? MessageTypesCache[messageClass]
                    : default;
                if (messageTypes == null)
                {
                    messageTypes = new List<Type>();
                    var type = messageClass;
                    while (type != null)
                    {
                        messageTypes.Add(type);
                        AddInterfaces(messageTypes, type.GetInterfaces());
                        type = type.BaseType;
                    }

                    MessageTypesCache[messageClass] = messageTypes;
                }

                return messageTypes;
            }
        }

        /// <summary>
        ///     Recurses through super interfaces.
        /// </summary>
        private static void AddInterfaces(List<Type> messageTypes, Type[] interfaces)
        {
            foreach (var interfaceClass in interfaces)
                if (!messageTypes.Contains(interfaceClass))
                {
                    messageTypes.Add(interfaceClass);
                    AddInterfaces(messageTypes, interfaceClass.GetInterfaces());
                }
        }

        /// <summary>
        ///     Invokes the subscriber if the subscriptions is still active. Skipping subscriptions prevents race conditions
        ///     between <see cref="Unregister" /> and message delivery. Otherwise the message might be delivered after the
        ///     subscriber unregistered.This is particularly important for main thread delivery and registrations bound to the
        ///     live cycle of an Activity or Fragment.
        /// </summary>
        public object? InvokeSubscriber(PendingPost pendingPost)
        {
            var message = pendingPost.Message;
            var subscription = pendingPost.Subscription;
            PendingPost.ReleasePendingPost(pendingPost);
            if (subscription != null && subscription.Active) return InvokeSubscriber(subscription, message);
            return null;
        }

        public object? InvokeSubscriber(Subscription subscription, object? message)
        {
            try
            {
                return subscription.SubscriberMethod.Method.Invoke(subscription.Subscriber, new[] { message });
            }
            catch (TargetInvocationException e)
            {
                HandleSubscriberException(subscription, message, e.InnerException);
                return null;
            }
        }

        private void HandleSubscriberException(Subscription subscription, object message, Exception cause)
        {
            if (message is SubscriberExceptionMessage exceptionEvent)
            {
                if (_logSubscriberExceptions)
                {
                    // Don't send another SubscriberExceptionEvent to avoid infinite message recursion, just log
                    Logger.log(LogLevel.Critical,
                        $"SubscriberExceptionEvent subscriber {subscription.Subscriber.GetType().FullName} threw an exception",
                        cause);
                    Logger.log(LogLevel.Critical,
                        $"Initial message {exceptionEvent.OriginalMessage} caused exception in {exceptionEvent.OriginalSubscriber}, {exceptionEvent.Exception.Message}");
                }
            }
            else
            {
                if (_throwSubscriberException) throw new EventBusException("Invoking subscriber failed", cause);
                if (_logSubscriberExceptions)
                    Logger.log(LogLevel.Critical,
                        $"Could not dispatch message: {message.GetType().FullName} to subscribing class {subscription.Subscriber.GetType().FullName}",
                        cause);
                if (_sendSubscriberExceptionMessage)
                {
                    var exEvent = new SubscriberExceptionMessage(this, cause, message,
                        subscription.Subscriber);
                    Post(exEvent);
                }
            }
        }

        public ExecutorService GetExecutorService()
        {
            return _executorService;
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
            public object? Message { get; set; }
            public Subscription? Subscription { get; set; }
        }

        // Just an idea: we could provide a callback to post() to be notified, an alternative would be events, of course...
        //// public ////
        private interface PostCallback
        {
            void onPostCompleted(List<SubscriberExceptionMessage> exceptionEvents);
        }
    }
}