using System.Threading.Tasks;
using VxEventInjectorCommon;

namespace VxEventInjectorSvc.Services.Configurator
{
    interface IConfiguratorSvc : IConfigurator
    {
        /// <summary>
        /// Start listening so that the VxEventInjector can start sending IPC messages
        /// </summary>
        /// <returns></returns>
        Task<bool> StartListeningAsync();

        /// <summary>
        /// Stop listening for IPC messages
        /// </summary>
        /// <returns></returns>
        Task StopListeningAsync();
    }
}
