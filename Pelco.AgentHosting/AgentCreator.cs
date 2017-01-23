// Copyright (c) 2013 Ivan Krivyakov

using System;
using System.Reflection;
using System.Windows;
using VxEventAgent;

namespace Pelco.AgentHosting
{
    static class AgentCreator
    {
        public static object CreateAgent(string assemblyName, string typeName, IHost host)
        {
            var assembly = Assembly.Load(assemblyName);
            var type = assembly.GetType(typeName);

            if (type == null)
            {
                throw new InvalidOperationException(string.Format("Could not find type {0} in assembly {1}",
                    typeName, assemblyName));
            }

            SetupWpfApplication(assembly);
            var hostConstructor = type.GetConstructor(new[] {typeof(IHost)});
            if (hostConstructor != null)
            {
                return hostConstructor.Invoke(new object[]{host});
            }

            var defaultConstructor = type.GetConstructor(new Type[0]);
            if (defaultConstructor == null)
            {
                var message = string.Format("Cannot create an instance of {0}. Either a public default constructor, or a public constructor taking IWpfHost must be defined", typeName);
                throw new InvalidOperationException(message);
            }
            return defaultConstructor.Invoke(null);
        }

        private static void SetupWpfApplication(Assembly assembly)
        {
            var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            Application.ResourceAssembly = assembly;
        }
    }
}
