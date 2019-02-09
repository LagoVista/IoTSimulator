using LagoVista.Client.Core.ViewModels;
using LagoVista.Core;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.SimulatorNetwork
{
    public class SimulatorNetworkViewModel : FormViewModelBase<IoT.Simulator.Admin.Models.SimulatorNetwork>
    {
        public override async Task<InvokeResult> SaveRecordAsync()
        {
            return await PerformNetworkOperation(async () =>
            {
                if (LaunchArgs.LaunchType == LaunchTypes.Create)
                {
                    return await FormRestClient.AddAsync("/api/simulator/network", this.Model);
                }
                else
                {
                    return await FormRestClient.UpdateAsync("/api/simulator/network", this.Model);
                }
            });
        }

        protected override string GetRequestUri()
        {
            return this.LaunchArgs.LaunchType == LaunchTypes.Edit ? $"/api/simulator/network/{LaunchArgs.ChildId}" : "/api/simulator/network/factory";
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            View[nameof(Model.Key).ToFieldKey()].IsUserEditable = LaunchArgs.LaunchType == LaunchTypes.Create;

            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.Key));
            form.AddViewCell(nameof(Model.Description));

            form.AddChildList<SimulatorInstanceViewModel>(nameof(Model.Simulators), Model.Simulators);
        }
    }
}
