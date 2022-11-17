using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using Snork.EventBus.Interfaces;

namespace Snork.EventBus.Util
{
    /// <summary>
    ///     Executes an <see cref="IRunnable" /> using a thread pool. Thrown exceptions are propagated by posting failure
    ///     events.
    ///     By default, uses <see cref="ExceptionFailureEvent" />.
    ///     Set a custom event type using <see cref="BuilderImpl.WithFailureEventType" />.
    ///     The failure event class must have a constructor with one parameter of type <see cref="Exception" />.
    ///     E.g. Add a rule like
    ///     <pre>
    ///         -keepclassmembers class com.example.CustomThrowableFailureEvent {
    ///         &lt;init&gt;(Exception);
    ///         }
    ///     </pre>
    /// </summary>
    public class AsyncExecutor
    {
        private static readonly string ConstructorErrorEvent =
            $"Failure event class must have a constructor with one parameter of type {nameof(Exception)}";

        private readonly EventBus _eventBus;
        private readonly ConstructorInfo? _failureEventConstructor;
        private readonly object? _scope;

        private AsyncExecutor(EventBus eventBus, Type failureEventType, object? scope)
        {
            _eventBus = eventBus;
            _scope = scope;

            try
            {
                _failureEventConstructor = failureEventType.GetConstructor(new[] { typeof(Exception) });
                if (_failureEventConstructor == null)
                    throw new InvalidOperationException(
                        ConstructorErrorEvent);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    ConstructorErrorEvent, e);
            }
        }

        public static BuilderImpl Builder()
        {
            return new BuilderImpl();
        }

        public static AsyncExecutor Create()
        {
            return new BuilderImpl().Build();
        }

        /// <summary>
        ///     Posts an failure event if the given <see cref="IRunnable" /> throws an Exception.
        /// </summary>
        public void Execute(IRunnable runnable)
        {
            var task = new Task(() =>
            {
                try
                {
                    runnable.Run();
                }
                catch (Exception e)
                {
                    object @event;
                    try
                    {
                        @event = _failureEventConstructor.Invoke(new object[] { e });
                    }
                    catch (Exception innerException)
                    {
                        _eventBus.Logger.LogCritical(e, $"Original exception: {e.Message}");
                        throw new InvalidOperationException("Could not create failure event", innerException);
                    }

                    if (@event is IExecutionScopeContainer executionScope) executionScope.ExecutionScope = _scope;
                    _eventBus.Post(@event);
                }
            });

            task.WaitAndUnwrapException();
        }

        public class BuilderImpl
        {
            private EventBus? _eventBus;

            private Type? _failureEventType;

            internal BuilderImpl()
            {
            }

            public BuilderImpl WithFailureEventType(Type failureEventType)
            {
                _failureEventType = failureEventType;
                return this;
            }

            public BuilderImpl WithEventBus(EventBus eventBus)
            {
                _eventBus = eventBus;
                return this;
            }

            public AsyncExecutor Build()
            {
                return BuildForScope(null);
            }

            public AsyncExecutor BuildForScope(object? executionContext)
            {
                _eventBus ??= EventBus.Default;

                _failureEventType ??= typeof(ExceptionFailureEvent);

                return new AsyncExecutor(_eventBus, _failureEventType, executionContext);
            }
        }
    }
}