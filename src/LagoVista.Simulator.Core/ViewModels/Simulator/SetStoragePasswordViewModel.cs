using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Text.RegularExpressions;

namespace LagoVista.Simulator.Core.ViewModels.Simulator
{
    public class SetStoragePasswordViewModel : XPlatViewModel
    {
        private const string REGEX = "^[a-zA-Z0-9!@#$%^&*]{6,20}$";

        public SetStoragePasswordViewModel()
        {
            SetStoragePasswordCommand = new RelayCommand(SetStoragePassword);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void SetStoragePassword()
        {
            var regex = new Regex(REGEX);

            if(String.IsNullOrEmpty(Password))
            {
                Popups.ShowAsync(Resources.SimulatorCoreResources.SecureStorage_PasswordIsRequired);
            }
            else if(!regex.Match(Password).Success)
            {
                Popups.ShowAsync(Resources.SimulatorCoreResources.SecureStorage_PasswordFormat);
            }
            else if (Password == ConfirmPassword)
            {
                this.CloseScreen();
                SecureStorage.Reset(Password);
                LaunchArgs.SelectedAction?.Invoke(null);
            }
            else
            {
                Popups.ShowAsync(Resources.SimulatorCoreResources.SecureStorage_PasswordConfirmPasswordMisMatch);
            }
        }

        public void Cancel()
        {
            CloseScreen();
            LaunchArgs.CancelledAction.Invoke();            
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { Set(ref _confirmPassword, value); }
        }

        public RelayCommand SetStoragePasswordCommand {get; private set;}

        public RelayCommand CancelCommand { get; private set; }
    }
}