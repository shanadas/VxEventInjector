using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VxEventAgent;
using VxEventInjector.Collections;
using VxEventInjector.Services;
using VxEventInjectorCommon;
using VxEventInjectorCommon.Extensions;

namespace VxEventInjector.ViewModels
{
    class ProcessAgentsViewModel : ConfigPageBase, IConfiguratorCallback
    {
        public DelegateCommand StartCmd { get; private set; }
        public LogMsgCollection MessageCollection { get; private set; }
        private string _agentName;
        private double _totalProgress;
        private double _agentProgress;
        private List<StateEventAgent> _agents;
        private Credentials _loginCredentials;

        public ProcessAgentsViewModel()
        {
            MessageCollection = new LogMsgCollection();
            StartCmd = new DelegateCommand(Start);
        }

        public string AgentName
        {
            get { return _agentName; }
            private set { SetProperty(ref _agentName, value); }
        }

        public double TotalProgress
        {
            get { return _totalProgress; }
            private set { SetProperty(ref _totalProgress, value); }
        }

        public double AgentProgress
        {
            get { return _agentProgress; }
            private set { SetProperty(ref _agentProgress, value); }
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            MessageCollection.Clear();
            base.OnNavigatedFrom(navigationContext);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            _agents = navigationContext.Parameters["agents"] as List<StateEventAgent>;
            _loginCredentials = navigationContext.Parameters["credentials"] as Credentials;
            foreach (var agent in _agents)
                MessageCollection.Add(new LogMsg(agent.Name, LogMsg.BrushBlue));
        }

        private void Start()
        {
            Task.Run(() => StartAsync());
        }

        private async void StartAsync()
        {
            int totalAgents = _agents.Count;
            int agentsProcessed = 0;

            // We act like there is 1 more agent than their is, because we want a little progress bar room at the end
            // to account for the time necessary to run all of the agents
            var getPercent = new Func<double, double>(soFar => 1.0 * soFar / (totalAgents + 1) * 100);
            TotalProgress = 2.0;
            try
            {
                using (var ipc = new IPCComSvc(this))
                {
                    await ipc.Configurator.SetCredentialsAsync(_loginCredentials);

                    AgentName = "Removing Existing Event Agents";
                    await ipc.Configurator.RemoveAllAgentsAsync();

                    foreach (var agent in _agents)
                    {
                        AgentName = agent.Name;
                        await ipc.Configurator.AddEventAgentAsync(agent.Id);

                        if (!agent.KeepConfiguration)
                        {
                            var sanitizedSituations = new List<INewSituation>();
                            if (agent.Situations != null)
                                sanitizedSituations = agent.Situations.Select(val => val.ToSanitizedNewSituation() as INewSituation).ToList();
                            await ipc.Configurator.AddSituationsAsync(agent.Id, sanitizedSituations);
                        }
                        else
                        {
                            var logMsg = new LogMsg(string.Format("Skipping pre-configured event agent ({0})", agent.Name), LogMsg.BrushOrange);
                            MyDispatcher.BeginInvoke(new Action(() => MessageCollection.Add(logMsg))).Wait();
                            AgentProgress = 100.0;
                        }
                        TotalProgress = getPercent(++agentsProcessed);
                    }

                    AgentName = "Starting All Event Agents";
                    await ipc.Configurator.RunAllAgentsAsync();

                    TotalProgress = getPercent(++agentsProcessed);
                    AgentName = "Complete";
                }
            }
            catch (System.ServiceModel.CommunicationObjectFaultedException e)
            {
                Logger.Log(Properties.Resources.InjectorSvcNotStartedMsg, e);
                var logMsg = new LogMsg(Properties.Resources.InjectorSvcNotStartedMsg, LogMsg.BrushRed);
                MyDispatcher.BeginInvoke(new Action(() => MessageCollection.Add(logMsg))).Wait();
            }
            catch (Exception e)
            {
                Logger.Log("Fatal error occurred while processing agents", e);
                var logMsg = new LogMsg(string.Format("Fatal error has occurred: ", e.Message), LogMsg.BrushRed);
                MyDispatcher.BeginInvoke(new Action(() => MessageCollection.Add(logMsg))).Wait();
            }
        }

        public void OnSituation(double percent, string message)
        {
            if (!MyDispatcher.CheckAccess())
            {
                MyDispatcher.BeginInvoke(new Action<double, string>(OnSituation), new object[] { percent, message });
                return;
            }

            var logMsg = new LogMsg(message);
            if (message.Contains(Constants.Success))
                logMsg.Brush = LogMsg.BrushGreen;
            else if (message.Contains(Constants.Failure))
                logMsg.Brush = LogMsg.BrushRed;

            MessageCollection.Add(logMsg);
            AgentProgress = percent;
        }
    }
}
