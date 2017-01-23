// Copyright (c) 2013 Ivan Krivyakov

using System;
using VxEventAgent;

namespace Pelco.AgentHosting
{
    public interface IAgentLoader : IDisposable
    {
        IRemoteAgent LoadAgent(IHost host, AgentStartupInfo startupInfo);
    }
}
