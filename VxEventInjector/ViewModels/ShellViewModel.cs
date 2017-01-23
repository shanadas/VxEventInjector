using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System.Threading.Tasks;
using VxEventInjector.Views;
using VxEventInjectorCommon.Services.AgentLoader;

namespace VxEventInjector.ViewModels
{
    class ShellViewModel : BaseViewModel
    {
        public DelegateCommand LoadedCmd { get; private set; }

        public ShellViewModel(IRegionManager regionMgr)
        {
            LoadedCmd = new DelegateCommand(Loaded);
            regionMgr.RegisterViewWithRegion(Properties.Resources.RegionShell, () => Container.Resolve<VxLoginView>());
        }

        private void Loaded()
        {
            // First and only time to call this
            Task.Run(() => Container.Resolve<IAgentLoaderSvc>().LoadAllAgents());
        }
    }
}
