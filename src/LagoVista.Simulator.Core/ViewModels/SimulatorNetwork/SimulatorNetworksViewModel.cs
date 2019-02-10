using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.IoT.Simulator.Admin.Models;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class SimulatorNetworksViewModel : ListViewModelBase<IoT.Simulator.Admin.Models.SimulatorNetworkSummary>
    {
        public SimulatorNetworksViewModel()
        {
            AddNewSimulatorNetworkCommand = new RelayCommand(AddNewSimulatorNetwork);

            DeleteSimNetworkCommand = new RelayCommand(DeleteSimNetwork);
        }

        public void AddNewSimulatorNetwork()
        {
            ViewModelNavigation.NavigateAndCreateAsync<SimulatorNetworkViewModel>(this);
        }

        public async void DeleteSimNetwork(object networkId)
        {
            if (networkId != null)
            {
                if (await Popups.ConfirmAsync(ClientResources.ConfirmDelete_Title, ClientResources.ConfirmDelete_Msg))
                {
                    await PerformNetworkOperation(async () =>
                    {
                        var uri = $"/api/simulator/network/{networkId}";
                        var result = await RestClient.DeleteAsync(uri);
                        if (result.Success)
                        {
                            var removedNetwork = ListItems.Where(sim => sim.Id == networkId.ToString()).FirstOrDefault();
                            if (removedNetwork != null)
                            {
                                var simList = ListItems.ToList();
                                simList.Remove(removedNetwork);
                                ListItems = simList;
                            }
                        }

                        return result.ToInvokeResult();
                    });
                }
            }
        }

        protected override string GetListURI()
        {
            return "/api/simulator/networks";
        }
       
        protected override void ItemSelected(SimulatorNetworkSummary model)
        {
            SelectedItem = null;
            ViewModelNavigation.NavigateAndEditAsync<SimulatorNetworkViewModel>(this, model.Id);
        }

        public RelayCommand AddNewSimulatorNetworkCommand { get; private set; }

        public RelayCommand DeleteSimNetworkCommand { get; private set; }
    }
}
