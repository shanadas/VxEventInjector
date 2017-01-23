// Copyright (c) 2013 Ivan Krivyakov

using System;
using System.Reflection;
using VxEventAgent;

namespace Pelco.AgentHosting
{
    class AssemblyResolver
    {
        private string _thisAssemblyName;
        private string _interfacesAssemblyName;

        public void Setup()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            _thisAssemblyName = GetType().Assembly.GetName().Name;
            _interfacesAssemblyName = typeof(IHost).Assembly.GetName().Name;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;
            var name = new AssemblyName(args.Name);
            if (name.Name == _thisAssemblyName)
                assembly = GetType().Assembly;
            if (name.Name == _interfacesAssemblyName)
                assembly = typeof(IHost).Assembly;
            return assembly;
        }
    }
}
