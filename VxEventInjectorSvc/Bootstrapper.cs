using Microsoft.Practices.Unity;
using Pelco.Logging;
using Prism.Logging;
using Prism.Unity;
using System;
using System.IO;
using System.Windows;
using VxEventInjectorCommon;
using VxEventInjectorCommon.Services.AgentLoader;
using VxEventInjectorSvc.Services.AgentMgr;
using VxEventInjectorSvc.Services.Cache;
using VxEventInjectorSvc.Services.Configurator;
using VxEventInjectorSvc.Services.EventInjector;
using VxEventInjectorSvc.Services.Msmq;
using VxEventInjectorSvc.Services.Serenity;
using VxEventInjectorSvc.Services.Server;

namespace VxEventInjectorSvc
{
    class Bootstrapper : UnityBootstrapper
    {
        private static readonly string _LogFileName = Path.Combine(Constants.LogDir, "VxEventInjectorSvc.txt");
        private ILogger _logger;

        public Bootstrapper()
        { }

        protected override ILoggerFacade CreateLogger()
        {
            _logger = new FileLogger(_LogFileName);
            return _logger;
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterInstance<ILogger>(_logger);
            Container.RegisterType<ICacheSvc, CacheSvc>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IConfiguratorSvc, ConfiguratorSvc>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAgentLoaderSvc, AgentLoaderSvc>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAgentMgrSvc, AgentMgrSvc>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISerenitySvc, SerenitySvc>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMsmqSvc, MsmqSvc>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IEventInjector, EventInjector>(new ContainerControlledLifetimeManager());
        }

        protected override DependencyObject CreateShell()
        {
            return null;
        }

        public async void StartAsync()
        {
            // Add the event handler for handling non-UI thread exceptions to the event.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                // Sometimes this service starts before networking is initialized, so wait.
                _logger.Log("Waiting for network to initialize");
                while (Utils.IPv4 == null)
                    System.Threading.Thread.Sleep(100);
                _logger.Log("Network has initialized");

                // Load up all the agents from the Agents directory
                _logger.Log("Loading all event agents from the 'Agents' directory");
                Container.Resolve<IAgentLoaderSvc>().LoadAllAgents();
                int agents = Container.Resolve<IAgentLoaderSvc>().Agents.Count;

                // Start and register the server service which listens for new events
                _logger.Log("Registering the service service");
                Container.RegisterInstance(Container.Resolve<ServerSvc>());

                // Load up cached event agents
                _logger.Log("Loading cached event agents");
                var agentMgr = Container.Resolve<IAgentMgrSvc>();
                await agentMgr.LoadCachedAgentsAsync();

                // Run all agents we currently have (cached only so far)
                _logger.Log("Run all event agents");
                agentMgr.RunAllAgents();

                // Start listening for IPC messages from the EventInjector client app
                _logger.Log("Start listening for IPC messages from the configurator");
                await Container.Resolve<IConfiguratorSvc>().StartListeningAsync();

                // Start event injector event loop
                _logger.Log("Start the event injector loop");
                Container.Resolve<IEventInjector>().StartLoop();

                // Indicate we've started
                _logger.Log("Engine Started");
            }
            catch (Exception e)
            {
                _logger.Log("Threw while starting up the system", e);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = new Exception($"There was a unhandled exception. Terminating: {e.IsTerminating}", e.ExceptionObject as Exception);
            _logger.Log("There was an unhandled exception!", ex);
        }

        public async void Stop()
        {
            try
            {
                _logger.Log("Stopping service");

                // Stop listening for IPC messages
                _logger.Log("Stoping IPC");
                await Container.Resolve<IConfiguratorSvc>().StopListeningAsync();

                // Stop all agents from running
                _logger.Log("Stopping all running agents");
                var agentMgr = Container.Resolve<IAgentMgrSvc>();
                agentMgr.StopAllAgents();

                // Stop the event injection loop
                _logger.Log("Stopping event injection loop");
                Container.Resolve<IEventInjector>().StopLoop();

                // Indicate we've stopped all services
                _logger.Log("Engine Stopped");
            }
            catch (Exception e)
            {
                _logger.Log("Threw while stopping the system", e);
            }
        }
    }
}
