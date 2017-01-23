using Prism.Regions;
using System.Linq;
using System.Windows;
using VxEventInjector.ViewModels;
using VxEventInjector.Views;

namespace VxEventInjector.PageStates
{
    class AgentSelectorState : PageStateBase
    {
        public AgentSelectorState()
        { }

        public override bool CanNext
        {
            get { return true; }
        }

        public override void Next()
        {
            var viewModel = GetViewModel<AgentSelectorView, AgentSelectorViewModel>();
            PageViewModel.RemoteAgentIds = viewModel.RemoteAgentIds;
            PageViewModel.SelectedAgents = viewModel.SelectedAgents.Select(val =>
            {
                return new StateEventAgent(val) { IsRemotelyConfigured = viewModel.RemoteAgentIds.Contains(val.Id) };
            }).ToList();
            PageViewModel.SelectedAgents.Sort((val, val2) => val.Id.CompareTo(val2.Id));

            if (PageViewModel.SelectedAgents.Count == 0)
            {
                MessageBox.Show(App.Current.MainWindow, Properties.Resources.MustSelectEventAgent, Properties.Resources.Alert);
                return;
            }

            var agentToConfig =  GetFirstEventAgent();
            var np = new NavigationParameters();
            np.Add("fromView", Properties.Resources.ViewAgentSelector);
            np.Add("fromState", PageViewModel.StateAgentSelector);
            np.Add("next", true);

            if(agentToConfig != null)
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
