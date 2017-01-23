using System.Collections.Generic;
using System.Threading.Tasks;
using VxEventAgent;
using VxEventInjectorCommon;

namespace VxEventInjectorSvc.Services.AgentMgr
{
    interface IAgentMgrSvc
    {
        Task<bool> LoadCachedAgentsAsync();
        bool AgentExists(string id);
        bool AddUpdateAgent(string id);
        bool RemoveAgent(string id);
        bool RemoveAllAgents();
        bool AgentIsRunning(string id);
        bool RunAgent(string id);
        bool RunAllAgents();
        void StopAgent(string id);
        void StopAllAgents();
        Task<bool> InjectSituationsAsync(IConfiguratorCallback callback, string id, List<INewSituation> situations);
    }
}
