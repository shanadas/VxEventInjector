using System;
using System.Collections.Generic;
using System.Windows;
using VxEventAgent;

namespace MinimalEventAgent
{
    class EventAgent : EventAgentBase
    {
        private IHost _host;
        private bool _isConfigured;
        private bool _isRunning;
        private EventAgentCtrl _agentCtrl;

        public EventAgent(IHost host)
        {
            _host = host;
        }

        #region EventAgentBase
        /// <summary>
        /// This specifies this particular agent among all the others. It should be a UUID and never
        /// change once set, it will stay the same for all versions of this event agent. Required.
        /// </summary>
        public override string Id
        {
            get { return "B17E3803-4CFA-42B7-A2A9-6993091836CF"; }
        }

        /// <summary>
        /// Name of the event agent. Required.
        /// e.g. SavVi Event Agent
        /// </summary>
        public override string Name
        {
            get { return "Sample Minimal Event Agent"; }
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
            get { return "Injects events from the minimal widget server"; }
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
            if (_agentCtrl == null)
            {
                _agentCtrl = new EventAgentCtrl();
                _agentCtrl.NameModifiedEvent += AgentCtrl_NameModifiedEvent;
            }
            return _agentCtrl;
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
            return success;
        }

        /// <summary>
        /// Stop should stop the event agent from listening to it's event generating device's events. Required.
        /// </summary>
        public override void Stop()
        {
            _isRunning = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (_agentCtrl != null)
            {
                _agentCtrl.NameModifiedEvent -= AgentCtrl_NameModifiedEvent;
                _agentCtrl = null;
            }
            base.Dispose(disposing);
        }
        #endregion

        private void AgentCtrl_NameModifiedEvent(object sender, string e)
        {
            _isConfigured = !string.IsNullOrWhiteSpace(e);
        }

        private List<NewSituation> GetSituations()
        {
            var situations = new List<NewSituation>();
            for (int i = 0; i < 100; i++)
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
                    Type = string.Format("external/minimal_agent/test_{0}", i)
                });
            }
            return situations;
        }

        private void InjectEvents()
        {
            var server = (IServer)_host.GetService(typeof(IServer));
            for (int i = 0; i < 1000; i++)
                server.InjectEvent(CreateEvent());
        }

        private NewEvent CreateEvent()
        {
            var newEvent = new NewEvent()
            {
                Properties = null,
                SituationType = "external/minimal_agent/test_0",
                SourceDeviceId = "uuid:daa28172-e31a-44ce-8485-ecbbb5e7b609",
                SourceUsername = null,
                Time = DateTime.UtcNow
            };
            return newEvent;
        }
    }
}
