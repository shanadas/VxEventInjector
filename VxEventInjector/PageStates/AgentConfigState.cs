using Prism.Regions;
using System.Windows;
using VxEventInjector.ViewModels;
using VxEventInjector.Views;

namespace VxEventInjector.PageStates
{
    class AgentConfigState : PageStateBase
    {
        public AgentConfigState()
        { }

        public override bool CanPrevious
        {
            get { return true; }
        }

        public override void Previous()
        {
            var viewModel = GetViewModel<AgentConfigView, AgentConfigViewModel>();
            if (viewModel != null && viewModel.AgentStack.Count > 0)
            {
                PageViewModel.CurrentState = viewModel.FromState;
                RegionMgr.RequestNavigate(Properties.Resources.RegionConfigMain, viewModel.FromView);
            }
            else
            {
                PageViewModel.CurrentState = PageViewModel.StateAgentSelector;
                RegionMgr.RequestNavigate(Properties.Resources.RegionConfigMain, Properties.Resources.ViewAgentSelector);
            }
        }

        public override bool CanNext
        {
            get { return true; }
        }

        public override void Next()
        {
            var viewModel = GetViewModel<AgentConfigView, AgentConfigViewModel>();
            if (viewModel == null) return;

            var activeEventAgent = viewModel.ActiveEventAgent;

            if (!activeEventAgent.IsConfigured)
            {
                MessageBox.Show(App.Current.MainWindow, Properties.Resources.ConfigureAgentBeforeProceeding, Properties.Resources.Alert);
                return;
            }

            var agentToConfig = GetFirstEventAgent(activeEventAgent);
            var np = new NavigationParameters();
            np.Add("fromView", Properties.Resources.ViewAgentConfig);
            np.Add("fromState", PageViewModel.StateAgentConfig);
            np.Add("next", true);

            if (agentToConfig != null)
            {
                np.Add("agent", agentToConfig);
                PageViewModel.CurrentState = PageViewModel.StateAgentConfig;
                RegionMgr.RequestNavigate(Properties.Resources.RegionConfigMain, Properties.Resources.ViewAgentConfig, np);
            }
            else
            {
                np.Add("agents", PageViewModel.SelectedAgents);
                np.Add("credentials", PageViewModel.LoginCredentials);
                PageViewModel.CurrentState = PageViewModel.StateProcessAgents;
                RegionMgr.RequestNavigate(Properties.Resources.RegionConfigMain, Properties.Resources.ViewProcessAgents, np);
            }
        }
    }
}
