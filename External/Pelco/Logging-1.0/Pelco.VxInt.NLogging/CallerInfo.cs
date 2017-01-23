using System;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;

namespace Pelco.VxInt.Logging
{
    /// <summary>
    /// adapted from <a href="http://exceptionalcode.wordpress.com/2012/06/09/extended-logging-with-caller-info-attributes/"></a>
    /// </summary>
    public sealed class CallerInfo
    {
        private CallerInfo(string filePath, string memberName, int lineNumber)
        {
            this.FilePath = filePath;
            this.MemberName = memberName;
            this.LineNumber = lineNumber;
        }

        public static CallerInfo Create([CallerFilePath] string filePath = "",
                                        [CallerMemberName] string memberName = "",
                                        [CallerLineNumber] int lineNumber = 0)
        {
            return new CallerInfo(filePath, memberName, lineNumber);
        }

        public string FilePath { get; private set; }

        private string _fileName;
        public string FileName
        {
            get
            {
                return this._fileName ?? (this._fileName = Path.GetFileName(this.FilePath));
            }
        }

        public string MemberName { get; private set; }

        public int LineNumber { get; private set; }

        public override string ToString()
        {
            return string.Concat(this.FilePath, ".", this.MemberName, ":", this.LineNumber);
        }
    }
}
