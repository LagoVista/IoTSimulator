using LagoVista.Client.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class AttributePickerViewModel : ListViewModelBase<MessageDynamicAttribute>
    {
        protected override string GetListURI()
        {
            throw new NotImplementedException();
        }

        protected async override void ItemSelected(MessageDynamicAttribute model)
        {
            LaunchArgs.SelectedAction(model);
            await ViewModelNavigation.GoBackAsync();
        }

        public override Task InitAsync()
        {
            var parentVM = (LaunchArgs.ParentViewModel as MessageValueViewModel);

            ListItems = parentVM.MessageTemplate.DynamicAttributes;

            return Task.FromResult(default(object));
        }
    }
}
