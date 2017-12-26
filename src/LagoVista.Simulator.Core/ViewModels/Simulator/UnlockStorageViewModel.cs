using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Simulator.Core.ViewModels.Simulator
{
    public class UnlockStorageViewModel : XPlatViewModel
    {

        public UnlockStorageViewModel()
        {
            UnlockStorageCommand = new RelayCommand(UnlockStorage);
            CancelCommand = new RelayCommand(Cancel);
            ResetPasswordCommand = new RelayCommand(ResetPassword);
        }

        public async void ResetPassword()
        {
            if(await Popups.ConfirmAsync(Resources.SimulatorCoreResources.UnlockPassword_ResetPassword_Title, Resources.SimulatorCoreResources.UnlockPassword_ResetPassword_Prompt))
            {
                await ViewModelNavigation.NavigateAndPickAsync<SetStoragePasswordViewModel>(this, StorageUnlocked, CancelCredentialsEntry);
            }
        }

        public void StorageUnlocked(Object obj)
        {
            CloseScreen();
            LaunchArgs.SelectedAction?.Invoke(null);
        }

        public void CancelCredentialsEntry()
        {
            CloseScreen();
            LaunchArgs.CancelledAction.Invoke();
        }

        public void UnlockStorage()
        {
            if(SecureStorage.UnlockSecureStorage(Password))
            {
                CloseScreen();
                LaunchArgs.SelectedAction?.Invoke(null);
            }
            else
            {
                Popups.ShowAsync(Resources.SimulatorCoreResources.UnlockPassword_WrongPassword);
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

        public RelayCommand UnlockStorageCommand { get; private set; }

        public RelayCommand CancelCommand { get; private set; }

        public RelayCommand ResetPasswordCommand { get; private set; }
    }
}
