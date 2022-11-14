using System;
using Microsoft.Extensions.Logging;

namespace Snork.EventBus
{
    public interface Logger
    {
        void log(LogLevel level, string msg);

        void log(LogLevel level, string msg, Exception th);

        class JavaLogger : Logger
        {
            //protected readonly java.util.logging.Logger logger;

            public JavaLogger(string tag)
            {
                //logger = java.util.logging.Logger.getLogger(tag);
            }


            public void log(LogLevel level, string msg)
            {
                // TODO Replace logged method with caller method
                //logger.log(level, msg);
            }


            public void log(LogLevel level, string msg, Exception th)
            {
                // TODO Replace logged method with caller method
                //logger.log(level, msg, th);
            }
        }

        class SystemOutLogger : Logger
        {
            private ILogger logger1;

            public void log(LogLevel level, string msg)
            {
                //System.out.println("[" + level + "] " + msg);
            }


            public void log(LogLevel level, string msg, Exception th)
            {
                //System.out.println("[" + level + "] " + msg);
                //th.printStackTrace(System.out);
            }
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