using Microsoft.Practices.Prism.Commands;
using Prism.Regions;
using StressAgent.Models;
using StressAgent.Services;
using System.Collections.Generic;
using System.IO;

namespace StressAgent.ViewModels
{
    class PerformanceConfigViewModel : BaseViewModel
    {
        public DelegateCommand DoneButtonCmd { get; private set; }
        public PerformanceConfigModel PerfConfig { get; private set; }
        private IEventAgentSvc _eventAgentSvc;
        private SituationModel _defaultSituation;

        public PerformanceConfigViewModel(IEventAgentSvc eventAgentSvc)
        {
            DoneButtonCmd = new DelegateCommand(DoneButton);
            PerfConfig = new PerformanceConfigModel();
            _eventAgentSvc = eventAgentSvc;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            _defaultSituation = navigationContext.Parameters[Properties.Resources.DefaultSituation] as SituationModel;
        }

        private void DoneButton()
        {
            for (uint i = 0; i < PerfConfig.NumSituations; i++)
            {
                var resources = new List<KeyValuePair<VxEventAgent.ResourceType,string>>();
                if(!string.IsNullOrWhiteSpace(_defaultSituation.SourceDeviceId))
                    resources.Add(new KeyValuePair<VxEventAgent.ResourceType, string>(VxEventAgent.ResourceType.Device, _defaultSituation.SourceDeviceId));

                _eventAgentSvc.Situations.Add(new VxEventAgent.NewSituation() 
                { 
                    AckNeeded = _defaultSituation.AckNeeded,
                    Audible = _defaultSituation.Audible,
                    AutoAcknowledgeTimeout = _defaultSituation.AutoAcknowledgeTimeout,
                    Log = _defaultSituation.Log,
                    Notify = _defaultSituation.Notify,
                    Severity = _defaultSituation.Severity,
                    SnoozeIntervals = _defaultSituation.SnoozeIntervals,
                    Type = string.Format("{0}_{1}", _defaultSituation.Type, i),
                    Resources = resources,
                    SourceDeviceId = _defaultSituation.SourceDeviceId
                });
            }

            var performanceConfigFile = Path.Combine(_eventAgentSvc.DocumentsDir, Properties.Resources.PerformanceConfigFilename);
            File.WriteAllText(performanceConfigFile, string.Format("{0},{1},{2},{3}", PerfConfig.NumSituations, PerfConfig.NumEvents, _defaultSituation.SourceDeviceId, _defaultSituation.Type));
            RegionMgr.RequestNavigate(Properties.Resources.RegionShellCtrl, Properties.Resources.ViewDoneView);
        }
    }
}
