using System;
using System.Runtime.CompilerServices;
using NLog;

namespace Pelco.VxInt.Logging
{
    /// <summary>
    /// adapted from <a href="http://exceptionalcode.wordpress.com/2012/06/09/extended-logging-with-caller-info-attributes/"></a>
    /// </summary>
    public static class NLogExtensions
    {
        public static void TraceMemberEntry(this Logger logger,
                                            [CallerFilePath] string filePath = "",
                                            [CallerMemberName] string memberName = "",
                                            [CallerLineNumber] int lineNumber = 0)
        {
            LogMemberEntry(logger, LogLevel.Trace, filePath, memberName, lineNumber);
        }

        public static void TraceMemberExit(this Logger logger,
                                           [CallerFilePath] string filePath = "",
                                           [CallerMemberName] string memberName = "",
                                           [CallerLineNumber] int lineNumber = 0)
        {
            LogMemberExit(logger, LogLevel.Trace, filePath, memberName, lineNumber);
        }

        public static void DebugMemberEntry(this Logger logger,
                                            [CallerFilePath] string filePath = "",
                                            [CallerMemberName] string memberName = "",
                                            [CallerLineNumber] int lineNumber = 0)
        {
            LogMemberEntry(logger, LogLevel.Debug, filePath, memberName, lineNumber);
        }

        public static void DebugMemberExit(this Logger logger,
                                           [CallerFilePath] string filePath = "",
                                           [CallerMemberName] string memberName = "",
                                           [CallerLineNumber] int lineNumber = 0)
        {
            LogMemberExit(logger, LogLevel.Debug, filePath, memberName, lineNumber);
        }

        public static void LogMemberEntry(this Logger logger,
                                          LogLevel logLevel,
                                          [CallerFilePath] string filePath = "",
                                          [CallerMemberName] string memberName = "",
                                          [CallerLineNumber] int lineNumber = 0)
        {
            const string MsgFormat = "Entering member {1} at line {2}";

            InternalLog(logger, logLevel, MsgFormat, filePath, memberName, lineNumber);
        }

        public static void LogMemberExit(this Logger logger,
                                         LogLevel logLevel,
                                         [CallerFilePath] string filePath = "",
                                         [CallerMemberName] string memberName = "",
                                         [CallerLineNumber] int lineNumber = 0)
        {
            const string MsgFormat = "Exiting member {1} at line {2}";

            InternalLog(logger, logLevel, MsgFormat, filePath, memberName, lineNumber);
        }

        private static void InternalLog(Logger logger,
                                        LogLevel logLevel,
                                        String format,
                                        String filePath,
                                        String memberName,
                                        int lineNumber)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (logLevel == null)
            {
                throw new ArgumentNullException("logLevel");
            }

            logger.Log(logLevel, format, filePath, memberName, lineNumber);
        }
    }
}
