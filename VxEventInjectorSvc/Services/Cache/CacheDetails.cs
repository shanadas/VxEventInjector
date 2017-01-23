using System;
using System.Collections.Generic;
using VxEventInjectorCommon;

namespace VxEventInjectorSvc.Services.Cache
{
    [Serializable]
    class CacheDetails
    {
        public Credentials VxCredentials { get; set; }
        public List<string> AgentIds { get; set; }

        public CacheDetails()
        {
            AgentIds = new List<string>();
            VxCredentials = new Credentials();
        }

        public CacheDetails(CacheDetails other)
            : this()
        {
            if (other != null)
            {
                VxCredentials = new Credentials(other.VxCredentials);
                AgentIds = new List<string>(other.AgentIds);
            }
        }
    }
}
