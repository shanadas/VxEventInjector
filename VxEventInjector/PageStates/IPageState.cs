namespace VxEventInjector.PageStates
{
    interface IPageState
    {
        bool CanStart { get; }
        void Start();
        bool CanPrevious { get; }
        void Previous();
        bool CanNext { get; }
        void Next();
        void Close();
    }
}
