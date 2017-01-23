using VxEventAgent;

namespace VxEventInjectorSvc.Services.AgentMgr
{
    class AgentDetails
    {
        private IEventAgent _eventAgent;

        public AgentDetails(IEventAgent eventAgent)
        {
            _eventAgent = eventAgent;
        }
        
        public bool IsRunning
        {
            get { return _eventAgent.IsRunning; }
        }

        public void Stop()
        {
            _eventAgent.Stop();
        }

        public bool Run()
        {
            return _eventAgent.Run();
        }
    }
}
