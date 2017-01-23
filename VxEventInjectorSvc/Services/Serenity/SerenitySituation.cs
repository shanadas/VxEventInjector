using System.Collections.Generic;

namespace VxEventInjectorSvc.Services.Serenity
{
    public class SerenitySituation
    {
        public List<int> SnoozeIntervals { get; set; }
        public string SituationType { get; set; }
        public bool AckNeeded { get; set; }
        public bool AudibleNotify { get; set; }
        public int AutoAck { get; set; }
        public bool Log { get; set; }
        public List<string> NotificationIds { get; set; }
        public bool Notify { get; set; }
        public int Severity { get; set; }
        public string SourceDeviceId { get; set; }
    }
}
