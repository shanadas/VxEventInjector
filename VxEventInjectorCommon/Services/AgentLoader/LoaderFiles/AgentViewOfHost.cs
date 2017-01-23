// Copyright (c) 2013 Ivan Krivyakov

using Microsoft.Practices.Unity;
using Prism.Events;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using VxEventAgent;
using VxEventInjectorCommon.Events;

namespace VxEventInjectorCommon.Services.AgentLoader
{
    public class AgentViewOfHost : MarshalByRefObject, IHost, IServer
    {
        public event Action<Exception> FatalError;
        public Exception LastError { get; private set; }
        private IUnityContainer _container;
        private IEventAggregator _eventAgg;

        public AgentViewOfHost(IUnityContainer container, IEventAggregator eventAgg)
        {
            _container = container;
            _eventAgg = eventAgg;
        }

        public void ReportFatalError(string userMessage, string fullExceptionText)
        {
            LastError = new AgentException(userMessage, fullExceptionText);
            if (FatalError != null) FatalError(LastError);
        }

        public int HostProcessId
        {
            get { return Process.GetCurrentProcess().Id; }
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            if (serviceType.IsAssignableFrom(GetType()))
                service = this;
            return service;
        }

        // Live forever
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void InjectEvent(NewEvent evt)
        {
            Task.Run(() => _eventAgg.GetEvent<OnNewEvent>().Publish(evt));
        }
    }
}
