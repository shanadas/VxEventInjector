using Microsoft.Practices.Unity;
using Pelco.Logging;
using Prism.Unity;
using StressAgent.Services;
using StressAgent.Views;
using System.IO;
using System.Windows;

namespace StressAgent
{
    class Bootstrapper : UnityBootstrapper
    {
        private bool _isPlugin;
        public string _documentsDir;

        public Bootstrapper(bool isPlugin = false, string documentsDir = "")
        {
            _isPlugin = isPlugin;
            _documentsDir = documentsDir;
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            string logPath = Path.Combine(_documentsDir, Properties.Resources.Logs, Properties.Resources.LogFilename);
            Container.RegisterInstance<ILogger>(new FileLogger(logPath));
            Container.RegisterType<IEventAgentSvc, EventAgentSvc>(new ContainerControlledLifetimeManager());
            Container.RegisterType<object, DefaultSituationView>(Properties.Resources.ViewDefaultSituationsView);
            Container.RegisterType<object, PerformanceConfigView>(Properties.Resources.ViewPerformanceConfigView);
            Container.RegisterType<object, DoneView>(Properties.Resources.ViewDoneView);
        }

        protected override DependencyObject CreateShell()
        {
            var view = _isPlugin ? null : (DependencyObject)Container.Resolve<Shell>();
            return view;
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            if (!_isPlugin)
            {
                App.Current.MainWindow = (Window)Shell;
                App.Current.MainWindow.Show();
            }
        }
    }
}
