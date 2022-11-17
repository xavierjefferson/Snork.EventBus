using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    public abstract class LoggingTestBase
    {
        protected LoggingTestBase(ITestOutputHelper output)
        {
            Output = output;
            Logger = new XUnitLogger<TestBase>(output);
            EventBus.ClearCaches();
            Setup();
        }

        public ITestOutputHelper Output { get; }
        protected ILogger Logger { get; }
        public EventBus EventBus { get; set; }
        protected abstract void Setup();
    }
}