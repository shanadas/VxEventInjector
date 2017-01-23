using Microsoft.Practices.Unity;
using Pelco.Logging;
using Prism.Unity;
using System;
using System.IO;
using System.Windows;

namespace SampleAgent
{
    class Bootstrapper : UnityBootstrapper
    {
        private static readonly string _LogFileName = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData), "Pelco", "VxEventInjector", "Logs", "SampleEventAgent.txt");

        private bool _isPlugin;

        public Bootstrapper(bool isPlugin = false)
        {
            _isPlugin = isPlugin;
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterInstance<ILogger>(new FileLogger(_LogFileName));
        }

        protected override DependencyObject CreateShell()
        {
            var view = _isPlugin ? null : (DependencyObject)Container.Resolve<Shell>();
            return view;
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            if (!_isPlugin)
            {
                App.Current.MainWindow = (Window)Shell;
                App.Current.MainWindow.Show();
            }
        }
    }
}
