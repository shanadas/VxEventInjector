using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using Pelco.Logging;
using Prism.Events;
using Prism.Regions;

namespace StressAgent.ViewModels
{
    class BaseViewModel : BindableBase, INavigationAware
    {
        [Dependency]
        public ILogger Logger { get; set; }

        [Dependency]
        public IRegionManager RegionMgr { get; set; }

        [Dependency]
        public IEventAggregator EventAgg { get; set; }

        [Dependency]
        public IUnityContainer Container { get; set; }

        protected bool LogIfNotEmpty(string msg)
        {
            bool logged = false;
            if (!string.IsNullOrWhiteSpace(msg))
            {
                Logger.Log(msg);
                logged = true;
            }
            return logged;
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        { }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        { }
    }
}
