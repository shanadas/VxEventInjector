using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using Pelco.Logging;
using Prism.Events;
using Prism.Regions;

namespace SampleAgent.ViewModels
{
    class BaseViewModel : BindableBase
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
    }
}
