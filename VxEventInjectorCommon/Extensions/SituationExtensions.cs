using System.Collections.Generic;
using VxEventAgent;

namespace VxEventInjectorCommon.Extensions
{
    public static class SituationExtensions
    {
        public static BasicNewSituation ToSanitizedNewSituation(this INewSituation situation)
        {
            var basicSituation = new BasicNewSituation()
            {
                AckNeeded = situation.AckNeeded,
                Audible = situation.Audible,
                AutoAcknowledgeTimeout = situation.AutoAcknowledgeTimeout,
                Log = situation.Log,
                Notify = situation.Notify,
                Severity = situation.Severity,
                SnoozeIntervals = situation.SnoozeIntervals,
                Type = situation.Type,
                SourceDeviceId = situation.SourceDeviceId,
                Resources = situation.Resources
            };

            // Adjustments to avoid weirdly configured situations
            if(situation.Notify)
            {
                if(situation.AckNeeded)
                    basicSituation.AutoAcknowledgeTimeout = 0;
                else
                    basicSituation.SnoozeIntervals = new List<uint>();
            }
            else
            {
                basicSituation.AutoAcknowledgeTimeout = 0;
                basicSituation.Audible = false;
                basicSituation.AckNeeded = false;
                basicSituation.SnoozeIntervals = new List<uint>();
            }
            return basicSituation;
        }
    }
}