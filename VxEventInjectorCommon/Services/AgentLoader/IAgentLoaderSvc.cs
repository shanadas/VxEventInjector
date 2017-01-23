using System.Collections.Generic;
using VxEventAgent;

namespace VxEventInjectorCommon.Services.AgentLoader
{
    public interface IAgentLoaderSvc
    {
        /// <summary>
        /// Should be called early in the application
        /// </summary>
        void LoadAllAgents();

        /// <summary>
        /// Returns all of the found agents
        /// </summary>
        List<IEventAgent> Agents { get; }
    }
}
