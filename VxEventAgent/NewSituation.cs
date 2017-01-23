using System;
using System.Collections.Generic;

namespace VxEventAgent
{
    [Serializable]
    public class NewSituation : MarshalByRefObject, INewSituation
    {
        public virtual bool AckNeeded { get; set; }
        public virtual bool Audible { get; set; }
        public virtual uint AutoAcknowledgeTimeout { get; set; }
        public virtual bool Log { get; set; }
        public virtual bool Notify { get; set; }
        public virtual uint Severity { get; set; }
        public virtual List<uint> SnoozeIntervals { get; set; }
        public virtual string Type { get; set; }
        public virtual string SourceDeviceId { get; set; }
        public virtual List<KeyValuePair<ResourceType, string>> Resources { get; set; }
    }
}
