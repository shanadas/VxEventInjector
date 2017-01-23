// Copyright (c) 2013 Ivan Krivyakov

using System;

namespace Pelco.AgentHosting
{
    [Serializable]
    public class AgentStartupInfo
    {
        public string FullAssemblyPath { get; set; }
        public string AssemblyName { get; set; }
        public int Bits { get; set; }
        public string MainClass { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public bool CreateControl { get; set; }
    }
}
