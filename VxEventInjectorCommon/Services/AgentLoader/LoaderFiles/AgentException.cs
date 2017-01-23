// Copyright (c) 2013 Ivan Krivyakov

using System;

namespace VxEventInjectorCommon.Services.AgentLoader
{
    public class AgentException : Exception
    {
        private string _fullExceptionText;

        public AgentException(string userMessage, string fullExceptionText)
            : base(userMessage)
        {
            _fullExceptionText = fullExceptionText;
        }

        public override string ToString()
        {
            return string.Format("Agent exception: {0}", _fullExceptionText);
        }
    }
}
