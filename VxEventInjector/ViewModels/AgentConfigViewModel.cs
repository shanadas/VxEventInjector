using Prism.Regions;
using System.Collections.Generic;
using System.Linq;

namespace VxEventInjector.ViewModels
{
    class AgentConfigViewModel : ConfigPageBase
    {
        public StateEventAgent ActiveEventAgent { get; private set; }
        public Stack<StateEventAgent> AgentStack { get; private set; }

        public AgentConfigViewModel()
        {
            AgentStack = new Stack<StateEventAgent>();
        }

        public bool IsRemotelyConfigured
        {
            get { return ActiveEventAgent.IsRemotelyConfigured; }
        }

        public bool KeepConfiguration
        {
            get { return ActiveEventAgent.KeepConfiguration; }
            set { ActiveEventAgent.KeepConfiguration = value; OnPropertyChanged("KeepConfiguration"); }
        }

        public string EventAgentName
        {
            get { return ActiveEventAgent.Name; }
        }

        private IRegion AgentRegion
        {
            get { return RegionMgr.Regions.First(var => var.Name == Properties.Resources.RegionAgentConfig); }
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // If navigating from to go forward in the wizard the push
            if (navigationContext.Parameters.Count() != 0)
                AgentStack.Push(ActiveEventAgent);
            var agentCtrlView = AgentRegion.Views.FirstOrDefault();
            AgentRegion.Deactivate(AgentRegion.GetView(ActiveEventAgent.Id));
            base.OnNavigatedFrom(navigationContext);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            ActiveEventAgent = navigationContext.Parameters["agent"] as StateEventAgent;

            if (ActiveEventAgent == null)
                ActiveEventAgent = AgentStack.Pop();

            var view = AgentRegion.GetView(ActiveEventAgent.Id);
            if (view == null)
            {
                var control = ActiveEventAgent.CreateControl();
                AgentRegion.Add(control, ActiveEventAgent.Id);
            }
            else
                AgentRegion.Activate(view);

            OnPropertyChanged("IsRemotelyConfigured");
            OnPropertyChanged("KeepConfiguration");
            OnPropertyChanged("EventAgentName");
        }
    }
}
