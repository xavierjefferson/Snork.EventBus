using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;

namespace Snork.EventBus.Util
{
    /// <summary>
    ///     Executes an <see cref="RunnableEx" /> using a thread pool. Thrown exceptions are propagated by posting failure
    ///     messages.
    ///     By default, uses <see cref="ExceptionFailureMessage" />.
    ///     Set a custom message type using <see cref="BuilderImpl.WithFailureEventType" />.
    ///     The failure message class must have a constructor with one parameter of type <see cref="Exception" />.
    ///     E.g. Add a rule like
    ///     <pre>
    ///         -keepclassmembers class com.example.CustomThrowableFailureEvent {
    ///         &lt;init&gt;(Exception);
    ///         }
    ///     </pre>
    /// </summary>
    public class AsyncExecutor
    {
        private static readonly string ConstructorErrorMessage =
            $"Failure message class must have a constructor with one parameter of type {nameof(Exception)}";

        private readonly EventBus _eventBus;
        private readonly ConstructorInfo? _failureEventConstructor;
        private readonly object? _scope;

        //private readonly Executor threadPool;

        private AsyncExecutor(EventBus eventBus, Type failureEventType, object? scope)
        {
            _eventBus = eventBus;
            _scope = scope;

            try
            {
                _failureEventConstructor = failureEventType.GetConstructor(new[] { typeof(Exception) });
                if (_failureEventConstructor == null)
                    throw new InvalidOperationException(
                        ConstructorErrorMessage);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    ConstructorErrorMessage, e);
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
        ///     Posts an failure message if the given <see cref="RunnableEx" /> throws an Exception.
        /// </summary>
        public void Execute(RunnableEx runnable)
        {
            var task = new Task(() =>
            {
                try
                {
                    runnable.Run();
                }
                catch (Exception e)
                {
                    object message;
                    try
                    {
                        message = _failureEventConstructor.Invoke(new object[] { e });
                    }
                    catch (Exception innerException)
                    {
                        _eventBus.Logger.LogCritical(e, $"Original exception: {e.Message}");
                        throw new InvalidOperationException("Could not create failure message", innerException);
                    }

                    if (message is IExecutionScopeContainer executionScope) executionScope.ExecutionScope = _scope;
                    _eventBus.Post(message);
                }
            });

            task.WaitAndUnwrapException();
        }

        public class BuilderImpl
        {
            private EventBus? _eventBus;

            private Type? _failureEventType;
            // private Executor _threadPool;

            internal BuilderImpl()
            {
            }

            //public Builder threadPool(Executor threadPool)
            //{
            //    _threadPool = threadPool;
            //    return this;
            //}

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

                // if (_threadPool == null) _threadPool = Executors.newCachedThreadPool();

                _failureEventType ??= typeof(ExceptionFailureMessage);

                return new AsyncExecutor(_eventBus, _failureEventType, executionContext);
            }
        }

        /// <summary>
        ///     Like <see cref="Runnable" />, but the run method may throw an exception.
        /// </summary>
        public interface RunnableEx
        {
            void Run();
        }
    }
}