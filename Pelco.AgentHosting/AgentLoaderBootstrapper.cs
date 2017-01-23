// Copyright (c) 2013 Ivan Krivyakov

using System;

namespace Pelco.AgentHosting
{
    /// <summary>
    /// Starts hosting logic
    /// </summary>
    /// <remarks>We need this class, because otherwise AgentLoader registration with remoting
    /// does not work. See
    /// http://stackoverflow.com/questions/18445813/remotingservices-marshal-does-not-work-when-invoked-from-another-appdomain
    /// </remarks>
    public class AgentLoaderBootstrapper : MarshalByRefObject
    {
        public void Run(string name)
        {
            new AgentLoader().Run(name);
        }
    }
}
