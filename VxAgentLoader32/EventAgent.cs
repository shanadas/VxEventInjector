using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using VxEventAgent;

namespace VxAgentLoader32
{
    class EventAgent : EventAgentBase, IVxAgentLoader32
    {
        private const string _AgentDirName = "Agents";
        private const string _AgentExtMask = "*.dll|*.exe";

        public override string Id
        {
            get { return "B14F267C-8D9F-40C9-937B-404FD17A0F68"; }
        }

        public override string Name
        {
            get { return "VxAgentLoader32"; }
        }

        public override string Version
        {
            get { return "1.0.1"; }
        }

        public override string Manufacturer
        {
            get { return "Pelco"; }
        }

        public override string Author
        {
            get { return "Pelco"; }
        }

        public override string Description
        {
            get { return "Should never be shown as an agent, used simply to locate 32bit agents"; }
        }

        public override List<NewSituation> Situations
        {
            get { return new List<NewSituation>(); }
        }

        public override bool IsRunning
        {
            get { return false; }
        }

        public override bool IsConfigured
        {
            get { return false; }
        }

        public override bool RequiresControl
        {
            get { return false; }
        }

        public override FrameworkElement CreateControl()
        {
            return null;
        }

        public override bool Run()
        {
            return true;
        }

        public override void Stop()
        { }

        public List<AgentCatalogEntry> Get32BitCatalog(string agentDir)
        {
            return GetAllAgents(agentDir);
        }

        // Adapted from AgentLoaderSvc::LoadAllAgents
        private List<AgentCatalogEntry> GetAllAgents(string agentDir)
        {
            var agentCatalog = new List<AgentCatalogEntry>();
            List<string> files;

            if (Directory.Exists(agentDir))
            {
                files = GetFiles(agentDir, _AgentExtMask, SearchOption.AllDirectories);
                var assemblies = new List<Assembly>();

                files.ForEach(val =>
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(val);
                        bool add = true;
                        add = add && !assemblyName.FullName.ToLower().StartsWith("system.");
                        add = add && !assemblyName.FullName.ToLower().StartsWith("microsoft.");
                        add = add && !assemblyName.FullName.ToLower().StartsWith("vxagentloader32");
                        if (add)
                        {
                            var assembly = Assembly.Load(assemblyName);
                            assemblies.Add(assembly);
                        }
                    }
                    catch
                    { }
                });

                var agentTypes = new List<Type>();
                assemblies.ForEach(assembly =>
                {
                    if (assembly != null)
                    {
                        try
                        {
                            PortableExecutableKinds peKind;
                            ImageFileMachine imageFileMachine;
                            assembly.ManifestModule.GetPEKind(out peKind, out imageFileMachine);

                            bool load32Bit = (peKind & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly;
                            if (load32Bit && (peKind & PortableExecutableKinds.Preferred32Bit) == PortableExecutableKinds.Preferred32Bit)
                                load32Bit = true;
                            else if (load32Bit && (peKind & PortableExecutableKinds.Required32Bit) == PortableExecutableKinds.Required32Bit)
                                load32Bit = true;
                            else
                                load32Bit = false;

                            if (load32Bit)
                            {
                                agentTypes.AddRange(assembly.GetTypes()
                                    .Where(val => !val.IsInterface && !val.IsAbstract &&
                                        val.GetInterface(typeof(IEventAgent).FullName) != null));
                            }
                        }
                        catch { }
                    }
                });

                agentTypes.ForEach(type => agentCatalog.Add(new AgentCatalogEntry()
                    {
                        AssemblyPath = type.Assembly.Location,
                        MainClass = type.FullName
                    }));
            }
            return agentCatalog;
        }

        // Adapted from AgentLoaderSvc
        private List<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            string[] searchPatterns = searchPattern.Split('|');
            var files = new List<string>();

            foreach (var sp in searchPatterns)
                files.AddRange(Directory.GetFiles(path, sp, searchOption));

            files.Sort();
            return files;
        }
    }
}
