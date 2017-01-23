using System;
using System.Collections.Generic;

namespace VxEventAgent
{
    public interface INewEvent
    {
        /// <summary>
        /// MUST be of the form external/<company>/<event> where <company> and <event> are UTF-8
        /// strings no greater than 64 characters each; forward slashes are not allowed.
        /// These strings describe the <company> that manufactured the device that was the
        /// source of the <event>. Required.
        /// e.g. /external/onguard/swipe
        /// </summary>
        string SituationType { get; }

        /// <summary>
        /// This is the unique source device id that is associated with the event generating device. Required.
        /// e.g. bldg_3_reader_2
        /// </summary>
        string SourceDeviceId { get; }

        /// <summary>
        /// The user that has caused the event. A User Principal Name has two String parts:
        /// the UPN prefix (a user name) and a UPN suffix (a domain name). The parts are joined
        /// together by the at symbol (@) to make the complete UPN. The UPN prefix alone MAY be
        /// provided; this implies the LOCAL domain. Optional.
        /// e.g. “JDoe@Example” or “JDoe”
        /// </summary>
        string SourceUsername { get; }

        /// <summary>
        /// Properties contains additional key/value information related to the event.
        /// If the event has no properties, this may be omitted. Optional.
        /// </summary>
        Dictionary<string, string> Properties { get; }

        /// <summary>
        /// Time at which the situation occurred in UTC. The DateTimeKind must be set. Required.
        /// </summary>
        DateTime Time { get; }
    }
}
