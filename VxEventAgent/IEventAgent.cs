using System;
using System.Collections.Generic;
using System.Windows;

namespace VxEventAgent
{
    public interface IEventAgent : IServiceProvider, IDisposable
    {
        /// <summary>
        /// This specifies this particular agent among all the others. It should be a UUID and never
        /// change once set, it will stay the same for all versions of this event agent. Required.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Name of the event agent. Required.
        /// e.g. SavVi Event Agent
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Version of the event agent. Required.
        /// e.g. 1.0.3
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Manufacturer of the event producing device. Required.
        /// e.g. AgentVi
        /// </summary>
        string Manufacturer { get; }

        /// <summary>
        /// The company authoring this event agent. Required
        /// e.g. Pelco
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Short description of what device events are queried from. Required
        /// e.g. Collects events from the AgentVi savVi system
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Returns a List of all possible Situations which are required by the event agent. Required.
        /// </summary>
        List<NewSituation> Situations { get; }

        /// <summary>
        /// Returns true if the event agent is currently running and listening for events from the third party. Required
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Signifies if the event agent is currently properly configured. Required
        /// </summary>
        bool IsConfigured { get; }

        /// <summary>
        /// This indentifies at a glance whether the event agent requires a control via CreateControlAsync or not.
        /// if this returns false, then CreateControlAsync should return null. Required.
        /// </summary>
        bool RequiresControl { get; }

        /// <summary>
        /// CreateControl will allow the event
        /// agent to present an interface to the user if user configuration is required to
        /// properly configure the event agent. Optional.
        /// </summary>
        FrameworkElement CreateControl();

        /// <summary>
        /// Run is called by the windows service and should start its own thread to start listening for
        /// events from it's configured event generating device. If everything is started properly
        /// then true should be returned. All other properties, especially Situations should
        /// be completed before calling Run. Required.
        /// </summary>
        bool Run();

        /// <summary>
        /// Stop should stop the event agent from listening to it's event generating device's events. Required.
        /// </summary>
        void Stop();
    }
}
