using LagoVista.Client.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class MessagePickerViewModel : ListViewModelBase<IoT.Simulator.Admin.Models.MessageTemplate>
    {
        protected override string GetListURI()
        {
            throw new NotImplementedException();
        }

        public override Task InitAsync()
        {
            ListItems = (LaunchArgs.ParentViewModel as TransmissionPlanViewModel).Simulator.MessageTemplates;

            return Task.FromResult(default(object));
        }

        protected async override void ItemSelected(MessageTemplate model)
        {
            LaunchArgs.SelectedAction(model);
            await ViewModelNavigation.GoBackAsync();
        }

    }
}
