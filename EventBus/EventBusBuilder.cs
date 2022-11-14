using System;
using System.Collections.Generic;
using Snork.EventBus.Meta;

namespace Snork.EventBus
{
    /// <summary>
    ///     Creates EventBus instances with custom parameters and also allows to install a custom default EventBus instance.
    ///     Create a new builder using <see cref="EventBus.Builder"/>.
    /// </summary>
    public class EventBusBuilder
    {
        private static readonly ExecutorService DEFAULT_EXECUTOR_SERVICE = new ExecutorService();
        public ExecutorService _executorService = DEFAULT_EXECUTOR_SERVICE;
        private Logger _logger;
        public List<Type> SkipMethodVerificationForClasses { get; } = new List<Type>();

        public List<ISubscriberInfoIndex> SubscriberInfoIndexes { get; } =
            new List<ISubscriberInfoIndex>();

        public bool EventInheritance { get; private set; } = true;
        public bool IgnoreGeneratedIndex { get; private set; }
        public bool LogNoSubscriberMessages { get; private set; } = true;

        public bool LogSubscriberExceptions { get; private set; } = true;
        public IMainThreadSupport? MainThreadSupport { get; private set; }
        public bool SendNoSubscriberEvent { get; private set; } = true;
        public bool SendSubscriberExceptionEvent { get; private set; } = true;
        public bool StrictMethodVerification { get; private set; }
        public bool ThrowSubscriberException { get; private set; }

        /// <summary>
        ///     Default: true
        /// </summary>
        public EventBusBuilder WithLogSubscriberExceptions(bool logSubscriberExceptions)
        {
            LogSubscriberExceptions = logSubscriberExceptions;
            return this;
        }

        /// <summary>
        ///     Default: true
        /// </summary>
        public EventBusBuilder WithLogNoSubscriberMessages(bool logNoSubscriberMessages)
        {
            LogNoSubscriberMessages = logNoSubscriberMessages;
            return this;
        }

        /// <summary>
        ///     Default: true
        /// </summary>
        public EventBusBuilder WithSendSubscriberExceptionEvent(bool sendSubscriberExceptionEvent)
        {
            SendSubscriberExceptionEvent = sendSubscriberExceptionEvent;
            return this;
        }

        /// <summary>
        ///     Default: true
        /// </summary>
        public EventBusBuilder WithSendNoSubscriberEvent(bool sendNoSubscriberEvent)
        {
            SendNoSubscriberEvent = sendNoSubscriberEvent;
            return this;
        }

        /// <summary>
        ///     Fails if an subscriber throws an exception (default: false).
        ///     <p />
        ///     Tip: Use this with BuildConfig.DEBUG to let the app crash in DEBUG mode (only). This way, you won't miss
        ///     exceptions during development.
        /// </summary>
        public EventBusBuilder WithThrowSubscriberException(bool throwSubscriberException)
        {
            ThrowSubscriberException = throwSubscriberException;
            return this;
        }

        /// <summary>
        ///     By default, EventBus considers the message class hierarchy (subscribers to super classes will be notified).
        ///     Switching this feature off will improve posting of events. For simple message classes extending object directly,
        ///     we measured a speed up of 20% for message posting. For more complex message hierarchies, the speed up should be
        ///     greater than 20%.
        ///     <p />
        ///     However, keep in mind that message posting usually consumes just a small proportion of CPU time inside an app,
        ///     unless it is posting at high rates, e.g. hundreds/thousands of events per second.
        /// </summary>
        public EventBusBuilder WithEventInheritance(bool eventInheritance)
        {
            EventInheritance = eventInheritance;
            return this;
        }


        /// <summary>
        ///     Provide a custom thread pool to EventBus used for async and background message delivery. This is an advanced
        ///     setting to that can break things: ensure the given ExecutorService won't get stuck to avoid undefined behavior.
        /// </summary>
        public EventBusBuilder ExecutorService(ExecutorService executorService)
        {
            _executorService = executorService;
            return this;
        }

        /// <summary>
        ///     MethodInfo name verification is done for methods starting with onEvent to avoid typos; using this method you can
        ///     exclude subscriber classes from this check. Also disables checks for method modifiers (public, not static nor
        ///     abstract).
        /// </summary>
        public EventBusBuilder WithSkipMethodVerificationFor(Type type)
        {
            SkipMethodVerificationForClasses.Add(type);
            return this;
        }

        /// <summary>
        ///     Forces the use of reflection even if there's a generated index (default: false).
        /// </summary>
        public EventBusBuilder WithIgnoreGeneratedIndex(bool ignoreGeneratedIndex)
        {
            IgnoreGeneratedIndex = ignoreGeneratedIndex;
            return this;
        }

        /// <summary>
        ///     Enables strict method verification (default: false).
        /// </summary>
        public EventBusBuilder WithStrictMethodVerification(bool strictMethodVerification)
        {
            StrictMethodVerification = strictMethodVerification;
            return this;
        }

        /// <summary>
        ///     Adds an index generated by EventBus' annotation preprocessor.
        /// </summary>
        public EventBusBuilder AddIndex(ISubscriberInfoIndex index)
        {
            SubscriberInfoIndexes.Add(index);
            return this;
        }

        /// <summary>
        ///     Set a specific log handler for all EventBus logging.
        ///     <p />
        ///     By default, all logging is via {@code android.util.Log} on Android or System.out on JVM.
        /// </summary>
        public EventBusBuilder WithLogger(Logger logger)
        {
            _logger = logger;
            return this;
        }

        public Logger GetLogger()
        {
            if (_logger != null)
                return _logger;
            return Logger.Default.get();
        }

        public IMainThreadSupport? GetMainThreadSupport()
        {
            if (MainThreadSupport != null)
                return MainThreadSupport;

            return null;
        }

        /// <summary>
        ///     Installs the default EventBus returned by <see cref="EventBus.Default"/>  using this builders' values. Must be
        ///     done only once before the first usage of the default EventBus.
        ///     @throws EventBusException if there's already a default EventBus instance in place
        /// </summary>
        public EventBus InstallDefaultEventBus()
        {
            lock (typeof(EventBus))
            {
                if (EventBus.Default != null)
                    throw new EventBusException("Default instance already exists." +
                                                " It may be only set once before it's used the first time to ensure consistent behavior.");

                EventBus._default = Build();
                return EventBus.Default;
            }
        }

        /// <summary>
        ///     Builds an EventBus based on the current configuration.
        /// </summary>
        public EventBus Build()
        {
            return new EventBus(this);
        }
    }
}