using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Core;
using LagoVista.IoT.Simulator.Admin.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.Simulator
{
    public class SimulatorStateViewModel : FormViewModelBase<SimulatorState>
    {
        public override Task<InvokeResult> SaveRecordAsync()
        {
            if (IsCreate)
            {
                var parent = LaunchArgs.GetParent<IoT.Simulator.Admin.Models.Simulator>();
                if (parent.SimulatorStates.Where(attr => attr.Key == Model.Key).Any())
                {
                    return Task.FromResult(InvokeResult.FromErrors(ClientResources.Common_KeyInUse.ToErrorMessage()));
                }
                parent.SimulatorStates.Add(Model);
            }

            return Task.FromResult(InvokeResult.Success);
        }

        public async override Task InitAsync()
        {
            await base.InitAsync();
            this.Model.Id = Guid.NewGuid().ToId();
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.Key));
            form.AddViewCell(nameof(Model.Description));
        }


        protected override string GetRequestUri()
        {
            return "/api/simulator/state/factory";

        }
    }
}
