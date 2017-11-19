using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Simulator.Admin.Models;
using System.Linq;
using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.Messages
{
    public class DynamicAttributeViewModel : FormViewModelBase<MessageDynamicAttribute>
    {
        public override Task<InvokeResult> SaveRecordAsync()
        {
            if (IsCreate)
            {
                var parent = LaunchArgs.GetParent<IoT.Simulator.Admin.Models.MessageTemplate>();
                if (parent.DynamicAttributes.Where(attr => attr.Key == Model.Key).Any())
                {
                    return Task.FromResult(InvokeResult.FromErrors(ClientResources.Common_KeyInUse.ToErrorMessage()));
                }
                parent.DynamicAttributes.Add(Model);
            }

            return Task.FromResult(InvokeResult.Success);
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.Key));
            form.AddViewCell(nameof(Model.ParameterType));
            form.AddViewCell(nameof(Model.DefaultValue));
            form.AddViewCell(nameof(Model.Description));
        }

        protected override string GetHelpLink()
        {
            return "http://support.nuviot.com/help.html#/Simulator/DynamicFields.md";
        }

        protected override string GetRequestUri()
        {
            return "/api/simulator/dyanimaicAttribute/factory";
        }
    }
}