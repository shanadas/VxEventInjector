using Microsoft.Practices.Unity;
using Pelco.Logging;
using Prism.Logging;
using Prism.Unity;
using System.IO;
using System.Windows;
using VxEventInjector.ViewModels;
using VxEventInjector.Views;
using VxEventInjectorCommon;
using VxEventInjectorCommon.Services.AgentLoader;

namespace VxEventInjector
{
    class Bootstrapper : UnityBootstrapper
    {
        private static readonly string _LogFileName = Path.Combine(Constants.LogDir, "VxEventInjector.txt");
        private ILogger _logger;

        protected override ILoggerFacade CreateLogger()
        {
            _logger = new FileLogger(_LogFileName);
            return _logger;
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterInstance<ILogger>(_logger);
            Container.RegisterType<IAgentLoaderSvc, AgentLoaderSvc>(new ContainerControlledLifetimeManager());
            Container.RegisterType<object, ConfigPageViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<object, VxLoginView>(Properties.Resources.ViewVxLogin);
            Container.RegisterType<object, ConfigPageView>(Properties.Resources.ViewConfigPage);
            Container.RegisterType<object, AgentSelectorView>(Properties.Resources.ViewAgentSelector);
            Container.RegisterType<object, AgentConfigView>(Properties.Resources.ViewAgentConfig);
            Container.RegisterType<object, ProcessAgentsView>(Properties.Resources.ViewProcessAgents);
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            App.Current.MainWindow = (Window)Shell;
            App.Current.MainWindow.Show();
        }
    }
}
