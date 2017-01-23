using System.ServiceProcess;
using VxEventInjectorCommon;

namespace VxEventInjectorSvc
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var eventInjectorSvc = new EventInjectorSvc();
            var servicesToRun = new ServiceBase[] { eventInjectorSvc };

            if (Constants.Debugging)
                eventInjectorSvc.OnDebugStart();
            else
                ServiceBase.Run(servicesToRun);
        }
    }
}
