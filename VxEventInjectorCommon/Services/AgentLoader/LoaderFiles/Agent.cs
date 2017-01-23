// Copyright (c) 2013 Ivan Krivyakov

using Microsoft.Practices.Unity;
using Pelco.AgentHosting;
using Pelco.Logging;
using System;
using System.AddIn.Pipeline;
using System.IO;
using System.Windows;
using VxEventAgent;

namespace VxEventInjectorCommon.Services.AgentLoader
{
    public class Agent : IServiceProvider, IDisposable
    {
        public event EventHandler<AgentErrorEventArgs> Error;
        public AgentCatalogEntry CatalogEntry { get; private set; }
        public FrameworkElement View { get; private set; }
        public string Title { get; private set; }
        private IUnityContainer _childContainer;
        private ILogger _logger;
        private AgentProcessProxy _remoteProcess;
        private AgentStartupInfo _startupInfo;
        private bool _isDisposing;
        private bool _fatalErrorOccurred;

        public Agent(IUnityContainer container, ILogger logger)
        {
            _logger = logger;
            _childContainer = container.CreateChildContainer();
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            if (_remoteProcess != null)
                service = _remoteProcess.RemoteAgent.GetService(serviceType);
            return service;
        }

        // Can be executed on any thread
        public void Load(AgentCatalogEntry catalogEntry)
        {
            if (CatalogEntry != null)
                throw new InvalidOperationException("Agent can be loaded only once");

            CatalogEntry = catalogEntry;
            Title = catalogEntry.Name;

            Initialize();

            _logger.Log($"Loading {catalogEntry.Bits} bit - {CatalogEntry.Name} from {CatalogEntry.AssemblyPath}, {CatalogEntry.MainClass}");

            var host = _childContainer.Resolve<AgentViewOfHost>();
            host.FatalError += OnFatalError;

            _remoteProcess = _childContainer.Resolve<AgentProcessProxy>();

            _remoteProcess.Start();
            new ProcessMonitor(OnProcessExited).Start(_remoteProcess.Process);

            _remoteProcess.LoadAgent();
        }

        // Must execute on UI thread
        public void CreateView()
        {
            _logger.Log("Creating agent view");
            try
            {
                View = FrameworkElementAdapters.ContractToViewAdapter(_remoteProcess.RemoteAgent.Contract);
                _logger.Log("Agent view created");
            }
            catch(Exception e)
            {
                _logger.Log("Failed to create the event agent's control", e);
            }
        }

        public void Dispose()
        {
            _isDisposing = true;
            try
            {
                var disposableView = View as IDisposable;
                if (disposableView != null) disposableView.Dispose();
            }
            catch (Exception ex)
            {
                ReportError("Error when disposing view", ex);
            }
            _childContainer.Dispose();
        }

        private void Initialize()
        {
            _childContainer.RegisterType<IHost, AgentViewOfHost>(new ContainerControlledLifetimeManager());
            _childContainer.RegisterType<AgentProcessProxy>(new ContainerControlledLifetimeManager());

            _startupInfo = new AgentStartupInfo
            {
                FullAssemblyPath = CatalogEntry.AssemblyPath,
                AssemblyName = Path.GetFileNameWithoutExtension(CatalogEntry.AssemblyPath),
                Bits = CatalogEntry.Bits,
                MainClass = CatalogEntry.MainClass,
                Name = CatalogEntry.Name,
                Parameters = CatalogEntry.Parameters,
                CreateControl = CatalogEntry.CreateControl
            };

            _childContainer.RegisterInstance(_startupInfo);
        }

        private void OnProcessExited()
        {
            if (!_isDisposing && !_fatalErrorOccurred )
                ReportError("Agent process terminated unexpectedly", null);
        }

        private void OnFatalError(Exception ex)
        {
            _fatalErrorOccurred = true;
            ReportError(null, ex);
        }

        private void ReportError(string message, Exception ex)
        {
            _logger.Log("Reporting Error:", ex);
            Error?.Invoke(this, new AgentErrorEventArgs(this, message, ex));
        }
    }
}
