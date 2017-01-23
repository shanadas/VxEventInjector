namespace VxEventAgent
{
    /// <summary>
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// InjectEventAsync shall be called by the event agent when an event is received/processed
        /// and would like to inject the event into VideoXpert.
        /// </summary>
        void InjectEvent(NewEvent evt);
    }
}
