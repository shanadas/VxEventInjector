using Microsoft.Practices.Unity;
using StressAgent.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VxEventAgent;

namespace StressAgent.Services
{
    class EventAgentSvc : IEventAgentSvc
    {
        public bool IsRunning { get; private set; }
        public List<NewSituation> Situations { get; set; }
        public IHost Host { get; set; }
        public string DocumentsDir { get; set; }
        private IUnityContainer _container;

        public EventAgentSvc(IUnityContainer container)
        {
            Situations = new List<NewSituation>();
            _container = container;
        }

        public string Id
        {
            get { return "CE5FFB33-7710-4258-8C59-E7BE4859E6FC"; }
        }

        public string Name
        {
            get { return "Stress Agent"; }
        }

        public string Version
        {
            get { return typeof(EventAgentSvc).Assembly.GetName().Version.ToString(); }
        }

        public string Manufacturer
        {
            get { return "Pelco"; }
        }

        public string Author
        {
            get { return "Pelco"; }
        }

        public string Description
        {
            get { return "Event agent to stress test the Event Injector"; }
        }

        public bool IsConfigured
        {
            get 
            {
                bool isConfigured = true;
                isConfigured = isConfigured && Situations.Count > 0;
                return isConfigured;
            }
        }

        public bool RequiresControl
        {
            get { return true; }
        }

        public FrameworkElement CreateControl()
        {
            return _container.Resolve<ShellCtrlView>();
        }

        public bool Run()
        {
            IsRunning = true;
            Task.Run(() => RunAsync());
            return true;
        }

        public void RunAsync()
        {
            var performanceConfigFile = Path.Combine(DocumentsDir, Properties.Resources.PerformanceConfigFilename);

            //string.Format("{0},{1},{2},{3}", PerfConfig.NumSituations, PerfConfig.NumEvents, _defaultSituation.DeviceId, _defaultSituation.Type);
            var performanceValues = File.ReadAllText(performanceConfigFile).Split(',');
            int situations = int.Parse(performanceValues[0]);
            int events = int.Parse(performanceValues[1]);
            string deviceId = performanceValues[2];
            string sitType = performanceValues[3];
            var server = Host.GetService(typeof(IServer)) as IServer;

            var rand = new Random();
            for (int i = 0; i < events; i++)
            {
                int sitNum = rand.Next(0, situations);
                string type = string.Format("{0}_{1}", sitType, sitNum);
                var newEvent = new NewEvent()
                {
                    Properties = new Dictionary<string,string>() {{"PropertyKey", "PropertyValue"}, {"PropertyKey2", "PropertyValue2"}},
                    SituationType = type,
                    SourceDeviceId = deviceId,
                    SourceUsername = null,
                    Time = DateTime.UtcNow
                };
                server.InjectEvent(newEvent);
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public object GetService(Type serviceType)
        { 
            return null; 
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        { }
    }
}
