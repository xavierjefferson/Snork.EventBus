using Microsoft.Extensions.Logging;

namespace Snork.EventBus
{
    public interface Logger
    {
        class SystemOutLogger : Logger
        {
            private ILogger logger1;
        }

        class Default
        {
            public static Logger get()
            {
                return new SystemOutLogger();
            }
        }
    }
}