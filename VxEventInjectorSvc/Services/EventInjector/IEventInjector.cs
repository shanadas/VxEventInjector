using System.Threading.Tasks;

namespace VxEventInjectorSvc.Services.EventInjector
{
    interface IEventInjector
    {
        Task UpdateSituations();
        void StartLoop();
        void StopLoop();
    }
}
