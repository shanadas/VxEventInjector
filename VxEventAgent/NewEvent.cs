using System;
using System.Collections.Generic;

namespace VxEventAgent
{
    [Serializable]
    public class NewEvent : MarshalByRefObject, INewEvent
    {
        public virtual string SituationType { get; set; }
        public virtual string SourceDeviceId { get; set; }
        public virtual string SourceUsername { get; set; }
        public virtual Dictionary<string, string> Properties { get; set; }
        public virtual DateTime Time { get; set; }
    }
}
