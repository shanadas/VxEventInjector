using Prism.Commands;
using Prism.Regions;
using System;
using System.Windows.Controls;
using VxEventInjector.Extensions;
using VxEventInjector.Models;
using VxEventInjector.Services;

namespace VxEventInjector.ViewModels
{
    class VxLoginViewModel : BaseViewModel
    {
        public DelegateCommand<PasswordBox> LoginCmd { get; private set; }
        public VxLoginModel LoginInfo { get; private set; }
        private bool _isLoggingIn;
        private string _errorMsg;

        public VxLoginViewModel()
        {
            LoginCmd = new DelegateCommand<PasswordBox>(Login);
            LoginInfo = new VxLoginModel();
        }

        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            private set { SetProperty(ref _isLoggingIn, value); }
        }

        public string ErrorMsg
        {
            get { return _errorMsg; }
            private set { SetProperty(ref _errorMsg, value); LogIfNotEmpty(_errorMsg); OnPropertyChanged("ShowError"); }
        }

        public bool ShowError
        {
            get
            {
                bool showError = true;
                showError = showError && !string.IsNullOrWhiteSpace(ErrorMsg);
                return showError;
            }
        }

        private async void Login(PasswordBox passwordBox)
        {
            IsLoggingIn = true;
            bool success = false;
            LoginInfo.Password = passwordBox.Password;

            try
            {
                using (var ipc = new IPCComSvc())
                {
                    success = await ipc.Configurator.TestCredentialsAsync(LoginInfo.ToCredentials());
                    ErrorMsg = success ? string.Empty : Properties.Resources.VxLoginFailMsg;
                }
            }
            catch (System.ServiceModel.CommunicationObjectFaultedException)
            {
                ErrorMsg = Properties.Resources.InjectorSvcNotStartedMsg;
            }
            catch(Exception e)
            {
                ErrorMsg = e.Message;
            }

            if (success)
            {
                var np = new NavigationParameters();
                np.Add("credentials", LoginInfo.ToCredentials());
                RegionMgr.RequestNavigate(Properties.Resources.RegionShell, Properties.Resources.ViewConfigPage, np);
            }
            IsLoggingIn = false;
        }
    }
}
