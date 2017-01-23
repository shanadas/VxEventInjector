// Copyright (c) 2013 Ivan Krivyakov

using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Threading;
using VxEventAgent;

namespace Pelco.AgentHosting
{
    /// <summary>
    /// Loads agents for the host
    /// </summary>
    public class AgentLoader : MarshalByRefObject, IAgentLoader
    {
        private Dispatcher _dispatcher;
        private IHost _host;
        private string _name;

        public void Run(string name)
        {
            _name = name;
            _dispatcher = Dispatcher.CurrentDispatcher;

            try
            {
                new AssemblyResolver().Setup();
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                IpcServices.RegisterChannel(name);
                RegisterObject();
                SignalReady();
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                ReportFatalError(ex);
            }

            // Allow any pending remoting operations to finish
            Thread.Sleep(100);
        }

        public IRemoteAgent LoadAgent(IHost host, AgentStartupInfo startupInfo)
        {
            _host = host;

            new ProcessMonitor(Dispose).Start(_host.HostProcessId);

            Func<AgentStartupInfo, object> createOnUiThread = LoadAgentOnUiThread;
            var result = _dispatcher.Invoke(createOnUiThread, startupInfo);

            if (result is Exception)
                throw new TargetInvocationException((Exception)result);
            return (IRemoteAgent)result;
        }

        public void Dispose()
        {
            if (_dispatcher != null)
                _dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            else
                Environment.Exit(1);
        }

        // Live forever
        public override object InitializeLifetimeService()
        {
            return null;
        }

        private object LoadAgentOnUiThread(AgentStartupInfo startupInfo)
        {
            var assembly = startupInfo.AssemblyName;
            var mainClass = startupInfo.MainClass;

            try
            {
                var obj = AgentCreator.CreateAgent(startupInfo.AssemblyName, startupInfo.MainClass, _host);
                var localAgent = obj as IEventAgent;

                if (localAgent == null)
                {
                    var message = string.Format("Object of type {0} cannot be loaded as agent " +
                        "because it does not implement IEventAgent interface", mainClass);
                    throw new InvalidOperationException(message);
                }

                var remoteAgent = new RemoteAgent(localAgent, startupInfo.CreateControl);
                return remoteAgent;
            }
            catch (Exception ex)
            {
                var message = string.Format("Error loading type '{0}' from assembly '{1}'. {2}",
                    mainClass, assembly, ex.Message);
                return new ApplicationException(message, ex);
            }
        }

        private void RegisterObject()
        {
            RemotingServices.Marshal(this, "AgentLoader", typeof(IAgentLoader));
        }

        private void SignalReady()
        {
            var eventName = _name + ".Ready";
            var readyEvent = EventWaitHandle.OpenExisting(eventName);
            readyEvent.Set();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception =
                (e.ExceptionObject as Exception) ??
                new Exception("Unknown error. Exception object is null");
            ReportFatalError(exception);
        }

        private void ReportFatalError(Exception exception)
        {
            if (_host != null)
                _host.ReportFatalError(ExceptionUtil.GetUserMessage(exception), exception.ToString());
            Environment.Exit(2);
        }
    }
}
