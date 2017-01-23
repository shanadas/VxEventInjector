using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using VxEventAgent;
using VxEventInjector.Services;
using VxEventInjectorCommon.Services.AgentLoader;

namespace VxEventInjector.ViewModels
{
    class AgentSelectorViewModel : ConfigPageBase
    {
        public DelegateCommand<string> CheckedCmd { get; private set; }
        public DelegateCommand<string> UncheckedCmd { get; private set; }
        public DelegateCommand LoadedCmd { get; private set; }
        public List<string> RemoteAgentIds { get; private set; }
        private IAgentLoaderSvc _agentLoader;
        private Dictionary<string, IEventAgent> _selectedAgents;
        private List<IEventAgent> _allAgents;

        public AgentSelectorViewModel(IAgentLoaderSvc agentLoader)
        {
            _agentLoader = agentLoader;
            _selectedAgents = new Dictionary<string,IEventAgent>();
            CheckedCmd = new DelegateCommand<string>(Checked);
            UncheckedCmd = new DelegateCommand<string>(Unchecked);
            LoadedCmd = new DelegateCommand(Loaded);
            AllAgents = _agentLoader.Agents;
        }

        public List<IEventAgent> AllAgents
        {
            get { return _allAgents; }
            private set { SetProperty(ref _allAgents, value); }
        }

        public List<IEventAgent> SelectedAgents
        {
            get
            {
                return _selectedAgents.Values.ToList();
            }
        }

        private void Checked(string id)
        {
            if (!_selectedAgents.ContainsKey(id))
                _selectedAgents.Add(id, AllAgents.FirstOrDefault(val => val.Id == id));
        }

        private void Unchecked(string id)
        {
            if (_selectedAgents.ContainsKey(id))
                _selectedAgents.Remove(id);
        }

        public async void Loaded()
        {
            try
            {
                using (var ipc = new IPCComSvc())
                {
                    RemoteAgentIds = await ipc.Configurator.GetAllAgentIdsAsync();
                }
            }
            catch (System.ServiceModel.CommunicationObjectFaultedException e)
            {
                Logger.Log(Properties.Resources.InjectorSvcNotStartedMsg, e);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, e);
            }
        }
    }
}
