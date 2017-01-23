// Copyright (c) 2013 Ivan Krivyakov

using Pelco.AgentHosting;
using Pelco.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using VxEventAgent;

namespace VxEventInjectorCommon.Services.AgentLoader
{
    public class AgentProcessProxy : IDisposable
    {
        public IRemoteAgent RemoteAgent { get; private set; }
        private readonly IHost _host;
        private readonly AgentStartupInfo _startupInfo;
        private readonly ILogger _logger;
        private EventWaitHandle _readyEvent;
        private Process _process;
        private string _name;
        private IAgentLoader _agentLoader;

        public AgentProcessProxy(AgentStartupInfo startupInfo, IHost host, ILogger logger)
        {
            _startupInfo = startupInfo;
            _host = host;
            _logger = logger;
        }

        public Process Process
        {
            get { return _process; }
        }

        public void Start()
        {
            if (Process != null) throw new InvalidOperationException("Agent process already started, cannot load more than one agent per process");
            StartAgentProcess(_startupInfo.FullAssemblyPath);
        }

        public void LoadAgent()
        {
            if (Process == null)
                _logger.LogThenThrow(new InvalidOperationException("Agent process not started"));
            if (Process.HasExited)
                _logger.LogThenThrow(new InvalidOperationException("Agent process has terminated unexpectedly"));

            _agentLoader = GetAgentLoader();
            RemoteAgent = _agentLoader.LoadAgent(_host, _startupInfo);
        }

        public void Dispose()
        {
            if (RemoteAgent != null)
            {
                try
                {
                    RemoteAgent.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Log(string.Format("Error disposing remote agent for {0}", _startupInfo.Name), ex);
                }
            }

            if (_agentLoader != null)
            {
                try
                {
                    _agentLoader.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Log(string.Format("Error disposing agent loader for {0}", _startupInfo.Name), ex);
                }
            }

            // This can take some time if we have many agents; should be made asynchronous
            if (Process != null)
            {
                Process.WaitForExit(5000);
                if (!Process.HasExited)
                {
                    _logger.Log(string.Format("Remote process for {0} did not exit within timeout period and will be terminated", _startupInfo.Name));
                    Process.Kill();
                }
            }
        }

        private void StartAgentProcess(string assemblyPath)
        {
            _name = "AgentProcess." + Guid.NewGuid();
            var eventName = _name + ".Ready";
            _readyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);

            var directory = Path.GetDirectoryName(GetType().Assembly.Location);
            var exeFile = _startupInfo.Bits == 64 ? "Pelco.AgentProcess64.exe" : "Pelco.AgentProcess.exe";
            var processName = Path.Combine(directory, exeFile);

            if (!File.Exists(processName))
                _logger.LogThenThrow(new InvalidOperationException("Could not find file '" + processName + "'"));

            const string quote = "\"";
            const string doubleQuote = "\"\"";

            var quotedAssemblyPath = quote + assemblyPath.Replace(quote, doubleQuote) + quote;
            var createNoWindow = true;

            var info = new ProcessStartInfo
            {
                Arguments = _name + " " + quotedAssemblyPath,
                CreateNoWindow = createNoWindow,
                UseShellExecute = false,
                FileName = processName
            };

            Trace.WriteLine(info.Arguments);

            _process = Process.Start(info);
        }

        private IAgentLoader GetAgentLoader()
        {
            if (Process.HasExited)
                _logger.LogThenThrow(new InvalidOperationException("Agent process has terminated unexpectedly"));

            var timeoutMs = 5000;

            if (!_readyEvent.WaitOne(timeoutMs))
                _logger.LogThenThrow(new InvalidOperationException("Agent process did not respond within timeout period"));

            var hostChannelName = "WpfHost." + Process.GetCurrentProcess().Id;
            IpcServices.RegisterChannel(hostChannelName);

            var url = "ipc://" + _name + "/AgentLoader";
            var agentLoader = (IAgentLoader)Activator.GetObject(typeof(IAgentLoader), url);
            return agentLoader;
        }
    }
}
