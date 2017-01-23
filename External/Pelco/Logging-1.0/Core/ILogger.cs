﻿using Prism.Logging;
using System;
using System.Runtime.CompilerServices;

namespace Pelco.Logging
{
    public interface ILogger : ILoggerFacade
    {
        void IntervalLog(string msg, double seconds = 15, [CallerFilePath] string path = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
        void Log(string msg, [CallerFilePath] string path = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
        void Log(object msg, [CallerFilePath] string path = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
        void Log(string msg, Exception e, string path = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
        void Log(Exception e, [CallerFilePath] string path = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
        void LogThenThrow(Exception e, [CallerFilePath] string path = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);
    }
}