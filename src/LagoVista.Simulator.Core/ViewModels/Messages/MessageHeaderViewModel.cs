using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using System.Linq;
using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.Messages
{
    public class MessageHeaderViewModel : FormViewModelBase<MessageHeader>
    {
        public override Task<InvokeResult> SaveRecordAsync()
        {
            if (IsCreate)
            {
                var parent = LaunchArgs.GetParent<IoT.Simulator.Admin.Models.MessageTemplate>();
                if (parent.MessageHeaders.Where(attr => attr.Key == Model.Key).Any())
                {
                    return Task.FromResult(InvokeResult.FromErrors(ClientResources.Common_KeyInUse.ToErrorMessage()));
                }
                parent.MessageHeaders.Add(Model);
            }

            return Task.FromResult(InvokeResult.Success);
        }

        protected override string GetRequestUri()
        {
            return "/api/simulator/messageheader/factory";
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.Key));
            form.AddViewCell(nameof(Model.HeaderName));
            form.AddViewCell(nameof(Model.Value));
            form.AddViewCell(nameof(Model.Description));
        }
    }
}