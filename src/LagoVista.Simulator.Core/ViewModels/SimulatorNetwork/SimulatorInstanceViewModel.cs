using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using System;
using System.Linq;
using LagoVista.Core;
using System.Threading.Tasks;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using LagoVista.Client.Core.Resources;
using LagoVista.Simulator.Core.Resources;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class SimulatorInstanceViewModel : FormViewModelBase<IoT.Simulator.Admin.Models.SimulatorInstance>
    {
        public SimulatorInstanceViewModel()
        {

        }

        public async override Task<InvokeResult> SaveRecordAsync()
        {
            var result = Validator.Validate(this.Model);

            if (LaunchArgs.LaunchType == LaunchTypes.Create)
            {
                var parent = LaunchArgs.GetParent<IoT.Simulator.Admin.Models.SimulatorNetwork>();
                if (parent.Simulators.Where(tmp => tmp.Key == Model.Key).Any())
                {
                    await this.Popups.ShowAsync(ClientResources.Common_KeyInUse);
                    return InvokeResult.FromErrors(ClientResources.Common_KeyInUse.ToErrorMessage());
                }

                parent.Simulators.Add(Model);
            }

            return result.ToInvokeResult();
        }

        public async override void EHPickerTapped(string fieldName)
        {
            if (fieldName == nameof(Model.Simulator))
            {
                if (Model.Simulator == null)
                {
                    await Popups.ShowAsync(SimulatorCoreResources.SimulatorInstance_SelectSimulatorBeforeTransmission);
                }
                else
                {
                    await ViewModelNavigation.NavigateAndPickAsync<SimulatorPickerViewModel>(this, SimulatorPicked);
                }
            }
        }

        public void SimulatorPicked(object obj)
        {
            if (obj is SimulatorSummary sim)
            {
                Model.Simulator = new LagoVista.Core.Models.EntityHeader<IoT.Simulator.Admin.Models.Simulator>()
                {
                    Id = sim.Id,
                    Text = sim.Name
                };

                View[nameof(Model.Simulator).ToFieldKey()].Display = Model.Simulator.Text;
                View[nameof(Model.Simulator).ToFieldKey()].Value = Model.Simulator.Id;
            }
            else
            {
                throw new Exception("Must return SimulatorSummary from picker.");
            }
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            View[nameof(Model.Key).ToFieldKey()].IsUserEditable = LaunchArgs.LaunchType == LaunchTypes.Create;

            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.Simulator));

            form.AddViewCell(nameof(Model.Key));
            form.AddViewCell(nameof(Model.DeviceId));
            form.AddViewCell(nameof(Model.Description));

            form.AddChildList<TransmissionPlanViewModel>(nameof(Model.TransmissionPlans), Model.TransmissionPlans);
        }

        protected override string GetRequestUri()
        {
            return "/api/simulator/instance/factory";
        }
    }
}
