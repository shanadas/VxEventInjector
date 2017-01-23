using System.Collections.Generic;
using VxEventAgent;

namespace StressAgent.Services
{
    interface IEventAgentSvc : IEventAgent
    {
        new List<NewSituation> Situations { get; set; }
        IHost Host { get; set; }
        string DocumentsDir { get; set; }
    }
}
