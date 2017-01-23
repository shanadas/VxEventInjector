using System;

namespace VxEventAgent
{
    public interface IHost : IServiceProvider
    {
        /// <summary>
        /// Reports fatal agent error to the host; the agent will be closed
        /// </summary>
        /// <param name="userMessage">Message explaining the nature of the error</param>
        /// <param name="fullExceptionText">Exception call stack as string</param>
        void ReportFatalError(string userMessage, string fullExceptionText);

        /// <summary>
        /// ID of the host process
        /// </summary>
        int HostProcessId { get; }
    }
}
