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
    ///     To receive messages, subscribers must register themselves to the bus using <see cref="Register" />.
    ///     Once registered, subscribers receive messages until <see cref="Unregister" /> is called.
    ///     Event handling methods must be annotated by <see cref="Subscribe" />, must be public, return nothing (void),
    ///     and have exactly one parameter (the message).
    /// </summary>
    public class EventBus
    {
        private readonly object _mutex = new object();
        private static readonly EventBusBuilder DefaultBuilder = new EventBusBuilder();
        private static readonly Dictionary<Type, List<Type>> MessageTypesCache = new Dictionary<Type, List<Type>>();

        internal static EventBus? _default;
        private readonly AsyncPoster? _asyncPoster;
        private readonly BackgroundPoster? _backgroundPoster;

        private readonly ThreadLocal<PostingThreadState> _currentPostingThreadState =
            new ThreadLocal<PostingThreadState>(
                () => new PostingThreadState());

        private readonly bool _messageInheritance;
        public IExecutor? Executor { get; }
        private readonly int _indexCount;
        private readonly bool _logNoSubscriberMessages;
        private readonly bool _logSubscriberExceptions;
        private readonly IPoster? _mainThreadPoster;
        private readonly IMainThreadSupport? _mainThreadSupport;
        private readonly bool _sendNoSubscriberMessage;
        private readonly bool _sendSubscriberExceptionMessage;
        private readonly Dictionary<Type, object> _stickyMessages;
        private readonly SubscriberMethodFinder? _subscriberMethodFinder;
        private readonly Dictionary<Type, ConcurrentList<Subscription>> _subscriptionsByEventType;
        private readonly bool _throwSubscriberException;
        private readonly Dictionary<object, List<Type>> _typesBySubscriber;

        /// <summary>
        ///     Creates a new EventBus instance; each instance is a separate scope in which messages are delivered.To use a
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
            _stickyMessages = new Dictionary<Type, object>();
            _mainThreadSupport = builder.MainThreadSupport;
            _mainThreadPoster = _mainThreadSupport?.CreatePoster(this);
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
            _messageInheritance = builder.EventInheritance;
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
            lock (MessageTypesCache)
            {
                MessageTypesCache.Clear();
            }
        }

        /// <summary>
        ///     Registers the given subscriber to receive messages. Subscribers must call <see cref="Unregister" /> once they
        ///     are no longer interested in receiving messages.
        ///     Subscribers have message handling methods that must be annotated by <see cref="SubscribeAttribute" />.
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
                if (_messageInheritance)
                {
                    // Existing sticky messages of all subclasses of messageType have to be considered.
                    // Note: Iterating over all messages may be inefficient with lots of sticky messages,
                    // thus data structure should be changed to allow a more efficient lookup
                    // (e.g. an additional map storing sub classes of super classes: Type -> List<Type>).

                    foreach (var entry in _stickyMessages)
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
                    var stickyEvent = _stickyMessages.ContainsKey(messageType) ? _stickyMessages[messageType] : default;
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
        ///     Only updates subscriptionsByEventType, not typesBySubscriber! Caller must update <see cref="_typesBySubscriber"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void UnsubscribeByEventType(object subscriber, Type messageType)
        {
            lock (_mutex)
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
            var subscribedTypes = _typesBySubscriber.ContainsKey(subscriber) ? _typesBySubscriber[subscriber] : null;
            if (subscribedTypes != null)
            {
                foreach (var messageType in subscribedTypes) UnsubscribeByEventType(subscriber, messageType);
                _typesBySubscriber.Remove(subscriber);
            }
            else
            {
                Logger.LogWarning($"Subscriber to unregister was not registered before: {subscriber.GetType().FullName}");
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
            lock (_stickyMessages)
            {
                _stickyMessages[message.GetType()] = message;
            }

            // Should be posted after it is putted, in case the subscriber wants to Remove immediately
            Post(message);
        }

        /// <summary>
        ///     Gets the most recent sticky message for the given type. See <see cref="PostSticky" />
        /// </summary>
        public object? GetStickyMessage(Type messageType)
        {
            lock (_stickyMessages)
            {
                return _stickyMessages.ContainsKey(messageType) ? _stickyMessages[messageType] : default;
            }
        }

        /// <summary>
        ///     Gets the most recent sticky message for the given type.
        ///     See <see cref="PostSticky" />
        /// </summary>
        public T GetStickyMessage<T>()
        {
            lock (_stickyMessages)
            {
                var messageType = typeof(T);
                return _stickyMessages.ContainsKey(messageType) ? (T)_stickyMessages[messageType] : default;
            }
        }

        /// <summary>
        ///     Remove and gets the recent sticky message for the given message type.
        ///     See <see cref="PostSticky" />
        /// </summary>
        public object? RemoveStickyMessage(Type messageType)
        {
            lock (_stickyMessages)
            {
                if (_stickyMessages.ContainsKey(messageType))
                {
                    var value = _stickyMessages[messageType];
                    _stickyMessages.Remove(messageType);
                    return value;
                }

                return null;
            }
        }

        /// <summary>
        ///     Remove and gets the recent sticky message for the given message type.
        ///     See <see cref="PostSticky" />
        /// </summary>
        public T RemoveStickyMessage<T>()
        {
            lock (_stickyMessages)
            {
                var messageType = typeof(T);
                if (_stickyMessages.ContainsKey(messageType))
                {
                    var result = _stickyMessages[messageType];
                    _stickyMessages.Remove(messageType);
                    return (T)result;
                }

                return default;
            }
        }

        /// <summary>
        ///     Removes the sticky message if it equals to the given message.
        ///     @return true if the messages matched and the sticky message was removed.
        /// </summary>
        public bool RemoveStickyMessage(object message)
        {
            lock (_mutex)
            {
                var messageType = message.GetType();
                var existingEvent = _stickyMessages.ContainsKey(messageType) ? _stickyMessages[messageType] : default;
                if (message.Equals(existingEvent))
                {
                    _stickyMessages.Remove(messageType);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Removes all sticky messages.
        /// </summary>
        public void RemoveAllStickyMessages()
        {
            lock (_mutex)
            {
                _stickyMessages.Clear();
            }
        }

        public bool HasSubscriberForEvent<T>()
        {
            return HasSubscriberForEvent(typeof(T));
        }

        public bool HasSubscriberForEvent(Type messageType)
        {
            var messageTypes = LookupAllMessageTypes(messageType);


            foreach (var type in messageTypes)
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

        private void PostSingleEvent(object message, PostingThreadState postingState)
        {
            var messageType = message.GetType();
            var subscriptionFound = false;
            if (_messageInheritance)
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
                    Logger.LogDebug($"No subscribers registered for message type {messageType.FullName}");
                if (_sendNoSubscriberMessage && messageType != typeof(NoSubscriberMessage) &&
                    messageType != typeof(SubscriberExceptionMessage))
                    Post(new NoSubscriberMessage(this, message));
            }
        }

        private bool PostSingleEventForEventType(object message, PostingThreadState postingState, Type messageType)
        {
            ConcurrentList<Subscription>? subscriptions;
            lock (_mutex)
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
        private static List<Type> LookupAllMessageTypes(Type messageType)
        {
            lock (MessageTypesCache)
            {
                var messageTypes = MessageTypesCache.ContainsKey(messageType)
                    ? MessageTypesCache[messageType]
                    : default;
                if (messageTypes == null)
                {
                    messageTypes = new List<Type>();
                    var type = messageType;
                    while (type != null)
                    {
                        messageTypes.Add(type);
                        AddInterfaces(messageTypes, type.GetInterfaces());
                        type = type.BaseType;
                    }

                    MessageTypesCache[messageType] = messageTypes;
                }

                return messageTypes;
            }
        }

        /// <summary>
        ///     Recurses through super interfaces.
        /// </summary>
        private static void AddInterfaces(List<Type> messageTypes, Type[] interfaces)
        {
            foreach (var interfaceType in interfaces)
                if (!messageTypes.Contains(interfaceType))
                {
                    messageTypes.Add(interfaceType);
                    AddInterfaces(messageTypes, interfaceType.GetInterfaces());
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

        public object? InvokeSubscriber(Subscription subscription, object message)
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
                    Logger.LogCritical(
                        $"SubscriberExceptionEvent subscriber {subscription.Subscriber.GetType().FullName} threw an exception",
                        cause);
                    Logger.LogCritical(
                        $"Initial message {exceptionEvent.OriginalMessage} caused exception in {exceptionEvent.OriginalSubscriber}, {exceptionEvent.Exception.Message}");
                }
            }
            else
            {
                if (_throwSubscriberException) throw new EventBusException("Invoking subscriber failed", cause);
                if (_logSubscriberExceptions)
                    Logger.LogCritical(
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

        public override string ToString()
        {
            return $"EventBus[indexCount={_indexCount}, eventInheritance={_messageInheritance}]";
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

        // Just an idea: we could provide a callback to post() to be notified, an alternative would be messages, of course...
        //// public ////
        private interface PostCallback
        {
            void onPostCompleted(List<SubscriberExceptionMessage> exceptionEvents);
        }
    }
}