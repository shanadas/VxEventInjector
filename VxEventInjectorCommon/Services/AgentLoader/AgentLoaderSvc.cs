using Microsoft.Practices.Unity;
using Pelco.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VxEventAgent;

namespace VxEventInjectorCommon.Services.AgentLoader
{
    public class AgentLoaderSvc : IAgentLoaderSvc
    {
        private const string _AgentDirName = "Agents";
        private const string _AgentExtMask = "*.dll|*.exe";
        public List<IEventAgent> Agents { get; private set; }
        private ILogger _logger;
        private AgentController _agentCtrl;
        private IUnityContainer _container;

        public AgentLoaderSvc(ILogger logger, IUnityContainer container)
        {
            _container = container;
            _logger = logger;
            _agentCtrl = _container.Resolve<AgentController>();
            Agents = new List<IEventAgent>();
        }

        public void LoadAllAgents()
        {
            Agents.ForEach(val => val.Dispose());
            Agents.Clear();

            LoadAllAgentsAsync();
            // Due to an unknown fact (probably due to plugin magic) we can't precisely determine completion, so we wait.
            System.Threading.Thread.Sleep(1000 * 5);
        }

        private void LoadAllAgentsAsync()
        {
            _logger.Log("Looking for agents");

            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _AgentDirName);
            List<string> files;

            if (Directory.Exists(path))
            {
                files = GetFiles(path, _AgentExtMask, SearchOption.AllDirectories);
                var assemblies = new List<Assembly>();

                files.ForEach(val =>
                    {
                        try
                        {
                            var assemblyName = AssemblyName.GetAssemblyName(val);
                            bool add = true;
                            add = add && !assemblyName.FullName.ToLower().StartsWith("system.");
                            add = add && !assemblyName.FullName.ToLower().StartsWith("microsoft.");
                            if(add)
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
                                var types = assembly.GetTypes();
                                var notInterface = types.Where(val => !val.IsInterface);
                                var notAbstract = types.Where(val => !val.IsAbstract);
                                var isEventAgent = types.Where(val => val.GetInterface(typeof(IEventAgent).FullName) != null);

                                agentTypes.AddRange(assembly.GetTypes()
                                    .Where(val => !val.IsInterface && !val.IsAbstract &&
                                    val.GetInterface(typeof(IEventAgent).FullName) != null));
                            }
                            catch { }
                        }
                    });

                agentTypes.ForEach(async val =>
                    {
                        try
                        {
                            var agentLoader32 = val.GetInterface(typeof(IVxAgentLoader32).FullName);
                            bool isAgentLoader32 = agentLoader32 != null;
                            bool requires32Bit = isAgentLoader32;

                            PortableExecutableKinds peKind;
                            ImageFileMachine imageFileMachine;
                            val.Assembly.ManifestModule.GetPEKind(out peKind, out imageFileMachine);

                            bool load64Bit = false;
                            if (peKind == PortableExecutableKinds.ILOnly)
                                load64Bit = true;
                            if ((peKind & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly &&
                                (peKind & PortableExecutableKinds.PE32Plus) == PortableExecutableKinds.PE32Plus)
                                load64Bit = true;

                            if (!load64Bit) return;

                            var fileName = Path.GetFileName(Assembly.GetEntryAssembly().Location);
                            bool createControl = true;
                            createControl = createControl && fileName != "VxEventInjectorSvc.exe";
                            createControl = createControl && !isAgentLoader32;

                            var getAgent = new Func<string, string, int, bool, Task<Agent>>(async (assPath, mainClass, assBits, createCtrl) =>
                            {
                                return await _agentCtrl.LoadAgentAsync(new AgentCatalogEntry()
                                {
                                    AssemblyPath = assPath,
                                    MainClass = mainClass,
                                    Name = mainClass,
                                    Bits = assBits,
                                    CreateControl = createCtrl
                                });
                            });

                            var agent = await getAgent(val.Assembly.Location, val.FullName, requires32Bit ? 32 : 64, createControl);
                            if (agent == null)
                            {
                                _logger.Log($"Error loading agent {val.FullName}");
                                return;
                            }

                            if (isAgentLoader32)
                            {
                                var agentLoader32Svc = agent.GetService(typeof(IVxAgentLoader32)) as IVxAgentLoader32;
                                if (agentLoader32Svc != null)
                                {
                                    var agentCatalog32 = agentLoader32Svc.Get32BitCatalog(path);
                                    if (agentCatalog32 != null && agentCatalog32.Count > 0)
                                    {
                                        foreach (var agentCatalogItem in agentCatalog32)
                                        {
                                            try
                                            {
                                                var agent32 = await getAgent(agentCatalogItem.AssemblyPath, agentCatalogItem.MainClass, 32, true);
                                                var eventAgent32 = _container.Resolve<AgentFacade>(new ParameterOverride("agent", agent32));
                                                Agents.Add(eventAgent32);
                                                _logger.Log(string.Format("Found x86 {0}", eventAgent32.Name));
                                            }
                                            catch (Exception e)
                                            {
                                                _logger.Log(string.Format("Attempted to load {0} via x86 reflection, but failed", agentCatalogItem.MainClass), e);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var eventAgent = _container.Resolve<AgentFacade>(new ParameterOverride("agent", agent));
                                Agents.Add(eventAgent);
                                _logger.Log(string.Format("Found x64 {0}", eventAgent.Name));
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Log(string.Format("Attempted to load {0} via x64 reflection, but failed", val.FullName), e);
                        }
                    });
            }
        }

        private List<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            string[] searchPatterns = searchPattern.Split('|');
            var files = new List<string>();

            foreach(var sp in searchPatterns)
                files.AddRange(Directory.GetFiles(path, sp, searchOption));

            files.Sort();
            return files;
        }
    }
}
