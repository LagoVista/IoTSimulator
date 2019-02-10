using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using System;
using LagoVista.Core;
using System.Linq;
using System.Threading.Tasks;
using LagoVista.IoT.Simulator.Admin.Models;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;
using LagoVista.Client.Core.Resources;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class MessageValueViewModel : FormViewModelBase<IoT.Simulator.Admin.Models.MessageValue>
    {
        public override async Task<InvokeResult> SaveRecordAsync()
        {
            var result = Validator.Validate(this.Model);

            if (LaunchArgs.LaunchType == LaunchTypes.Create && result.Successful)
            {
                var parent = LaunchArgs.GetParent<IoT.Simulator.Admin.Models.MessageTransmissionPlan>();
                if (parent.Values.Where(tmp => tmp.Attribute.Id == Model.Attribute.Id).Any())
                {
                    await this.Popups.ShowAsync(ClientResources.Common_KeyInUse);
                    return InvokeResult.FromErrors(ClientResources.Common_KeyInUse.ToErrorMessage());
                }

                parent.Values.Add(Model);
            }

            return result.ToInvokeResult();
        }

        public async override void EHPickerTapped(string fieldName)
        {
            if (fieldName == nameof(Model.Attribute))
            {
                await ViewModelNavigation.NavigateAndPickAsync<AttributePickerViewModel>(this, AttributePicked);
            }
        }

        public void AttributePicked(Object obj)
        {
            if(obj is MessageDynamicAttribute dynAttr)
            {
                Model.Attribute= new EntityHeader<MessageDynamicAttribute>()
                {
                    Id = dynAttr.Id,
                    Text = dynAttr.Name
                };

                View[nameof(Model.Attribute).ToFieldKey()].Display = Model.Attribute.Text;
                View[nameof(Model.Attribute).ToFieldKey()].Value = Model.Attribute.Id;
            }
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            View[nameof(Model.Attribute).ToFieldKey()].IsUserEditable = LaunchArgs.LaunchType == LaunchTypes.Create;

            form.AddViewCell(nameof(Model.Attribute));
            form.AddViewCell(nameof(Model.Value));

            var parentVM = (LaunchArgs.ParentViewModel as TransmissionPlanViewModel);

            Simulator = parentVM.Simulator;
            MessageTemplate = parentVM.MessageTemplate;
        }

        protected override string GetRequestUri()
        {
            return "/api/simulator/instance/transmissionplan/messagevalue/factory";
        }

        public IoT.Simulator.Admin.Models.Simulator Simulator { get; private set; }

        public IoT.Simulator.Admin.Models.MessageTemplate MessageTemplate { get; private set; }
    }
}
