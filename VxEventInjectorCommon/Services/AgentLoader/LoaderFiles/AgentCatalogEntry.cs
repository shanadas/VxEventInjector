// Copyright (c) 2013 Ivan Krivyakov


namespace VxEventInjectorCommon.Services.AgentLoader
{
    public class AgentCatalogEntry
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string AssemblyPath { get; set; }
        public string MainClass { get; set; }
        public int Bits { get; set; }
        public string Parameters { get; set; }
        public bool CreateControl { get; set; }
    }
}