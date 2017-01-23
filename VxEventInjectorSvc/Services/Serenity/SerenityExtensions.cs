using System.Collections.Generic;
using System.Linq;
using VxEventAgent;

namespace VxEventInjectorSvc.Services.Serenity
{
    public static class SerenityExtensions
    {
        public static CPPCli.NewSituation ToSerNewSituation(this INewSituation situation)
        {
            var serNewSituation = new CPPCli.NewSituation()
            {
                IsAckNeeded = situation.AckNeeded,
                UseAudibleNotification = situation.Audible,
                AutoAcknowledge = situation.AutoAcknowledgeTimeout == 0 ? -1 : (int)situation.AutoAcknowledgeTimeout,
                ShouldLog = situation.Log,
                ShouldNotify = situation.Notify,
                Severity = (int)situation.Severity,
                Type = situation.Type,
                SnoozeIntervals = (situation.SnoozeIntervals ?? new List<uint>()).Select(i => (int)i).ToList(),
                SourceDeviceId = situation.SourceDeviceId
            };
            return serNewSituation;
        }

        public static CPPCli.NewEvent ToSerNewEvent(this INewEvent evt)
        {
            var properties = new List<KeyValuePair<string, string>>();
            foreach (var props in evt.Properties)
                properties.Add(new KeyValuePair<string, string>(props.Key, props.Value));

            var serNewEvent = new CPPCli.NewEvent()
            {
                Properties = properties,
                SourceDeviceId = evt.SourceDeviceId,
                //SourceUserName = evt.SourceUsername,
                //GeneratorDeviceId =
                Time = evt.Time,
                SituationType = evt.SituationType
            };
            return serNewEvent;
        }

        public static SerenitySituation ToSerenitySituation(this CPPCli.Situation serSituation)
        {
            var serenitySituation = new SerenitySituation()
            {
                AckNeeded = serSituation.IsAckNeeded,
                AudibleNotify = serSituation.UseAudibleNotification,
                AutoAck = serSituation.AutoAcknowledge,
                Log = serSituation.ShouldLog,
                NotificationIds = (serSituation.Notifications ?? new List<CPPCli.Notification>()).Select(notification => notification.Id).ToList(),
                Notify = serSituation.ShouldNotify,
                Severity = serSituation.Severity,
                SituationType = serSituation.Type,
                SnoozeIntervals = serSituation.SnoozeIntervals,
                SourceDeviceId = serSituation.SourceDeviceId
            };
            return serenitySituation;
        }
    }
}