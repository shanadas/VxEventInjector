using Microsoft.Practices.Unity;
using Prism.Regions;
using StressAgent.Views;

namespace StressAgent.ViewModels
{
    class ShellCtrlViewModel : BaseViewModel
    {
        public ShellCtrlViewModel(IRegionManager regionMgr)
        {
            regionMgr.RegisterViewWithRegion(Properties.Resources.RegionShellCtrl, () => Container.Resolve<DefaultSituationView>());
        }
    }
}
