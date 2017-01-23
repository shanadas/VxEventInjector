using Microsoft.Practices.Unity;
using StressAgent.Services;
using System.Collections.Generic;
using System.Windows;
using VxEventAgent;

namespace StressAgent
{
    class EventAgent : EventAgentBase
    {
        private Bootstrapper _bootstrapper;
        private IEventAgentSvc _eventAgentSvc;

        public EventAgent(IHost host)
        {
            _eventAgentSvc = new EventAgentSvc(null);
            _bootstrapper = new Bootstrapper(true, EventAgentDocumentsDir);
            _bootstrapper.Run();
            _eventAgentSvc = _bootstrapper.Container.Resolve<IEventAgentSvc>();
            _eventAgentSvc.Host = host;
            _eventAgentSvc.DocumentsDir = EventAgentDocumentsDir;
        }

        public override string Id
        {
            get { return _eventAgentSvc.Id; }
        }

        public override string Name
        {
            get { return _eventAgentSvc.Name; }
        }

        public override string Version
        {
            get { return _eventAgentSvc.Version; }
        }

        public override string Manufacturer
        {
            get { return _eventAgentSvc.Manufacturer; }
        }

        public override string Author
        {
            get { return _eventAgentSvc.Author; }
        }

        public override string Description
        {
            get { return _eventAgentSvc.Description; }
        }

        public override List<NewSituation> Situations
        {
            get { return _eventAgentSvc.Situations; }
        }

        public override bool IsRunning
        {
            get { return _eventAgentSvc.IsRunning; }
        }

        public override bool IsConfigured
        {
            get { return _eventAgentSvc.IsConfigured; }
        }

        public override bool RequiresControl
        {
            get { return _eventAgentSvc.RequiresControl; }
        }

        public override FrameworkElement CreateControl()
        {
            return _eventAgentSvc.CreateControl();
        }

        public override bool Run()
        {
            return _eventAgentSvc.Run();
        }

        public override void Stop()
        {
            _eventAgentSvc.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            _eventAgentSvc.Dispose();
            base.Dispose(disposing);
        }
    }
}
