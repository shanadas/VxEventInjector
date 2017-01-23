using System;
using System.Collections.Generic;

namespace VxEventAgent
{
    public interface IVxAgentLoader32
    {
        List<AgentCatalogEntry> Get32BitCatalog(string agentDir);
    }

    [Serializable]
    public class AgentCatalogEntry
    {
        public string AssemblyPath { get; set; }
        public string MainClass { get; set; }
    }
}
