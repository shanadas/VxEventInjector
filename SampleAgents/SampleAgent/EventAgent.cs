using Microsoft.Practices.Unity;
using Pelco.Logging;
using Prism.Events;
using SampleAgent.Events;
using SampleAgent.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using VxEventAgent;

namespace SampleAgent
{
    class EventAgent : EventAgentBase
    {
        private IHost _host;
        private Bootstrapper _bootstrapper = new Bootstrapper(true);
        private bool _isConfigured;
        private ILogger _logger;
        private bool _isRunning;

        public EventAgent(IHost host)
        {
            _host = host;
            _bootstrapper.Run();
            var eventAgg = _bootstrapper.Container.Resolve<IEventAggregator>();
            eventAgg.GetEvent<ConfiguredEvent>().Subscribe(val => _isConfigured = val);
            _logger = _bootstrapper.Container.Resolve<ILogger>();
        }

        #region EventAgentBase
        /// <summary>
        /// This specifies this particular agent among all the others. It should be a UUID and never
        /// change once set, it will stay the same for all versions of this event agent. Required.
        /// </summary>
        public override string Id
        {
            get { return "791D24EB-354F-46B1-963F-19C8D00BC64F"; }
        }

        /// <summary>
        /// Name of the event agent. Required.
        /// e.g. SavVi Event Agent
        /// </summary>
        public override string Name
        {
            get { return "Sample Event Agent"; }
        }

        /// <summary>
        /// Version of the event agent. Required.
        /// e.g. 1.0.3
        /// </summary>
        public override string Version
        {
            get { return "1.0.0"; }
        }

        /// <summary>
        /// Manufacturer of the event producing device. Required.
        /// e.g. AgentVi
        /// </summary>
        public override string Manufacturer
        {
            get { return "Pelco"; }
        }

        /// <summary>
        /// The company authoring this event agent. Required
        /// e.g. Pelco
        /// </summary>
        public override string Author
        {
            get { return "Pelco"; }
        }

        /// <summary>
        /// Short description of what device events are queried from. Required
        /// e.g. Collects events from the AgentVi savVi system
        /// </summary>
        public override string Description
        {
            get { return "Injects events from the widget server"; }
        }

        /// <summary>
        /// Returns a List of all possible Situations which are required by the event agent. Required.
        /// </summary>
        public override List<NewSituation> Situations
        {
            get { return GetSituations(); }
        }

        /// <summary>
        /// Returns true if the event agent is currently running and listening for events from the third party. Required
        /// </summary>
        public override bool IsRunning
        {
            get { return _isRunning; }
        }

        /// <summary>
        /// Signifies if the event agent is currently properly configured. Required
        /// </summary>
        public override bool IsConfigured
        {
            get { return _isConfigured; }
        }

        /// <summary>
        /// This indentifies at a glance whether the event agent requires a control via CreateControlAsync or not.
        /// if this returns false, then CreateControlAsync should return null. Required.
        /// </summary>
        public override bool RequiresControl
        {
            get { return true; }
        }

        /// <summary>
        /// CreateControl will allow the event
        /// agent to present an interface to the user if user configuration is required to
        /// properly configure the event agent. Optional.
        /// </summary>
        public override FrameworkElement CreateControl()
        {
            return _bootstrapper.Container.Resolve<ShellCtrlView>();
        }

        /// <summary>
        /// Run is called by the windows service and should start its own thread to start listening for
        /// events from it's configured event generating device. If everything is started properly
        /// then true should be returned. All other properties, especially Situations should
        /// be completed before calling Run. Required.
        /// </summary>
        public override bool Run()
        {
            //Task.Run(() => InjectEvents());
            bool success = _isRunning = true;
            _logger.Log(string.Format("Run: {0}", success ? "true" : "false"));
            return success;
        }

        /// <summary>
        /// Stop should stop the event agent from listening to it's event generating device's events. Required.
        /// </summary>
        public override void Stop()
        {
            _isRunning = false;
            _logger.Log("Stop: True");
        }
        #endregion

        private List<NewSituation> GetSituations()
        {
            var situations = new List<NewSituation>();
            for (int i = 0; i < 20; i++)
            {
                situations.Add(new NewSituation()
                {
                    AckNeeded = false,
                    Audible = false,
                    AutoAcknowledgeTimeout = 60 * 5,
                    Log = true,
                    Notify = true,
                    Severity = 3,
                    SnoozeIntervals = null,
                    Type = string.Format("external/sample_agent/test_{0}", i)
                });
            }
            return situations;
        }

        private void InjectEvents()
        {
            _logger.Log("Start InjectEvents!!!!!");
            var server = (IServer)_host.GetService(typeof(IServer));
            for(int i = 0; i < 5000; i++)
                server.InjectEvent(CreateEvent());
            _logger.Log("End InjectEvents!!!!!");
        }

        private NewEvent CreateEvent()
        {
            var newEvent = new NewEvent()
            {
                Properties = null,
                SituationType = "external/sample_agent/test_0",
                SourceDeviceId = "uuid:dd5dfe19-85ad-4077-9e27-04f35b1b47ff",
                SourceUsername = null,
                Time = DateTime.UtcNow
            };
            return newEvent;
        }
    }
}
