// Copyright (c) 2013 Ivan Krivyakov

using System;
using System.AddIn.Contract;

namespace Pelco.AgentHosting
{
    class NativeHandleContractInsulator : MarshalByRefObject, INativeHandleContract
    {
        private INativeHandleContract _source;

        public NativeHandleContractInsulator(INativeHandleContract source)
        {
            _source = source;
        }

        public IntPtr GetHandle()
        {
            return _source.GetHandle();
        }

        public int AcquireLifetimeToken()
        {
            return _source.AcquireLifetimeToken();
        }

        public int GetRemoteHashCode()
        {
            return _source.GetRemoteHashCode();
        }

        public IContract QueryContract(string contractIdentifier)
        {
            return _source.QueryContract(contractIdentifier);
        }

        public bool RemoteEquals(IContract contract)
        {
            return _source.RemoteEquals(contract);
        }

        public string RemoteToString()
        {
            return _source.RemoteToString();
        }

        public void RevokeLifetimeToken(int token)
        {
            _source.RevokeLifetimeToken(token);
        }

        // Live forever
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
