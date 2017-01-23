using System.Collections.Generic;
using VxEventAgent;

namespace StressAgent.Models
{
    class SituationModel : INewSituation
    {
        public bool AckNeeded { get; set; }
        public bool Audible { get; set; }
        public uint AutoAcknowledgeTimeout { get; set; }
        public bool Log { get; set; }
        public bool Notify { get; set; }
        public uint Severity { get; set; }
        public List<uint> SnoozeIntervals { get; set; }
        public string Type { get; set; }
        public string SourceDeviceId { get; set; }
        public List<KeyValuePair<ResourceType, string>> Resources { get; set; }

        public SituationModel()
        {
            SnoozeIntervals = new List<uint>();
        }
    }
}
