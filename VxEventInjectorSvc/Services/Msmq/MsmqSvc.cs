using Pelco.Logging;
using System;
using System.Linq;
using System.Messaging;
using System.Threading;
using VxEventAgent;

namespace VxEventInjectorSvc.Services.Msmq
{
    class MsmqSvc : IMsmqSvc
    {
        public event EventHandler<INewEvent> OnMsmqEvent;
        private Semaphore _eventSem = new Semaphore(0, 10);
        private const uint MaxFailedAttempts = 100;
        private const string _QueueName = @".\Private$\VxEventInjector";
        private MessageQueue _mq;
        private ILogger _logger;

        public MsmqSvc(ILogger logger)
        {
            _logger = logger;
            if (MessageQueue.Exists(_QueueName))
                _mq = new MessageQueue(_QueueName);
            else
                _mq = MessageQueue.Create(_QueueName);
            _mq.Formatter = new BinaryMessageFormatter();
            _mq.PeekCompleted += MQ_PeekCompleted;
            _mq.BeginPeek();
        }

        public void StartMonitoringMsmq()
        {
            _eventSem.Release(10);
        }

        private void MQ_PeekCompleted(object sender, PeekCompletedEventArgs e)
        {
            _mq.EndPeek(e.AsyncResult);
            _eventSem.WaitOne();

            if (OnMsmqEvent != null)
            {
                var message = _mq.Receive();
                var evt = message.Body as INewEvent;
                var action = new WaitCallback(_ =>
                {
                    OnMsmqEvent?.Invoke(this, evt);
                    _eventSem.Release();
                });
                ThreadPool.QueueUserWorkItem(action);
            }
            _mq.BeginPeek();
        }

        public bool AddEvent(INewEvent evt)
        {
            bool success = false;
            var evtMessage = evt as EventMessage;
            if (evtMessage == null)
                evtMessage = new EventMessage(evt);
            else
            {
                evtMessage.FailedAttempts++;
                if (evtMessage.FailedAttempts > MaxFailedAttempts)
                {
                    _logger.Log(string.Format("An event has failed {0} times, so it is being dropped from the queue. {1}: {2} / {3}",
                        MaxFailedAttempts, evtMessage.Time, evtMessage.SourceDeviceId, evtMessage.SituationType));
                    evtMessage = null;
                }
            }

            if(evtMessage != null)
            {
                try
                {
                    using (var message = new Message(evtMessage, new BinaryMessageFormatter()))
                    {
                        message.Recoverable = true;
                        var label = evt.SituationType.Split('/').Last();
                        _mq.Send(message, string.Format("{0} - {1}", evt.Time, label));
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    _logger.Log("Failed to add an event", e);
                }
            }
            return success;
        }

        public void Dispose()
        {
            _mq.PeekCompleted -= MQ_PeekCompleted;
            _mq.Close();
            _mq.Dispose();
        }
    }
}
