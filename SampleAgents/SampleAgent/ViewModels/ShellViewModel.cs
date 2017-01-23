using Microsoft.Practices.Unity;
using SampleAgent.Views;

namespace SampleAgent.ViewModels
{
    class ShellViewModel : BaseViewModel
    {
        public ShellViewModel()
        {
            RegionMgr.RegisterViewWithRegion(Properties.Resources.RegionShell, () => Container.Resolve<ShellCtrlView>());
        }
    }
}
