using Pelco.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VxEventInjectorSvc.Services.Msmq;
using VxEventInjectorSvc.Services.Serenity;

namespace VxEventInjectorSvc.Services.EventInjector
{
    class EventInjector : IEventInjector
    {
        private ILogger _logger;
        private IMsmqSvc _msmq;
        private ISerenitySvc _serenity;
        private List<SerenitySituation> _situations;
        private readonly object _situationsLock = new object();

        public EventInjector(ILogger logger, IMsmqSvc msmq, ISerenitySvc serenity)
        {
            _logger = logger;
            _msmq = msmq;
            _serenity = serenity;
        }

        private void MSMQ_OnMsmqEvent(object sender, VxEventAgent.INewEvent evt)
        {
            InjectEvent(evt).Wait();
        }

        public async void StartLoop()
        {
            await UpdateSituations();
            _msmq.OnMsmqEvent += MSMQ_OnMsmqEvent;
            _msmq.StartMonitoringMsmq();
        }

        public async Task UpdateSituations()
        {
            var situations = await _serenity.GetSituations(false);
            lock(_situationsLock)
            {
                _situations = (situations ?? _situations ?? new List<SerenitySituation>());
            }
        }

        private async Task InjectEvent(VxEventAgent.INewEvent evt)
        {
            if (evt == null) return;

            try
            {
                if (_serenity.IsLoggedIn)
                {
                    bool situationExistsOnVx = false;
                    lock (_situationsLock)
                    {
                        situationExistsOnVx = _situations.Any(val => val.SituationType == evt.SituationType);
                    }

                    if (situationExistsOnVx)
                    {
                        bool success = await _serenity.InjectEventAsync(evt).ConfigureAwait(false);
                        if (!success)
                        {
                            _logger.IntervalLog(string.Format("Unable to inject event for some reason - Situation: {0}", evt.SituationType));
                            _msmq.AddEvent(evt);
                        }
                    }
                    else
                    {
                        _logger.IntervalLog($"Unable to inject event, situation doesn't exist in VideoXpert - Situation: {evt.SituationType}");
                        _msmq.AddEvent(evt);
                    }
                }
                else
                    _msmq.AddEvent(evt);
            }
            catch (Exception ex)
            {
                _logger.Log("Threw and exited the run loop", ex);
            }
        }

        public void StopLoop()
        {
            _msmq.OnMsmqEvent -= MSMQ_OnMsmqEvent;
        }
    }
}
