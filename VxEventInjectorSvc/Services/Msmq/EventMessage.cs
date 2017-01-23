using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VxEventAgent;

namespace VxEventInjectorSvc.Services.Msmq
{
    [Serializable]
    public class EventMessage : NewEvent, ISerializable
    {
        public uint FailedAttempts { get; set; }

        public EventMessage(INewEvent evt)
        {
            SituationType = evt.SituationType;
            SourceDeviceId = evt.SourceDeviceId;
            SourceUsername = evt.SourceUsername;
            Properties = evt.Properties;
            Time = evt.Time;
            FailedAttempts = 0;
        }

        public EventMessage(SerializationInfo info, StreamingContext context)
        {
            SituationType = (string)info.GetValue("SituationType", typeof(string));
            SourceDeviceId = (string)info.GetValue("SourceDeviceId", typeof(string));
            SourceUsername = (string)info.GetValue("SourceUsername", typeof(string));
            Properties = (Dictionary<string, string>)info.GetValue("Properties", typeof(Dictionary<string, string>));
            Time = (DateTime)info.GetValue("Time", typeof(DateTime));
            FailedAttempts = (uint)info.GetValue("FailedAttempts", typeof(uint));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SituationType", SituationType);
            info.AddValue("SourceDeviceId", SourceDeviceId);
            info.AddValue("SourceUsername", SourceUsername);
            info.AddValue("Properties", Properties);
            info.AddValue("Time", Time);
            info.AddValue("FailedAttempts", FailedAttempts);
        }
    }
}
