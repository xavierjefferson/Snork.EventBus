using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Snork.EventBus.Interfaces;
using Snork.EventBus.Meta;

namespace Snork.EventBus
{
    /// <summary>
    ///     Creates EventBus instances with custom parameters and also allows to install a custom default <see cref="EventBus"/> instance.
    ///     Create a new builder using <see cref="EventBus.Builder"/>.
    /// </summary>
    public class EventBusBuilder
    {
        private static readonly IExecutor DefaultExecutor = new ExecutorService();
        public IExecutor Executor { get; set; } = DefaultExecutor;
        private ILogger _logger = NullLogger.Instance;
        public List<Type> SkipMethodVerificationForTypes { get; } = new List<Type>();

        public List<ISubscriberInfoIndex> SubscriberInfoIndexes { get; } =
            new List<ISubscriberInfoIndex>();

        public bool EventInheritance { get; private set; } = true;
        public bool IgnoreGeneratedIndex { get; private set; }
        public bool LogNoSubscriberEvents { get; private set; } = true;

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
        public EventBusBuilder WithLogNoSubscriberEvents(bool logNoSubscriberEvents)
        {
            LogNoSubscriberEvents = logNoSubscriberEvents;
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
        /// </summary>
        public EventBusBuilder WithThrowSubscriberException(bool throwSubscriberException)
        {
            ThrowSubscriberException = throwSubscriberException;
            return this;
        }

        /// <summary>
        ///     By default, EventBus considers the event class hierarchy (subscribers to super classes will be notified).
        ///     Switching this feature off will improve posting of events. For simple event classes extending object directly,
        ///     we measured a speed up of 20% for event posting. For more complex event hierarchies, the speed up should be
        ///     greater than 20%.
        ///     However, keep in mind that event posting usually consumes just a small proportion of CPU time inside an app,
        ///     unless it is posting at high rates, e.g. hundreds/thousands of events per second.
        /// </summary>
        public EventBusBuilder WithEventInheritance(bool eventInheritance)
        {
            EventInheritance = eventInheritance;
            return this;
        }


        /// <summary>
        ///     Provide a custom thread pool to <see cref="EventBus"/> used for async and background event delivery. This is an advanced
        ///     setting to that can break things: ensure the given <see cref="ExecutorService"/> won't get stuck to avoid undefined behavior.
        /// </summary>
        public EventBusBuilder WithExecutor(IExecutor executor)
        {
            Executor = executor;
            return this;
        }

        /// <summary>
        ///     <see cref="System.Reflection.MethodInfo"/> name verification is done for methods starting with OnEvent to avoid typos; using this method you can
        ///     exclude subscriber classes from this check. Also disables checks for method modifiers (public, not static nor
        ///     abstract).
        /// </summary>
        public EventBusBuilder WithSkipMethodVerificationFor(Type type)
        {
            SkipMethodVerificationForTypes.Add(type);
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
        /// </summary>
        public EventBusBuilder WithLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public ILogger Logger => _logger ?? NullLogger.Instance;

        public IMainThreadSupport? GetMainThreadSupport()
        {
            return MainThreadSupport ?? null;
        }

        /// <summary>
        ///     Installs the default EventBus returned by <see cref="EventBus.Default"/>  using this builders' values. Must be
        ///     done only once before the first usage of the default <see cref="EventBus"/>.
        ///     @throws EventBusException if there's already a default <see cref="EventBus"/> instance in place
        /// </summary>
        public EventBus InstallDefaultEventBus()
        {
            lock (typeof(EventBus))
            {
                if (EventBus.Default != null)
                    throw new EventBusException($"Default {nameof(EventBus)} instance already exists.");

                EventBus._default = Build();
                return EventBus.Default;
            }
        }

        /// <summary>
        ///     Builds an <see cref="EventBus"/> instance based on the current configuration.
        /// </summary>
        public EventBus Build()
        {
            return new EventBus(this);
        }
    }
}