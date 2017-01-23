using System;
using System.Runtime.CompilerServices;

namespace Pelco.VxInt.Logging
{
    public interface INLogger
    {
        #region Configuration

        /// <summary>
        /// Base logfile name (without extension.)
        /// </summary>
        /// <remarks>Logfile will be written to Ops Center logging data with a .log extension.</remarks>
        string LogFileName { get; set; }

        /// <summary>
        /// Minimum message level to write (default="info")
        /// </summary>
        /// <remarks>Valid values: trace, debug, info, warn, error.</remarks>
        string LoggedLevel { get; set; }

        /// <summary>
        /// Maximum number of archived logfiles to keep (default=7.)
        /// </summary>
        int MaxArchiveFiles { get; set; }
        
        /// <summary>
        /// Maximum allowable size of the logfile before archiving (in kb.)
        /// </summary>
        /// <remarks>Default value is 10MB.</remarks>
        int MaxLogFileSize { get; set; }
        
        #endregion

        #region Logging calls

        void Trace(string message,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);
        void TraceException(Exception exception,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);
        void TraceException(string message, Exception exception,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);

        void Debug(string message,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);
        void DebugException(Exception exception,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);
        void DebugException(string message, Exception exception,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);

        void Info(string message,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);
        void InfoException(Exception exception,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);
        void InfoException(string message, Exception exception,
            [CallerFilePath] string callerPath = "", string callerMember = "", [CallerLineNumber] int callerLine = 0);

        void Warn(string message,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);
        void WarnException(Exception exception,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);
        void WarnException(string message, Exception exception,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);

        void Error(string message, Exception exception = null,
            [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0);

        #endregion
    }
}
