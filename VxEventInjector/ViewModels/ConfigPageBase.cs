using Prism.Regions;
using System.Windows.Threading;
using VxEventInjector.PageStates;

namespace VxEventInjector.ViewModels
{
    class ConfigPageBase : BaseViewModel, INavigationAware
    {
        public string FromView { get; private set; }
        public IPageState FromState { get; private set; }
        protected Dispatcher MyDispatcher { get; private set; }
        protected bool NavigatedByNext { get; private set; }

        public ConfigPageBase()
        {
            MyDispatcher = Dispatcher.CurrentDispatcher;
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        { }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            bool? next = navigationContext.Parameters["next"] as bool?;
            NavigatedByNext = next.HasValue && next.Value;
            if (NavigatedByNext)
            {
                FromView = navigationContext.Parameters["fromView"] as string;
                FromState = navigationContext.Parameters["fromState"] as IPageState;
            }
        }
    }
}
