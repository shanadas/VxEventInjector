using System;
using VxEventInjectorCommon;

namespace VxEventInjector.Services
{
    interface IIPCComSvc : IDisposable
    {
        IConfigurator Configurator { get; }
    }
}
