using Pelco.Logging;
using Prism.Events;
using VxEventAgent;
using VxEventInjectorCommon.Events;
using VxEventInjectorSvc.Services.Msmq;

namespace VxEventInjectorSvc.Services.Server
{
    class ServerSvc
    {
        private ILogger _logger;
        private IMsmqSvc _msmq;
        private IEventAggregator _eventAgg;

        public ServerSvc(ILogger logger, IMsmqSvc msmq, IEventAggregator eventAgg)
        {
            _logger = logger;
            _msmq = msmq;
            _eventAgg = eventAgg;
            _eventAgg.GetEvent<OnNewEvent>().Subscribe(InjectEvent);
        }

        private void InjectEvent(INewEvent evt)
        {
            _msmq.AddEvent(evt);
        }
    }
}
