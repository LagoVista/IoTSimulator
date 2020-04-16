using LagoVista.Client.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class SimulatorStatePickerViewModel : ListViewModelBase<SimulatorState>
    {
        protected override string GetListURI()
        {
            throw new NotImplementedException();
        }

        protected async override void ItemSelected(SimulatorState model)
        {
            LaunchArgs.SelectedAction(model);
            await ViewModelNavigation.GoBackAsync();
        }

        public override Task InitAsync()
        {
            ListItems = (LaunchArgs.ParentViewModel as TransmissionPlanViewModel).Simulator.SimulatorStates.OrderBy(stat=>stat.Name);
            return Task.FromResult(default(object));
        }
    }
}
