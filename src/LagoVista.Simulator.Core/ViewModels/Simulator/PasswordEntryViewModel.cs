using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Simulator.Core.ViewModels.Simulator
{
    public class PasswordEntryViewModel : XPlatViewModel
    {
        public PasswordEntryViewModel()
        {
            OKTappedCommand = new RelayCommand(OKTapped);

            CancelledTappedCommand = new RelayCommand(CancelledTapped);
        }

        private void CancelledTapped()
        {
            LaunchArgs.CancelledAction();
            this.CloseScreen();
        }

        private void OKTapped()
        {
            if (!String.IsNullOrEmpty(PasswordOrAccessKey))
            {
                LaunchArgs.SelectedAction(PasswordOrAccessKey);
                this.CloseScreen();
            }
        }

        public String PasswordOrAccessKey { get; set; }

        public RelayCommand OKTappedCommand { get; private set; }

        public RelayCommand CancelledTappedCommand { get; private set; }
    }
}
