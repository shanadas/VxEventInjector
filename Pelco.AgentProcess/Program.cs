// Copyright (c) 2013 Ivan Krivyakov

using Pelco.AgentHosting;
using System;
using System.IO;

namespace Pelco.AgentProcess64
{
    class Program
    {
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: Agent Process name assemblyPath");
                return;
            }

            try
            {
                var name = args[0];
                int bits = IntPtr.Size * 8;
                Console.WriteLine("Starting Agent Process {0}, {1} bit", name, bits);

                var assemblyPath = args[1];
                Console.WriteLine("Agent assembly: {0}", assemblyPath);

                CheckFileExists(assemblyPath);
                var configFile = GetConfigFile(assemblyPath);

                var appBase = Path.GetDirectoryName(assemblyPath);

                var appDomain = CreateAppDomain(appBase, configFile);
                var bootstrapper = CreateInstanceFrom<AgentLoaderBootstrapper>(appDomain);
                bootstrapper.Run(name);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static T CreateInstanceFrom<T>(AppDomain appDomain)
        {
            return (T)appDomain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName);
        }

        private static void CheckFileExists(string path)
        {
            if (!File.Exists(path)) throw new InvalidOperationException("File '" + path + "' does not exist");
        }

        private static string GetConfigFile(string assemblyPath)
        {
            var name = assemblyPath + ".config";
            return File.Exists(name) ? name : null;
        }

        private static AppDomain CreateAppDomain(string appBase, string config)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = appBase,
                ConfigurationFile = string.IsNullOrEmpty(config) ? null : config
            };

            return AppDomain.CreateDomain("AgentDomain", null, setup);
        }
    }
}
