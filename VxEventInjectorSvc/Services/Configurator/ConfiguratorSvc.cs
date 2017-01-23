using Pelco.Logging;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using VxEventAgent;
using VxEventInjectorCommon;
using VxEventInjectorSvc.Services.AgentMgr;
using VxEventInjectorSvc.Services.Cache;
using VxEventInjectorSvc.Services.EventInjector;
using VxEventInjectorSvc.Services.Serenity;

namespace VxEventInjectorSvc.Services.Configurator
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    class ConfiguratorSvc : IConfiguratorSvc
    {
        private ServiceHost _svcHost;
        private ILogger _logger;
        private IAgentMgrSvc _agentMgr;
        private ICacheSvc _cache;
        private ISerenitySvc _serenity;
        private IEventInjector _injector;

        public ConfiguratorSvc(ILogger logger, IAgentMgrSvc agentMgr, ICacheSvc cache, ISerenitySvc serenity, IEventInjector injector)
        {
            _logger = logger;
            _agentMgr = agentMgr;
            _cache = cache;
            _serenity = serenity;
            _injector = injector;
        }

        #region IConfiguratorSvc
        public async Task<bool> StartListeningAsync()
        {
            if (_svcHost != null) return true;

            _logger.Log("Engine started listening for IPC");
            await Task.Delay(0);
            bool success = false;
            try
            {
                _svcHost = new ServiceHost(this, new Uri[] { new Uri(Constants.BasePipeUri) });
                _svcHost.AddServiceEndpoint(typeof(IConfigurator), new NetTcpBinding()
                {
                    HostNameComparisonMode = HostNameComparisonMode.Exact,
                    ReceiveTimeout = TimeSpan.MaxValue,
                    SendTimeout = TimeSpan.MaxValue,
                    CloseTimeout = TimeSpan.MaxValue,
                    OpenTimeout = TimeSpan.MaxValue,
                    MaxReceivedMessageSize = Int32.MaxValue
                }, Constants.ConfiguratorPipePath);
                _svcHost.Open();
                success = true;
            }
            catch (Exception e)
            {
                _logger.Log(e);
                if (_svcHost != null)
                {
                    _svcHost.Close();
                    _svcHost = null;
                }
            }
            return success;
        }

        public async Task StopListeningAsync()
        {
            _logger.Log("Engine stopped listening for IPC");
            await Task.Delay(0);
            try
            {
                if (_svcHost != null)
                {
                    _svcHost.Close();
                    _svcHost = null;
                }
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
        }
        #endregion

        #region IConfigurator
        public async Task<bool> TestCredentialsAsync(Credentials credentials)
        {
            _logger.Log(string.Format("TestCredentials: {0}/{1}", credentials.IP, credentials.Port));
            bool success = false;
            try
            {
                success = await _serenity.LoginAsync(credentials);
            }
            catch (Exception e)
            {
                _logger.Log("Exception in TestCredentialsAsync", e);
            }
            return success;
        }

        public async Task<List<string>> GetAllAgentIdsAsync()
        {
            _logger.Log("GetAllAgentIdsAsync");
            var agentIds = new List<string>();
            try
            {
                await Task.Run(() => agentIds = _cache.Cache.AgentIds ?? new List<string>());
            }
            catch (Exception e)
            {
                _logger.Log("Exception in GetAllAgentIdsAsync", e);
            }
            return agentIds;
        }

        public async Task<bool> RemoveAllAgentsAsync()
        {
            _logger.Log("RemoveAllAgentsAsync");
            bool success = false;
            try
            {
                await Task.Run(() => success = _agentMgr.RemoveAllAgents());
            }
            catch (Exception e)
            {
                _logger.Log("Exception in RemoveAllAgentsAsync", e);
            }
            return success;
        }

        public async Task<bool> SetCredentialsAsync(Credentials credentials)
        {
            _logger.Log(string.Format("SetCredentials: {0}/{1}", credentials.IP, credentials.Port));
            bool success = false;
            try
            {
                success = await _serenity.LoginAsync(credentials);
                success = success && _cache.SaveCredentials(credentials);
            }
            catch (Exception e)
            {
                _logger.Log("Exception in SetCredentialsAsync", e);
                success = false;
            }
            return success;
        }

        public async Task<bool> AddEventAgentAsync(string id)
        {
            _logger.Log(string.Format("AddEventAgentAsync: {0}", id));
            bool success = false;
            try
            {
                await Task.Run(() => success = _agentMgr.AddUpdateAgent(id));
            }
            catch (Exception e)
            {
                _logger.Log("Exception in AddEventAgentAsync", e);
            }
            return success;
        }

        public async Task<bool> AddSituationsAsync(string id, List<INewSituation> situations)
        {
            _logger.Log("AddSituationsAsync");
            bool success = false;
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IConfiguratorCallback>();
                success = await _agentMgr.InjectSituationsAsync(callback, id, situations);
                await _injector.UpdateSituations();
            }
            catch (Exception e)
            {
                _logger.Log("Exception in AddSituationsAsync", e);
            }
            return success;
        }

        public async Task<bool> RunAllAgentsAsync()
        {
            _logger.Log("RunAllAgentsAsync");
            bool success = false;
            try
            {
                await _injector.UpdateSituations();
                await Task.Run(() => success = _agentMgr.RunAllAgents());
            }
            catch (Exception e)
            {
                _logger.Log("Exception in RunAllAgentsAsync", e);
            }
            return success;
        }
        #endregion
    }
}
