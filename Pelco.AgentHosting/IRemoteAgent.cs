// Copyright (c) 2013 Ivan Krivyakov

using System;
using System.AddIn.Contract;

namespace Pelco.AgentHosting
{
    public interface IRemoteAgent : IServiceProvider, IDisposable
    {
        INativeHandleContract Contract { get; }
    }
}
