// Copyright (c) 2013 Ivan Krivyakov

using System;

namespace VxEventInjectorCommon.Services.AgentLoader
{
    public class AgentErrorEventArgs : EventArgs
    {
        public AgentErrorEventArgs(Agent agent, string message, Exception exception)
        {
            Agent = agent;
            Message = message;
            Exception = exception;
        }

        public Agent Agent { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }
    }
}
