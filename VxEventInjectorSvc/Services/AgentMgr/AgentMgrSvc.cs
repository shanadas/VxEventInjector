using Microsoft.Practices.Unity;
using Pelco.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VxEventAgent;
using VxEventInjectorCommon;
using VxEventInjectorCommon.Services.AgentLoader;
using VxEventInjectorSvc.Services.Cache;
using VxEventInjectorSvc.Services.Serenity;

namespace VxEventInjectorSvc.Services.AgentMgr
{
    class AgentMgrSvc : IAgentMgrSvc
    {
        private Dictionary<string, AgentDetails> _agentCache = new Dictionary<string, AgentDetails>();
        private ICacheSvc _cache;
        private ILogger _logger;
        private IAgentLoaderSvc _loader;
        private IUnityContainer _container;
        private ISerenitySvc _serenity;

        public AgentMgrSvc(ICacheSvc cache, ILogger logger, IAgentLoaderSvc loader, IUnityContainer container, ISerenitySvc serenity)
        {
            _cache = cache;
            _logger = logger;
            _loader = loader;
            _container = container;
            _serenity = serenity;
        }

        public async Task<bool> LoadCachedAgentsAsync()
        {
            _logger.Log("Loading cached agents");
            bool success = true;
            var cacheDetails = _cache.Load();
            if (cacheDetails != null)
            {
                _logger.Log(string.Format("Found {0} cached agents", cacheDetails.AgentIds.Count));

                await _serenity.LoginAsync(cacheDetails.VxCredentials);

                foreach (var agentId in cacheDetails.AgentIds)
                {
                    bool agentSuccess = AddUpdateAgent(agentId);
                    success = success && agentSuccess;
                    if (!agentSuccess)
                        _logger.Log(string.Format("Failed to load cache agent {0}", agentId));
                }
            }
            else
                _logger.Log("No cached agents were found");
            return success;
        }

        public bool AgentExists(string id)
        {
            bool success = false;
            lock(_agentCache)
            {
                success = _agentCache.ContainsKey(id);
            }
            return success;
        }

        public bool AddUpdateAgent(string id)
        {
            var agent = GetEventAgent(id);
            if (agent == null) return false;

            var agentDetails = _container.Resolve<AgentDetails>(new ParameterOverride("eventAgent", agent));
            bool success = _cache.Add(id);

            lock (_agentCache)
            {
                if (AgentExists(id))
                {
                    StopAgent(id);
                    _agentCache[id] = agentDetails;
                }
                else
                    _agentCache.Add(id, agentDetails);
            }
            return success;
        }

        public bool RemoveAgent(string id)
        {
            bool success = _cache.Remove(id);
            lock (_agentCache)
            {
                if (AgentExists(id))
                {
                    StopAgent(id);
                    success = _agentCache.Remove(id) && success;
                }
            }
            return success;
        }

        public bool RemoveAllAgents()
        {
            lock (_agentCache)
            {
                var ids = _agentCache.Keys.ToList();
                foreach (var id in ids)
                    RemoveAgent(id);

                // Good time to refresh the agents we have in the Agents folder
                _loader.LoadAllAgents();
            }
            return true;
        }

        public bool AgentIsRunning(string id)
        {
            bool isRunning = false;
            lock (_agentCache)
            {
                if (AgentExists(id))
                    isRunning = _agentCache[id].IsRunning;
            }
            return isRunning;
        }

        public bool RunAgent(string id)
        {
            bool success = false;
            lock (_agentCache)
            {
                if (AgentExists(id) && !_agentCache[id].IsRunning)
                    success = _agentCache[id].Run();
            }
            return success;
        }

        public bool RunAllAgents()
        {
            bool success = true;
            lock (_agentCache)
            {
                foreach (var id in _agentCache.Keys)
                    success = RunAgent(id) && success;
            }
            return success;
        }

        public void StopAgent(string id)
        {
            lock (_agentCache)
            {
                if (AgentExists(id) && _agentCache[id].IsRunning)
                    _agentCache[id].Stop();
            }
        }

        public void StopAllAgents()
        {
            lock (_agentCache)
            {
                foreach (var id in _agentCache.Keys)
                    StopAgent(id);
            }
        }

        public async Task<bool> InjectSituationsAsync(IConfiguratorCallback callback, string id, List<INewSituation> situations)
        {
            var agent = GetEventAgent(id);
            if (agent == null) return false;
            StopAgent(id);

            situations = situations ?? new List<INewSituation>();
            var serSituations = await _serenity.GetSituations();

            int totalSituations = situations.Count;
            int situationsProcessed = 0;
            var getPercent = new Func<double, double>(soFar => 1.0 * soFar / totalSituations * 100);

            callback.OnSituation(0.0, string.Format("Importing situations for {0}", agent.Name));
            Parallel.ForEach(situations, new ParallelOptions { MaxDegreeOfParallelism = Constants.MaxVxInjectionThreads }, situation =>
            {
                string msg = string.Empty;
                bool success = true;
                bool exists = false;

                if (!string.IsNullOrWhiteSpace(situation.SourceDeviceId))
                {
                    exists = serSituations.FirstOrDefault(val => val.SituationType == situation.Type &&
                        val.SourceDeviceId == situation.SourceDeviceId) != null;
                }
                else
                    exists = serSituations.FirstOrDefault(val => val.SituationType == situation.Type) != null;

                if (!exists)
                {
                    success = _serenity.InjectSituationAsync(situation).Result;
                    msg = string.Format("{0}: adding {1}", success ? Constants.Success : Constants.Failure, situation.Type);
                }
                else
                    msg = string.Format("{0}: pre-existing - {1}", Constants.Success, situation.Type);

                _logger.Log(msg);
                callback.OnSituation(getPercent(++situationsProcessed), msg);
            });

            callback.OnSituation(100.0, string.Format("Completed importing situations for {0}", agent.Name));
            return true;
        }

        private IEventAgent GetEventAgent(string id)
        {
            return _loader.Agents.FirstOrDefault(val => val.Id == id);
        }
    }
}
