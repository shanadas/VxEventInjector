using System.Windows;
using VxEventInjector.ViewModels;
using VxEventInjector.Views;

namespace VxEventInjector.PageStates
{
    class ProcessAgentsState : PageStateBase
    {
        private bool _canStart;
        private bool _canPrevious;

        public ProcessAgentsState()
        {
            _canStart = true;
            _canPrevious = true;
        }

        public override bool CanStart
        {
            get { return _canStart; }
        }

        public override void Start()
        {
            var result = MessageBox.Show(App.Current.MainWindow, Properties.Resources.ProceedWithConfigurationSave,
                Properties.Resources.Alert, MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                _canPrevious = false;
                _canStart = false;
                var viewModel = GetViewModel<ProcessAgentsView, ProcessAgentsViewModel>();
                viewModel.StartCmd.Execute();
            }
        }

        public override bool CanPrevious
        {
            get { return _canPrevious; }
        }

        public override void Previous()
        {
            var viewModel = GetViewModel<ProcessAgentsView, ProcessAgentsViewModel>();
            PageViewModel.CurrentState = viewModel.FromState;
            RegionMgr.RequestNavigate(Properties.Resources.RegionConfigMain, viewModel.FromView);
        }
    }
}
