using System;
using System.ServiceModel;
using VxEventInjectorCommon;

namespace VxEventInjector.Services
{
    class IPCComSvc : IIPCComSvc, IConfiguratorCallback
    {
        public IConfigurator Configurator { get; private set; }
        private DuplexChannelFactory<IConfigurator> _configurator;

        public IPCComSvc(IConfiguratorCallback callback = null)
        {
            callback = callback ?? this;
            _configurator = new DuplexChannelFactory<IConfigurator>(callback,
                new NetTcpBinding()
                {
                    HostNameComparisonMode = HostNameComparisonMode.Exact,
                    ReceiveTimeout = TimeSpan.MaxValue,
                    SendTimeout = TimeSpan.MaxValue,
                    CloseTimeout = TimeSpan.MaxValue,
                    OpenTimeout = TimeSpan.MaxValue,
                    MaxReceivedMessageSize = Int32.MaxValue
                },
                new EndpointAddress(string.Format("{0}/{1}", Constants.BasePipeUri, Constants.ConfiguratorPipePath)));
            Configurator = _configurator.CreateChannel();
        }

        public void Dispose()
        {
            _configurator.Close();
            _configurator = null;
            Configurator = null;
        }

        #region IConfiguratorCallback
        public void OnSituation(double percent, string message)
        { }
        #endregion
    }
}
