using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Snork.EventBus.Util
{
    /// <summary>
    ///     Maps exceptions to texts for error dialogs. Use Config to configure the mapping.
    ///     @author Markus
    /// </summary>
    public class ExceptionToResourceMapping
    {
        public Dictionary<Type, int> ExceptionToMessageIdMap { get; } = new Dictionary<Type, int>();

        /// <summary>
        ///     Looks at the exception and its causes trying to find an ID.
        /// </summary>
        public int? MapException(Exception exception)
        {
            var exceptionToCheck = exception;
            var depthToGo = 20;

            while (true)
            {
                var resId = MapExceptionFlat(exceptionToCheck);
                if (resId != null) return resId;

                exceptionToCheck = exceptionToCheck.InnerException;
                depthToGo--;
                if (depthToGo <= 0 || exceptionToCheck == exception || exceptionToCheck == null)
                {
                    //var logger = Logger.Default.get(); // No EventBus instance here
                    //logger.LogDebug( "No specific message resource ID found for " + exception);
                    // return config.defaultErrorMsgId;
                    return null;
                }
            }
        }

        /// <summary>
        ///     Mapping without checking the cause (done in mapThrowable).
        /// </summary>
        protected int? MapExceptionFlat(Exception exception)
        {
            var exceptionType = exception.GetType();
            int? resId = null;
            if (ExceptionToMessageIdMap.ContainsKey(exceptionType)) return ExceptionToMessageIdMap[exceptionType];

            Type? closestType = null;
            foreach (var map in ExceptionToMessageIdMap)
            {
                var candidate = map.Key;
                if (candidate.IsAssignableFrom(exceptionType))
                    if (closestType == null || closestType.IsAssignableFrom(candidate))
                    {
                        closestType = candidate;
                        resId = map.Value;
                    }
            }

            return resId;
        }

        public ExceptionToResourceMapping AddMapping(Type type, int msgId)
        {
            ExceptionToMessageIdMap[type] = msgId;
            return this;
        }
    }
}