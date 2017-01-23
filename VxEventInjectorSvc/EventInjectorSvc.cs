using System.ServiceProcess;
using System.Threading.Tasks;

namespace VxEventInjectorSvc
{
    public partial class EventInjectorSvc : ServiceBase
    {
        Bootstrapper _bootstrapper = new Bootstrapper();

        public EventInjectorSvc()
        {
            InitializeComponent();
        }

        public void OnDebugStart()
        {
            Task.Run(() => OnStart(null));
            System.Threading.Thread.Sleep(1000 * 60 * 60);
        }

        protected override void OnStart(string[] args)
        {
            _bootstrapper.Run();
            _bootstrapper.StartAsync();
        }

        protected override void OnStop()
        {
            _bootstrapper.Stop();
        }
    }
}
