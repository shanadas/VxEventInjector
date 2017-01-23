using SampleAgent.Events;

namespace SampleAgent.ViewModels
{
    class ShellCtrlViewModel : BaseViewModel
    {
        private string _name;

        public ShellCtrlViewModel()
        { }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); FireConfiguredEvent(!string.IsNullOrWhiteSpace(_name)); }
        }

        private void FireConfiguredEvent(bool isConfigured)
        {
            EventAgg.GetEvent<ConfiguredEvent>().Publish(isConfigured);
        }
    }
}
