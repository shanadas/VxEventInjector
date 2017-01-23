using Microsoft.Practices.Unity;
using Pelco.Logging;
using Prism.Events;
using Prism.Regions;
using System;
using System.Linq;
using System.Windows.Controls;
using VxEventAgent;
using VxEventInjector.Events;
using VxEventInjector.ViewModels;

namespace VxEventInjector.PageStates
{
    abstract class PageStateBase : IPageState
    {
        [Dependency]
        public IEventAggregator EventAgg { get; set; }
        [Dependency]
        public IRegionManager RegionMgr { get; set; }
        [Dependency]
        public ConfigPageViewModel PageViewModel { get; set; }
        [Dependency]
        public ILogger Logger { get; set; }

        public PageStateBase()
        { }

        public virtual IRegion RegionConfigMain
        {
            get { return RegionMgr.Regions[Properties.Resources.RegionConfigMain]; }
        }

        public virtual bool CanPrevious
        {
            get { return false; }
        }

        public virtual bool CanNext 
        {
            get { return false; }
        }

        public virtual bool CanStart
        {
            get { return false; }
        }

        public virtual void Previous()
        { }

        public virtual void Next()
        { }

        public virtual void Start()
        { }

        public virtual void Close()
        {
            EventAgg.GetEvent<CloseEvent>().Publish(null);
        }

        protected StateEventAgent GetFirstEventAgent(IEventAgent after = null)
        { 
            StateEventAgent eventAgent = null;
            bool found = false;
            foreach (var agent in PageViewModel.SelectedAgents)
            {
                if (after == null)
                {
                    if (agent.RequiresControl)
                    {
                        eventAgent = agent;
                        break;
                    }
                }
                else
                {
                    if (found)
                    {
                        if (agent.RequiresControl)
                        {
                            eventAgent = agent;
                            break;
                        }
                    }
                    else
                    {
                        if (agent.Id == after.Id)
                            found = true;
                    }
                }
            }
            return eventAgent;
        }

        protected TViewModel GetViewModel<TView, TViewModel>()
            where TView : UserControl
            where TViewModel : class
        {
            TViewModel viewModel = null;
            try
            {
                var view = RegionConfigMain.Views.FirstOrDefault(val => val is TView) as TView;
                viewModel = view.DataContext as TViewModel;
            }
            catch (Exception e)
            {
                Logger.Log("Failed to properly display plugin", e);
            }
            return viewModel;
        }
    }
}
