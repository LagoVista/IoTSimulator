using LagoVista.Core.Commanding;
using LagoVista.IoT.Simulator.Admin.Models;
using LagoVista.Client.Core.Resources;
using LagoVista.Simulator.Core.ViewModels.Simulator;
using System.Collections.Generic;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Auth;
using LagoVista.Client.Core.ViewModels.Orgs;
using System.Linq;
using System.Resources;

[assembly: NeutralResourcesLanguage("en")]

namespace LagoVista.Simulator.Core.ViewModels
{   
    public class MainViewModel : ListViewModelBase<IoT.Simulator.Admin.Models.SimulatorSummary>
    {
        public MainViewModel()
        {
            AddNewSimulatorCommand = new RelayCommand(AddNewSimulator);
            SettingsCommand = new RelayCommand(ToggleSettings);
            DeleteSimulatorCommand = new RelayCommand(DeleteSimulator);

            MenuItems = new List<MenuItem>()
            {
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<UserOrgsViewModel>(this)),
                    Name = ClientResources.MainMenu_SwitchOrgs,
                    FontIconKey = "fa-users"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<ChangePasswordViewModel>(this)),
                    Name = ClientResources.MainMenu_ChangePassword,
                    FontIconKey = "fa-key"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<InviteUserViewModel>(this)),
                    Name = ClientResources.MainMenu_InviteUser,
                    FontIconKey = "fa-user"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => Logout()),
                    Name = ClientResources.Common_Logout,
                    FontIconKey = "fa-sign-out"
                }
            };
        }

        public void AddNewSimulator()
        {
            ViewModelNavigation.NavigateAndCreateAsync<SimulatorEditorViewModel>(this);
        }

        public void ToggleSettings()
        {
            MenuVisible = !MenuVisible;
        }

        public async void DeleteSimulator(object simulatorId)
        {
            if (simulatorId != null)
            {
                if (await Popups.ConfirmAsync(ClientResources.ConfirmDelete_Title, ClientResources.ConfirmDelete_Msg))
                {
                    await PerformNetworkOperation(async () =>
                    {
                        var uri = $"/api/simulator/{simulatorId}";
                        var result = await RestClient.DeleteAsync(uri);
                        if (result.Success)
                        {
                            var removedSimulator = ListItems.Where(sim => sim.Id == simulatorId.ToString()).FirstOrDefault();
                            if (removedSimulator != null)
                            {
                                var simList = ListItems.ToList();
                                simList.Remove(removedSimulator);
                                ListItems = simList;
                            }
                        }
                        return result.ToInvokeResult();

                    });
                }
            }
        }

        protected override string GetHelpLink()
        {
            return "http://support.nuviot.com/help.html#/Simulator/Index.md";
        }

        protected override void ItemSelected(SimulatorSummary model)
        {
            SelectedItem = null;
            ViewModelNavigation.NavigateAndEditAsync<SimulatorViewModel>(this, model.Id);
        }

        protected override string GetListURI()
        {
            return $"/api/org/simulators";
        }

        public RelayCommand DeleteSimulatorCommand { get; private set; }

        public RelayCommand AddNewSimulatorCommand { get; private set; }

        public RelayCommand LogoutCommand { get; private set; }

        public RelayCommand SettingsCommand { get; private set; }
    }
}
