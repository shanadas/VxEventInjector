using Pelco.Logging;
using System;
using System.Collections.Generic;
using System.Windows;
using VxEventAgent;

namespace VxEventInjectorCommon.Services.AgentLoader
{
    public class AgentFacade : IEventAgent
    {
        private Agent _agent;
        private IEventAgent _eventAgent;
        private ILogger _logger;

        public AgentFacade(Agent agent, ILogger logger)
        {
            _agent = agent;
            _logger = logger;
            _eventAgent = (IEventAgent)_agent.GetService(typeof(IEventAgent));
        }

        public string Id
        {
            get { return _eventAgent.Id; }
        }

        public string Name
        {
            get { return _eventAgent.Name; }
        }

        public string Version
        {
            get { return _eventAgent.Version; }
        }

        public string Manufacturer
        {
            get { return _eventAgent.Manufacturer; }
        }

        public string Author
        {
            get { return _eventAgent.Author; }
        }

        public string Description
        {
            get { return _eventAgent.Description; }
        }

        public List<NewSituation> Situations
        {
            get { return _eventAgent.Situations; }
        }

        public bool IsRunning
        {
            get { return _eventAgent.IsRunning; }
        }

        public bool IsConfigured
        {
            get { return _eventAgent.IsConfigured; }
        }

        public bool RequiresControl
        {
            get { return _eventAgent.RequiresControl; }
        }

        public FrameworkElement CreateControl()
        {
            FrameworkElement element = null;
            try
            {
                if (_agent.View == null)
                    _agent.CreateView();
                element = _agent.View;
            }
            catch (Exception e)
            {
                _logger.Log("Failed to create event agent control", e);
            }
            return element;
        }

        public bool Run()
        {
            bool success = false;
            try
            {
                success = _eventAgent.Run();
            }
            catch (Exception e)
            {
                _logger.Log("Failed to Run the event agent", e);
            }
            return success;
        }

        public void Stop()
        {
            try
            {
                _eventAgent.Stop();
            }
            catch (Exception e)
            {
                _logger.Log("Failed to Stop the event agent", e);
            }
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            try
            {
                service = _eventAgent.GetService(serviceType);
            }
            catch (Exception e)
            {
                _logger.Log("Failed to GetService from the event agent", e);
            }
            return service;
        }

        public void Dispose()
        {
            _agent.Dispose();
            _agent = null;
            _eventAgent = null;
        }
    }
}
