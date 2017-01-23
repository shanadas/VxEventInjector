using System;
using System.IO;

namespace VxEventInjectorCommon
{
    public static class Constants
    {
        public const string BasePipeUri = "net.tcp://localhost:9876/vxeventinjector";
        public const string ConfiguratorPipePath = "configurator";
        public const string Success = "Success";
        public const string Failure = "Failure";
        public const ushort MaxVxInjectionThreads = 20;
        public static readonly bool Debugging = System.Diagnostics.Debugger.IsAttached;
        public static readonly string InjectorDir = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData), "Pelco", "VxEventInjector");
        public static readonly string LogDir = Path.Combine(InjectorDir, "Logs");
        public static readonly string AgentDataDir = Path.Combine(InjectorDir, "EventAgents");

        static Constants()
        {
            Directory.CreateDirectory(InjectorDir);
            Directory.CreateDirectory(LogDir);
            Directory.CreateDirectory(AgentDataDir);
        }
    }
}
