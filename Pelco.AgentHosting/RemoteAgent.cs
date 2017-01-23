// Copyright (c) 2013 Ivan Krivyakov

using System;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using VxEventAgent;

namespace Pelco.AgentHosting
{
    class RemoteAgent : MarshalByRefObject, IRemoteAgent
    {
        public INativeHandleContract Contract { get; private set; }
        private IEventAgent _agent;

        public RemoteAgent(IEventAgent agent, bool createControl)
        {
            _agent = agent;

            if (createControl)
            {
                var control = _agent.CreateControl();
                var localContract = FrameworkElementAdapters.ViewToContractAdapter(control);
                Contract = new NativeHandleContractInsulator(localContract);
            }
        }

        public object GetService(Type serviceType)
        {
            return _agent.GetService(serviceType);
        }

        // Live forever
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Dispose()
        {
            _agent.Dispose();
        }
    }
}
