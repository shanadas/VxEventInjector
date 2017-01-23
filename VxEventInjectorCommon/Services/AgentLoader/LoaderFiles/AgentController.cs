// Copyright (c) 2013 Ivan Krivyakov

using Microsoft.Practices.Unity;
using Pelco.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace VxEventInjectorCommon.Services.AgentLoader
{
    public class AgentController : IDisposable
    {
        public ObservableCollection<Agent> LoadedAgents { get; private set; }
        private IUnityContainer _container;
        private ILogger _logger;

        public AgentController(IUnityContainer container, ILogger logger)
        {
            _container = container;
            _logger = logger;
            LoadedAgents = new ObservableCollection<Agent>();
        }

        public Task<Agent> LoadAgentAsync(AgentCatalogEntry info)
        {
            return Task.Run(() => LoadAgent(info))
                .ContinueWith(loadAgentTask =>
                {
                    Agent agent = null;
                    if (loadAgentTask.Status != TaskStatus.Faulted)
                    {
                        agent = loadAgentTask.Result;
                        if (agent != null)
                            LoadedAgents.Add(agent);
                    }
                    return agent;
                });
        }

        public void Dispose()
        {
            foreach (var agent in LoadedAgents)
            {
                DisposeAgent(agent);
            }
        }

        public void RemoveAgent(Agent agent)
        {
            LoadedAgents.Remove(agent);
            DisposeAgent(agent);
        }

        private void DisposeAgent(Agent agent)
        {
            if (agent == null) return;
            agent.Error -= OnAgentError;

            try
            {
                agent.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("Error disposing agent {0}", agent.Title), ex);
            }
        }

        private Agent LoadAgent(AgentCatalogEntry info)
        {
            var agent = _container.Resolve<Agent>();
            agent.Error += OnAgentError;

            try
            {
                agent.Load(info);
            }
            catch (Exception)
            {
                DisposeAgent(agent);
                throw;
            }

            return agent;
        }

        private void OnAgentError(object sender, AgentErrorEventArgs args)
        {
            var task = new Task(() => AgentErrorHandler(args));
            task.Start();
        }

        private void AgentErrorHandler(AgentErrorEventArgs args)
        {
            if (args == null) return;
            if (args.Agent == null) return;

            string title = args.Agent.Title;
            string message = string.Format("An error occurred in agent {0}. The agent tab will be now closed.\r\n{1}\r\n", title, args.Message);

            if (LoadedAgents.Contains(args.Agent))
            {
                _logger.Log(message, args.Exception);
                LoadedAgents.Remove(args.Agent);
            }
            else
            {
                _logger.Log(message, args.Exception);
            }

            DisposeAgent(args.Agent);
        }
    }
}
