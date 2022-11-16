using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Snork.EventBus.Tests
{
    /**
 * @author Markus Junginger, greenrobot
 */
    public abstract class LoggingTestBase
    {
        public ITestOutputHelper Output { get; }
        protected ILogger Logger { get; }
        protected LoggingTestBase(ITestOutputHelper output)
        {
            Output = output;
            Logger = new XUnitLogger<TestBase>(output);
        }
    }
}