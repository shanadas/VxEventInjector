using Microsoft.Practices.Prism.Commands;
using Prism.Regions;
using StressAgent.Models;

namespace StressAgent.ViewModels
{
    class DefaultSituationViewModel : BaseViewModel
    {
        public DelegateCommand NextButtonCmd { get; private set; }
        public SituationModel DefaultSituation { get; private set; }

        public DefaultSituationViewModel()
        {
            NextButtonCmd = new DelegateCommand(NextButton);
            DefaultSituation = new SituationModel();
        }

        public string DefaultSituationType
        {
            set
            {
                DefaultSituation.Type = string.Format("external/stress_agent/{0}", value);
            }
        }

        private void NextButton()
        {
            var np = new NavigationParameters();
            np.Add(Properties.Resources.DefaultSituation, DefaultSituation);
            RegionMgr.RequestNavigate(Properties.Resources.RegionShellCtrl, Properties.Resources.ViewPerformanceConfigView, np);
        }
    }
}
