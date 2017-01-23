using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Unity;
using Prism.Regions;
using System.Collections.Generic;
using VxEventInjector.PageStates;
using VxEventInjector.Views;
using VxEventInjectorCommon;
using VxEventInjectorCommon.Services.AgentLoader;

namespace VxEventInjector.ViewModels
{
    class ConfigPageViewModel : BaseViewModel, INavigationAware
    {
        public DelegateCommand LoadedCmd { get; private set; }
        public DelegateCommand PreviousCmd { get; private set; }
        public DelegateCommand NextCmd { get; private set; }
        public DelegateCommand StartCmd { get; private set; }
        public IPageState CurrentState { get; set; }
        public IPageState StateAgentSelector { get; private set; }
        public IPageState StateAgentConfig { get; private set; }
        public IPageState StateProcessAgents { get; private set; }
        public IPageState StateComplete { get; private set; }
        public List<StateEventAgent> SelectedAgents { get; set; }
        public List<string> RemoteAgentIds { get; set; }
        public Credentials LoginCredentials { get; private set; }
        private IAgentLoaderSvc _agentLoader;

        public ConfigPageViewModel(IAgentLoaderSvc agentLoader, IRegionManager regionMgr)
        {
            SelectedAgents = new List<StateEventAgent>();
            LoadedCmd = new DelegateCommand(Loaded);
            PreviousCmd = new DelegateCommand(Previous);
            NextCmd = new DelegateCommand(Next);
            StartCmd = new DelegateCommand(Start);
            _agentLoader = agentLoader;
            regionMgr.RegisterViewWithRegion(Properties.Resources.RegionConfigMain, () => Container.Resolve<AgentSelectorView>());
        }

        private void Loaded()
        {
            StateAgentSelector = Container.Resolve<AgentSelectorState>();
            StateAgentConfig = Container.Resolve<AgentConfigState>();
            StateProcessAgents = Container.Resolve<ProcessAgentsState>();
            CurrentState = StateAgentSelector;
            UpdateButtonStates();
        }

        public bool CanPrevious
        {
            get { return CurrentState == null ? false : CurrentState.CanPrevious; }
        }

        public bool CanNext
        {
            get { return CurrentState == null ? false : CurrentState.CanNext; }
        }

        public bool CanStart
        {
            get { return CurrentState == null ? false : CurrentState.CanStart; }
        }

        private void Previous()
        {
            CurrentState.Previous();
            UpdateButtonStates();
        }

        private void Next()
        {
            CurrentState.Next();
            UpdateButtonStates();
        }

        private void Start()
        {
            CurrentState.Start();
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            OnPropertyChanged("CanPrevious");
            OnPropertyChanged("CanNext");
            OnPropertyChanged("CanStart");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoginCredentials = navigationContext.Parameters["credentials"] as Credentials;
        }
    }
}
