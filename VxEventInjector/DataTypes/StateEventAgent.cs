using System;
using System.Collections.Generic;
using System.Windows;
using VxEventAgent;

namespace VxEventInjector
{
    class StateEventAgent : IEventAgent
    {
        private IEventAgent _agent;

        public StateEventAgent(IEventAgent agent)
        {
            _agent = agent;
        }

        public bool IsRemotelyConfigured { get; set; }
        public bool KeepConfiguration { get; set; }

        #region IEventAgent
        public string Id
        {
            get { return _agent.Id; }
        }

        public string Name
        {
            get { return _agent.Name; }
        }

        public string Version
        {
            get { return _agent.Version; }
        }

        public string Manufacturer
        {
            get { return _agent.Manufacturer; }
        }

        public string Author
        {
            get { return _agent.Author; }
        }

        public string Description
        {
            get { return _agent.Description; }
        }

        public List<NewSituation> Situations
        {
            get { return _agent.Situations; }
        }

        public bool IsRunning
        {
            get { return _agent.IsRunning; }
        }

        public bool IsConfigured
        {
            get { return KeepConfiguration || _agent.IsConfigured; }
        }

        public bool RequiresControl
        {
            get { return _agent.RequiresControl; }
        }

        public FrameworkElement CreateControl()
        {
            return _agent.CreateControl();
        }

        public bool Run()
        {
            return _agent.Run();
        }

        public void Stop()
        {
            _agent.Stop();
        }

        public object GetService(Type serviceType)
        {
            return _agent.GetService(serviceType);
        }

        public void Dispose()
        {
            _agent.Dispose();
        }
        #endregion
    }
}
