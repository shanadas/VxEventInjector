using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace VxEventAgent
{
    public abstract class EventAgentBase : MarshalByRefObject, IEventAgent
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Manufacturer { get; }
        public abstract string Author { get; }
        public abstract string Description { get; }
        public abstract List<NewSituation> Situations { get; }
        public abstract bool IsRunning { get; }
        public abstract bool IsConfigured { get; }
        public abstract bool RequiresControl { get; }
        public abstract FrameworkElement CreateControl();
        public abstract bool Run();
        public abstract void Stop();

        protected string EventAgentDocumentsDir
        {
            get
            {
                var docsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Pelco", "VxEventInjector", Author, Manufacturer, Name);
                Directory.CreateDirectory(docsDir);
                return docsDir;
            }
        }

        // Live forever
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            if (serviceType.IsAssignableFrom(GetType()))
                service = this;
            return service;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }
    }
}
