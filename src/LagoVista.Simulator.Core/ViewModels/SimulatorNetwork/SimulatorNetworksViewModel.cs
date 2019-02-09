using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.IoT.Simulator.Admin.Models;
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
        }

        public void AddNewSimulatorNetwork()
        {
            ViewModelNavigation.NavigateAndCreateAsync<SimulatorNetworkViewModel>(this);
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
    }
}
