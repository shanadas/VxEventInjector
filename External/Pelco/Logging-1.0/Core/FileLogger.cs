using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Pelco.Logging
{
    public class FileLogger : LoggerBase
    {
        private string _logPath;
        private bool _logToFileWhenDebugging;

        public FileLogger(string logPath, bool logToFileWhenDebugging = false)
        {
            _logPath = logPath;
            _logToFileWhenDebugging = logToFileWhenDebugging;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_logPath)));
        }

        protected override void Log(DateTime date, string path, string memberName, int lineNumber, string msg)
        {
            var logMsg = string.Format("{0} - {1}:{2}:{3} - {4}{5}",
                date.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                path, memberName, lineNumber, msg, Environment.NewLine);

            if (Debugger.IsAttached)
            {
                Debug.WriteLine(logMsg);
                if (_logToFileWhenDebugging) File.AppendAllText(_logPath, logMsg);
            }
            else
            {
                File.AppendAllText(_logPath, logMsg);
            }

        }
    }
}
