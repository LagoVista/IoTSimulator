using LagoVista.Client.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class SimulatorPickerViewModel : ListViewModelBase<SimulatorSummary>
    {
        protected override string GetListURI()
        {
            return $"/api/org/simulators";
        }

        protected async override void ItemSelected(SimulatorSummary model)
        {
            LaunchArgs.SelectedAction(model);
            await ViewModelNavigation.GoBackAsync();
        }
    }
}
