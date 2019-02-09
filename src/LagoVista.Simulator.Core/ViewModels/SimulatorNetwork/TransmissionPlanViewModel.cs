using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class TransmissionPlanViewModel : FormViewModelBase<IoT.Simulator.Admin.Models.MessageTransmissionPlan>
    {
        public async override Task<InvokeResult> SaveRecordAsync()
        {
            var result = Validator.Validate(this.Model);

            if (LaunchArgs.LaunchType == LaunchTypes.Create)
            {
                var parent = LaunchArgs.GetParent<IoT.Simulator.Admin.Models.SimulatorInstance>();
                if (parent.TransmissionPlans.Where(tmp => tmp.Key == Model.Key).Any())
                {
                    await this.Popups.ShowAsync(ClientResources.Common_KeyInUse);
                    return InvokeResult.FromErrors(ClientResources.Common_KeyInUse.ToErrorMessage());
                }

                parent.TransmissionPlans.Add(Model);
            }

            return result.ToInvokeResult();
        }

        public async override void EHPickerTapped(string fieldName)
        {
            if (fieldName == nameof(Model.Message))
            {
                await ViewModelNavigation.NavigateAndPickAsync<MessagePickerViewModel>(this, MessagePicked);
            }
        }

        private void MessagePicked(object obj)
        {
            if (obj is MessageTemplate msg)
            {
                Model.Message = new EntityHeader<MessageTemplate>()
                {
                    Id = msg.Id,
                    Text = msg.Name
                };

                View[nameof(Model.Message).ToFieldKey()].Display = Model.Message.Text;
                View[nameof(Model.Message).ToFieldKey()].Value = Model.Message.Id;
            }
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            View[nameof(Model.Key).ToFieldKey()].IsUserEditable = LaunchArgs.LaunchType == LaunchTypes.Create;

            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.Key));
            form.AddViewCell(nameof(Model.PeriodMS));
            form.AddViewCell(nameof(Model.Message));

            form.AddChildList<MessageValueViewModel>(nameof(Model.Values), Model.Values);
        }

        public async override Task InitAsync()
        {
            var simEH = (LaunchArgs.Parent as SimulatorInstance).Simulator;

            await PerformNetworkOperation(async () =>
            {
                var sim = await RestClient.GetAsync<DetailResponse<IoT.Simulator.Admin.Models.Simulator>>($"/api/simulator/{simEH.Id}");
                Simulator = sim.Result.Model;
                
            });

            await base.InitAsync();
        }

        protected override string GetRequestUri()
        {
            return "/api/simulator/instance/transmissionplan/factory";
        }

        public IoT.Simulator.Admin.Models.Simulator Simulator { get; private set; }
    }
}
