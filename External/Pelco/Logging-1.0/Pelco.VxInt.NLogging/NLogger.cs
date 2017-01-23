using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Pelco.VxInt.Logging
{
    /// <summary>
    /// Log wrapper. Currently uses NLog as a back end.
    /// Caller info slickness mentioned by <a href="http://stackoverflow.com/a/15446149/222458"></a>,
    /// and expanded on by <a href="http://exceptionalcode.wordpress.com/2012/06/09/extended-logging-with-caller-info-attributes/"></a>
    /// </summary>
    public class NLogger : INLogger
    {
        #region Fields

        private const int MAX_LOG_FILE_SIZE_MB = 1024 * 10000; // 10 MB Max size

        private static NLogger _logger;
        
        private readonly String _nLogLayout =
            @"${time} |${processid}.${pad:padding=2:padCharacter=0:inner=${event-context:item=threadId}}| ${pad:padding=-5:inner=${level:uppercase=true}} [${event-context:item=callermember} " +
            @"(${event-context:item=callerpath}:${event-context:item=callerline})]: ${message} " +
            @"${onexception:${exception:format=shorttype,message,stacktrace:maxInnerExceptionLevel=5:innerFormat=shorttype,message,method,stacktrace}}";

        private readonly BlockingCollection<Task> _pendingLogWrites;
        private readonly CancellationTokenSource _logWriteCancellation;

        private LogLevel _logLevel;

        #endregion

        #region Properties

        public string LogFileName { get; set; }

        public string LoggedLevel
        {
            get { return _logLevel.Name; }
            set
            {
                _logLevel = LogLevel.FromString(value);
            }
        }

        public static INLogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    throw new InvalidOperationException("Logger has not been created. Call Nlogger.Create() first.");
                }

                return _logger;
            }
        }
        
        public int MaxArchiveFiles { get; set; }
        
        public int MaxLogFileSize  { get; set; }

        #endregion

        /// <summary>
        /// static constructor.
        /// </summary>
        private NLogger(string logFileName = null, string logLevelName = null)
        {
#if DEBUG
            _logLevel = LogLevel.Debug;
#else
            _logLevel = LogLevel.Info;
#endif

            LogFileName = logFileName;
            if (!String.IsNullOrWhiteSpace(logLevelName)) _logLevel = LogLevel.FromString(logLevelName);
            MaxLogFileSize = 10000;
            MaxArchiveFiles = 7;

            _pendingLogWrites = new BlockingCollection<Task>(new ConcurrentQueue<Task>());
            _logWriteCancellation = new CancellationTokenSource();
        }

        #region Logging control

        public static INLogger Create(string logFileName = null, string logLevelName = null, int maxArchiveFiles = 0, int maxLogFileSize = 0)
        {
            if (_logger != null)
            {
                throw new InvalidOperationException("Logger has already been created!");
            }

            _logger = new NLogger(logFileName, logLevelName);
            if (maxArchiveFiles > 0) _logger.MaxArchiveFiles = maxArchiveFiles;
            if (maxLogFileSize > 0) _logger.MaxLogFileSize = maxLogFileSize;

            _logger.ConfigureNLog();

            // Start log entry collection thread
            Task.Factory.StartNew(() =>
            {
                foreach (Task logTask in _logger._pendingLogWrites.GetConsumingEnumerable())
                {
                    logTask.RunSynchronously();
                    if (_logger._logWriteCancellation.IsCancellationRequested && _logger._pendingLogWrites.Count == 0)
                    {
                        break;
                    }
                }
            }, TaskCreationOptions.LongRunning);

            return _logger;
        }
        
        /// <summary>
        /// Stop logging tasks.
        /// </summary>
        public static void Stop()
        {
            if (_logger != null)
            {
                _logger._pendingLogWrites.CompleteAdding();
                _logger._logWriteCancellation.Cancel();
            }
        }

        #endregion

        /// <summary>
        /// Configure NLog.
        /// </summary>
        private void ConfigureNLog()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            string appName = LogFileName;
            if (String.IsNullOrWhiteSpace(appName))
            {
                string fullProcessPath = Process.GetCurrentProcess().MainModule.FileName;
                appName = Path.GetFileNameWithoutExtension(fullProcessPath)
                    .Replace(".vshost", "");
            }

            // Keep 7 logs, at a maximum of 100 MB each.
            //TODO: keep path sync'd with CommonDirectories.CommonAppDataFolder
            var fileTarget = new FileTarget()
            {
                Layout = _nLogLayout,
                Encoding = Encoding.UTF8,
                FileName = @"${specialfolder:folder=CommonApplicationData}/Pelco/OpsCenter/Logs/" + appName + ".log",
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                ArchiveAboveSize = MaxLogFileSize * 1024,
                MaxArchiveFiles = MaxArchiveFiles,
                // If only a single process is writing to the log file, this increases performance.
                // see https://github.com/NLog/NLog/wiki/File-target.
                ConcurrentWrites = true
            };

            //log messages are queued and batched on a background thread
            var asyncFileTarget = new AsyncTargetWrapper(fileTarget);
            config.AddTarget("Log", asyncFileTarget);
            config.LoggingRules.Add(new LoggingRule("*", _logLevel, asyncFileTarget));

            LogManager.Configuration = config;
        }

        /// <summary>
        /// Base call into NLog. 
        /// </summary>
        /// <param name="level">log level</param>
        /// <param name="message">message</param>
        /// <param name="exception">optional exception</param>
        /// <param name="callerPath">optional path of calling file</param>
        /// <param name="callerMember">optional name of calling object</param>
        /// <param name="callerLine">optional line number of call</param>
        private void NLog(LogLevel level,
                                 String message,
                                 Exception exception = null,
                                 String callerPath = "",
                                 String callerMember = "",
                                 int callerLine = 0)
        {
            // Build up the log message right away so it has an accurate timestamp.
            LogEventInfo logEvent = new LogEventInfo(level, callerPath, message);

            int threadId = Thread.CurrentThread.ManagedThreadId;
            if (_pendingLogWrites.IsAddingCompleted)
            {
                Console.WriteLine("Log writing is completed, message not written.");
                return;
            }

            // Add it to a queue of messages that's processed and submitted on a dedicated thread.
            _pendingLogWrites.Add(new Task(() =>
            {
                try
                {
                    //best practice is use separate loggers for each class.
                    // Getting the logger for a particular class can be expensive.
                    Logger logger = LogManager.GetLogger(callerPath);
                    if (logger == null || !logger.IsEnabled(level))
                    {
                        return;
                    }

                    //shortening file path for readability in log
                    String shortPath = "";
                    if (callerPath != null && callerPath != "")
                    {
                        shortPath = Path.GetFileName(callerPath);
                    }

                    //attach caller info to message
                    logEvent.Exception = exception;
                    logEvent.Properties.Add("callerpath", shortPath);
                    logEvent.Properties.Add("callermember", callerMember);
                    logEvent.Properties.Add("callerline", callerLine);
                    logEvent.Properties.Add("threadId", threadId);
                    logger.Log(logEvent);
                }
                catch (Exception e)
                {
                    //Sometimes I see the app lock up in GetLogger while debugging? -CS
                    //don't want to infinite loop log exceptions
                    Console.WriteLine("NLog log event failed: {0}", e.Message);
                }
            }));
        }

        /// <summary>
        /// Ensure special exception types are logged with full detail. 
        /// </summary>
        /// <param name="level">log level</param>
        /// <param name="message">message</param>
        /// <param name="exception">optional exception</param>
        /// <param name="callerPath">optional path of calling file</param>
        /// <param name="callerMember">optional name of calling object</param>
        /// <param name="callerLine">optional line number of call</param>
        private void LogException(
            LogLevel level,
            string message,
            Exception exception,
            string callerPath,
            string callerMember,
            int callerLine)
        {
            if (exception is AggregateException)
            {
                AggregateException aggregateException = exception as AggregateException;
                foreach (Exception ex in aggregateException.InnerExceptions)
                {
                    LogException(level, message, ex, callerPath, callerMember, callerLine);
                }
            }
            else
            {
                NLog(level, message, exception, callerPath, callerMember, callerLine);
            }
        }

        /// <summary>
        /// Write a trace message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void Trace(String message,
                                 [CallerFilePath] String callerPath = "",
                                 [CallerMemberName] String callerMember = "",
                                 [CallerLineNumber] int callerLine = 0)
        {
            NLog(LogLevel.Trace, message, null, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a trace message about an exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void TraceException(String message,
                                          Exception exception,
                                          [CallerFilePath] String callerPath = "",
                                          [CallerMemberName] String callerMember = "",
                                          [CallerLineNumber] int callerLine = 0)
        {
            LogException(LogLevel.Trace, message, exception, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write trace information about an exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void TraceException(Exception exception,
                                          [CallerFilePath] String callerPath = "",
                                          [CallerMemberName] String callerMember = "",
                                          [CallerLineNumber] int callerLine = 0)
        {
            TraceException("", exception, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a debug message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void Debug(String message,
                                 [CallerFilePath] String callerPath = "",
                                 [CallerMemberName] String callerMember = "",
                                 [CallerLineNumber] int callerLine = 0)
        {
            NLog(LogLevel.Debug, message, null, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a debug message about an exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void DebugException(String message,
                                          Exception exception,
                                          [CallerFilePath] String callerPath = "",
                                          [CallerMemberName] String callerMember = "",
                                          [CallerLineNumber] int callerLine = 0)
        {
            LogException(LogLevel.Debug, message, exception, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write debug information about an exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void DebugException(Exception exception,
                                          [CallerFilePath] String callerPath = "",
                                          [CallerMemberName] String callerMember = "",
                                          [CallerLineNumber] int callerLine = 0)
        {
            DebugException("", exception, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a formatted debug message with one argument.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        public void DebugFormat1(String format,
                                        object arg0,
                                        [CallerFilePath] String callerPath = "",
                                        [CallerMemberName] String callerMember = "",
                                        [CallerLineNumber] int callerLine = 0)
        {
            Debug(String.Format(format, arg0), callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a formatted debug message with two arguments.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public void DebugFormat2(String format,
                                        object arg0,
                                        object arg1,
                                        [CallerFilePath] String callerPath = "",
                                        [CallerMemberName] String callerMember = "",
                                        [CallerLineNumber] int callerLine = 0)
        {
            Debug(String.Format(format, arg0, arg1), callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a formatted debug message with 3 arguments.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void DebugFormat3(String format,
                                        object arg0,
                                        object arg1,
                                        object arg2,
                                        [CallerFilePath] String callerPath = "",
                                        [CallerMemberName] String callerMember = "",
                                        [CallerLineNumber] int callerLine = 0)
        {
            Debug(String.Format(format, arg0, arg1, arg2), callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write an informational message.
        /// </summary>
        /// <param name="message"></param>
        public void Info(String message,
                                [CallerFilePath] String callerPath = "",
                                [CallerMemberName] String callerMember = "",
                                [CallerLineNumber] int callerLine = 0)
        {
            NLog(LogLevel.Info, message, null, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write an informational message about an exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void InfoException(String message,
                                         Exception exception,
                                         [CallerFilePath] String callerPath = "",
                                         [CallerMemberName] String callerMember = "",
                                         [CallerLineNumber] int callerLine = 0)
        {
            LogException(LogLevel.Info, message, exception, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write information about an exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void InfoException(Exception exception,
                                         [CallerFilePath] String callerPath = "",
                                         [CallerMemberName] String callerMember = "",
                                         [CallerLineNumber] int callerLine = 0)
        {
            InfoException("", exception, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a formatted informational message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        public void InfoFormat1(String format,
                                       object arg0,
                                       [CallerFilePath] String callerPath = "",
                                       [CallerMemberName] String callerMember = "",
                                       [CallerLineNumber] int callerLine = 0)
        {
            Info(String.Format(format, arg0), callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a formatted informational message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public void InfoFormat2(String format,
                                       object arg0,
                                       object arg1,
                                       [CallerFilePath] String callerPath = "",
                                       [CallerMemberName] String callerMember = "",
                                       [CallerLineNumber] int callerLine = 0)
        {
            Info(String.Format(format, arg0, arg1), callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a formatted informational message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void InfoFormat3(String format,
                                       object arg0,
                                       object arg1,
                                       object arg2,
                                       [CallerFilePath] String callerPath = "",
                                       [CallerMemberName] String callerMember = "",
                                       [CallerLineNumber] int callerLine = 0)
        {
            Info(String.Format(format, arg0, arg1, arg2), callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a warning message.
        /// </summary>
        /// <param name="message"></param>
        public void Warn(String message,
                                [CallerFilePath] String callerPath = "",
                                [CallerMemberName] String callerMember = "",
                                [CallerLineNumber] int callerLine = 0)
        {
            NLog(LogLevel.Warn, message, null, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a warning message about an exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void WarnException(String message,
                                         Exception exception,
                                         [CallerFilePath] String callerPath = "",
                                         [CallerMemberName] String callerMember = "",
                                         [CallerLineNumber] int callerLine = 0)
        {
            LogException(LogLevel.Warn, message, exception, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write warning information about an exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void WarnException(Exception exception,
                                         [CallerFilePath] String callerPath = "",
                                         [CallerMemberName] String callerMember = "",
                                         [CallerLineNumber] int callerLine = 0)
        {
            WarnException("", exception, callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a formatted warning message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        public void WarnFormat1(String format,
                                       object arg0,
                                       [CallerFilePath] String callerPath = "",
                                       [CallerMemberName] String callerMember = "",
                                       [CallerLineNumber] int callerLine = 0)
        {
            Warn(String.Format(format, arg0), callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a formatted warning message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public void WarnFormat2(String format,
                                       object arg0,
                                       object arg1,
                                       [CallerFilePath] String callerPath = "",
                                       [CallerMemberName] String callerMember = "",
                                       [CallerLineNumber] int callerLine = 0)
        {
            Warn(String.Format(format, arg0, arg1), callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Write a formatted warning message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void WarnFormat3(String format,
                                       object arg0,
                                       object arg1,
                                       object arg2,
                                       [CallerFilePath] String callerPath = "",
                                       [CallerMemberName] String callerMember = "",
                                       [CallerLineNumber] int callerLine = 0)
        {
            Warn(String.Format(format, arg0, arg1, arg2), callerPath, callerMember, callerLine);
        }

        /// <summary>
        /// Writes the diagnostic message at the Error level.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="callerPath"></param>
        /// <param name="callerMember"></param>
        /// <param name="callerLine"></param>
        public void Error(string message, Exception exception = null,
            [CallerFilePathAttribute] string callerPath = "",
            [CallerMemberName] string callerMember = "",
            [CallerLineNumber] int callerLine = 0)
        {
            NLog(LogLevel.Error, message, exception, callerPath, callerMember, callerLine);
        }

#if false
        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    Debug(message, "--", "Unknown", 999);
                    break;
                case Category.Info:
                    Info(message, "--", "Unknown", 999);
                    break;
                case Category.Warn:
                    Warn(message, "--", "Unknown", 999);
                    break;
                case Category.Exception:
                    Error(message, null, "--", "Unknown", 999);
                    break;
            }
        }
#endif
    }
}
