using Prism.Logging;
using System;
using System.IO;
using System.Runtime.Caching;
using System.Text;

namespace Pelco.Logging
{
    public abstract class LoggerBase : ILogger
    {
        private static readonly object _LogLock = new object();
        private MemoryCache _intervalCache = new MemoryCache("LoggerBase#interval");

        abstract protected void Log(DateTime date, string path, string memberName, int lineNumber, string msg);

        public void IntervalLog(string msg, double seconds = 15, string path = "", string memberName = "", int lineNumber = 0)
        {
            lock (_LogLock)
            {
                bool added = _intervalCache.Add(msg, msg, new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(seconds) });
                if(added)
                    Log(msg, path, memberName, lineNumber);
            }
        }

        public void Log(string msg, string path = "", string memberName = "", int lineNumber = 0)
        {
            lock (_LogLock)
            {
                Log(DateTime.Now, Path.GetFileNameWithoutExtension(path), memberName, lineNumber, msg);
            }
        }

        public void Log(object msg, string path = "", string memberName = "", int lineNumber = 0)
        {
            if (msg != null)
            {
                var msgString = msg.ToString();
                if (!string.IsNullOrWhiteSpace(msgString))
                    Log(msgString, path, memberName, lineNumber);
            }
        }

        public void Log(string msg, Exception e, string path = "", string memberName = "", int lineNumber = 0)
        {
            lock (_LogLock)
            {
                Log(msg, path, memberName, lineNumber);
                Log(e, path, memberName, lineNumber);
            }
        }

        public void Log(Exception e, string path = "", string memberName = "", int lineNumber = 0)
        {
            if (e == null)
                return;

            var sb = new StringBuilder();
            sb.AppendLine(e.GetType().ToString());
            if (!string.IsNullOrWhiteSpace(e.Message))
                sb.AppendLine(e.Message);
            if (!string.IsNullOrWhiteSpace(e.StackTrace))
                sb.AppendLine(e.StackTrace);
            if (e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message))
                sb.AppendLine(e.InnerException.Message);
            if (e.InnerException != null && e.InnerException.StackTrace != null)
                sb.AppendLine(e.InnerException.StackTrace);

            Log(sb.ToString(), path, memberName, lineNumber);
        }

        public void LogThenThrow(Exception e, string path = "", string memberName = "", int lineNumber = 0)
        {
            Log(e, path, memberName, lineNumber);
            throw e;
        }

        public void Log(string msg, Category category, Priority priority)
        {
            var message = string.Format("{0} {1} - {2}", category, priority, msg);
            Log(msg);
        }
    }
}
