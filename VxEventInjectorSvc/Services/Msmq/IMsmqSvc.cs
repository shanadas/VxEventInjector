using System;
using VxEventAgent;

namespace VxEventInjectorSvc.Services.Msmq
{
    interface IMsmqSvc : IDisposable
    {
        event EventHandler<INewEvent> OnMsmqEvent;
        bool AddEvent(INewEvent evt);
        void StartMonitoringMsmq();
    }
}
