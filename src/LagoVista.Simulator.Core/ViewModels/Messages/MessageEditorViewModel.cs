using LagoVista.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using System.Threading.Tasks;
using LagoVista.Core;
using System.Linq;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Validation;
using LagoVista.Core.Models;
using LagoVista.Client.Core.Resources;

namespace LagoVista.Simulator.Core.ViewModels.Messages
{
    public class MessageEditorViewModel : FormViewModelBase<MessageTemplate>
    {
        public async override Task<InvokeResult> SaveRecordAsync()
        {
            var result = Validator.Validate(this.Model);

            if (LaunchArgs.LaunchType == LaunchTypes.Create)
            {
                var parent = LaunchArgs.GetParent<IoT.Simulator.Admin.Models.Simulator>();
                if (parent.MessageTemplates.Where(tmp => tmp.Key == Model.Key).Any())
                {
                    await this.Popups.ShowAsync(ClientResources.Common_KeyInUse);
                    return InvokeResult.FromErrors(ClientResources.Common_KeyInUse.ToErrorMessage());
                }

                parent.MessageTemplates.Add(Model);
            }

            return InvokeResult.Success;
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            if (IsCreate)
            {
                var parent = LaunchArgs.GetParent<IoT.Simulator.Admin.Models.Simulator>();
                Model.EndPoint = parent.DefaultEndPoint;
                Model.Port = parent.DefaultPort;
                Model.Transport = parent.DefaultTransport;
                Model.QueueName = parent.QueueName;
                Model.PayloadType = parent.DefaultPayloadType;

                View[nameof(Model.TextPayload).ToFieldKey()].IsVisible = false;
                View[nameof(Model.BinaryPayload).ToFieldKey()].IsVisible = false;
            }

            if (!EntityHeader.IsNullOrEmpty(Model.Transport))
            {
                ShowErrorMessage = false;
                HasTransport = true;


                View[nameof(Model.TextPayload).ToFieldKey()].IsVisible = Model.PayloadType.Value == PaylodTypes.String;
                View[nameof(Model.BinaryPayload).ToFieldKey()].IsVisible = Model.PayloadType.Value == PaylodTypes.Binary;

                form.OptionSelected += Form_OptionSelected;
                form.DeleteItem += Form_DeleteItem;
                View[nameof(Model.Key).ToFieldKey()].IsUserEditable = IsCreate;
                form.AddViewCell(nameof(Model.Name));
                form.AddViewCell(nameof(Model.Key));

                /* At some point we may want to allow one simulator to target different transports/endpoint/ports 
                form.AddViewCell(nameof(Model.Transport));
                form.AddViewCell(nameof(Model.EndPoint));
                form.AddViewCell(nameof(Model.Port));
                */

                form.AddViewCell(nameof(Model.QualityOfServiceLevel));
                form.AddViewCell(nameof(Model.RetainFlag));
                form.AddViewCell(nameof(Model.To));
                form.AddViewCell(nameof(Model.MessageId));
                form.AddViewCell(nameof(Model.QueueName));
                form.AddViewCell(nameof(Model.Topic));
                form.AddViewCell(nameof(Model.AppendCR));
                form.AddViewCell(nameof(Model.AppendLF));
                form.AddViewCell(nameof(Model.PayloadType));
                form.AddViewCell(nameof(Model.ContentType));
                form.AddViewCell(nameof(Model.HttpVerb));
                form.AddViewCell(nameof(Model.TextPayload));
                form.AddViewCell(nameof(Model.BinaryPayload));
                form.AddViewCell(nameof(Model.PathAndQueryString));

                if (Model.Transport.Value == TransportTypes.RestHttp ||
                    Model.Transport.Value == TransportTypes.RestHttps)
                {
                    form.AddChildList<MessageHeaderViewModel>(nameof(Model.MessageHeaders), Model.MessageHeaders);
                }

                form.AddChildList<DynamicAttributeViewModel>(nameof(Model.DynamicAttributes), Model.DynamicAttributes);

                ModelToView(Model, form);

                HideAll();

                switch (Model.Transport.Value)
                {
                    case TransportTypes.AzureIoTHub: SetForAzureIoTHub(); break;
                    case TransportTypes.AzureEventHub: SetForAzureEventHub(); break;
                    case TransportTypes.MQTT: SetForMQTT(); break;
                    case TransportTypes.TCP: SetForTCP(); break;
                    case TransportTypes.UDP: SetForUDP(); break;
                    case TransportTypes.AzureServiceBus: SetForServiceBus(); break;
                    case TransportTypes.RestHttp:
                    case TransportTypes.RestHttps: SetForREST(); break;
                }
            }
            else
            {
                ShowErrorMessage = true;
                HasTransport = false;
            }
        }

        bool _hasTransport;
        public bool HasTransport
        {
            get { return _hasTransport; }
            set { Set(ref _hasTransport, value); }
        }

        bool _showErrorMessage;
        public bool ShowErrorMessage
        {
            get { return _showErrorMessage; }
            set { Set(ref _showErrorMessage, value); }
        }

        protected override string GetHelpLink()
        {
            if (Model != null && !EntityHeader.IsNullOrEmpty(Model.Transport))
            {
                switch (Model.Transport.Value)
                {
                    case TransportTypes.MQTT: return "http://support.nuviot.com/help.html#/Simulator/MQTT.md";
                    case TransportTypes.AzureEventHub: return "http://support.nuviot.com/help.html#/Simulator/AzureEventHub.md";
                    case TransportTypes.AzureServiceBus: return "http://support.nuviot.com/help.html#/Simulator/AzureServiceBus.md";
                    case TransportTypes.AzureIoTHub: return "http://support.nuviot.com/help.html#/Simulator/AzureIoTHub.md";
                    case TransportTypes.RestHttp:
                    case TransportTypes.RestHttps: return "http://support.nuviot.com/help.html#/Simulator/REST.md";
                    case TransportTypes.TCP: return "http://support.nuviot.com/help.html#/Simulator/TCP.md";
                    case TransportTypes.UDP: return "http://support.nuviot.com/help.html#/Simulator/UDP.md";
                }
            }

            return "http://support.nuviot.com/help.html#/Simulator/Index.md";
        }

        protected override string GetRequestUri()
        {
            return "/api/simulator/messagetemplate/factory";
        }

        private void Form_DeleteItem(object sender, DeleteItemEventArgs e)
        {
            if (e.Type == nameof(Model.MessageHeaders))
            {
                var hdr = Model.MessageHeaders.Where(itm => itm.Id == e.Id).FirstOrDefault();
                Model.MessageHeaders.Remove(hdr);
            }

            if (e.Type == nameof(Model.DynamicAttributes))
            {
                var attr = Model.DynamicAttributes.Where(itm => itm.Id == e.Id).FirstOrDefault();
                Model.DynamicAttributes.Remove(attr);
            }
        }

        private void HideAll()
        {
            View[nameof(Model.QueueName).ToFieldKey()].IsVisible = false;
            View[nameof(Model.HttpVerb).ToFieldKey()].IsVisible = false;
            View[nameof(Model.ContentType).ToFieldKey()].IsVisible = false;
            View[nameof(Model.To).ToFieldKey()].IsVisible = false;
            View[nameof(Model.Topic).ToFieldKey()].IsVisible = false;
            View[nameof(Model.MessageId).ToFieldKey()].IsVisible = false;
            View[nameof(Model.RetainFlag).ToFieldKey()].IsVisible = false;
            View[nameof(Model.QualityOfServiceLevel).ToFieldKey()].IsVisible = false;
            View[nameof(Model.PathAndQueryString).ToFieldKey()].IsVisible = false;
            View[nameof(Model.ContentType).ToFieldKey()].IsVisible = false;
            View[nameof(Model.AppendLF).ToFieldKey()].IsVisible = false;
            View[nameof(Model.AppendCR).ToFieldKey()].IsVisible = false;
        }

        private void SetForAzureIoTHub()
        {
            View[nameof(Model.PayloadType).ToFieldKey()].IsVisible = true;
            View[nameof(Model.Topic).ToFieldKey()].IsVisible = true;
            View[nameof(Model.AppendCR).ToFieldKey()].IsVisible = true;
            View[nameof(Model.AppendLF).ToFieldKey()].IsVisible = true;
        }

        private void SetForAzureEventHub()
        {
            View[nameof(Model.PayloadType).ToFieldKey()].IsVisible = true;
            View[nameof(Model.AppendCR).ToFieldKey()].IsVisible = true;
            View[nameof(Model.AppendLF).ToFieldKey()].IsVisible = true;
        }

        private void SetForMQTT()
        {
            View[nameof(Model.PayloadType).ToFieldKey()].IsVisible = true;
            View[nameof(Model.Topic).ToFieldKey()].IsVisible = true;
            View[nameof(Model.RetainFlag).ToFieldKey()].IsVisible = true;
            View[nameof(Model.QueueName).ToFieldKey()].IsVisible = true;
            View[nameof(Model.QualityOfServiceLevel).ToFieldKey()].IsVisible = true;
        }

        private void SetForREST()
        {
            View[nameof(Model.ContentType).ToFieldKey()].IsVisible = true;
            View[nameof(Model.HttpVerb).ToFieldKey()].IsVisible = true;
            View[nameof(Model.PathAndQueryString).ToFieldKey()].IsVisible = true;
        }

        private void SetForServiceBus()
        {
            View[nameof(Model.MessageId).ToFieldKey()].IsVisible = true;
            View[nameof(Model.To).ToFieldKey()].IsVisible = true;
            View[nameof(Model.QueueName).ToFieldKey()].IsVisible = true;
            View[nameof(Model.ContentType).ToFieldKey()].IsVisible = true;
            View[nameof(Model.QueueName).ToFieldKey()].IsVisible = true;
        }

        private void SetForTCP()
        {
            View[nameof(Model.AppendCR).ToFieldKey()].IsVisible = true;
            View[nameof(Model.AppendLF).ToFieldKey()].IsVisible = true;
            View[nameof(Model.PayloadType).ToFieldKey()].IsVisible = true;
        }

        private void SetForUDP()
        {
            View[nameof(Model.AppendCR).ToFieldKey()].IsVisible = true;
            View[nameof(Model.AppendLF).ToFieldKey()].IsVisible = true;
            View[nameof(Model.PayloadType).ToFieldKey()].IsVisible = true;
        }

        private void Form_OptionSelected(object sender, OptionSelectedEventArgs e)
        {
            if (e.Key == nameof(Model.PayloadType))
            {
                if (e.Value == MessageTemplate.PayloadTypes_Binary)
                {
                    FormAdapter.HideView(nameof(Model.TextPayload));
                    FormAdapter.ShowView(nameof(Model.BinaryPayload));
                }
                else if (e.Value == MessageTemplate.PayloadTypes_Text)
                {
                    FormAdapter.ShowView(nameof(Model.TextPayload));
                    FormAdapter.HideView(nameof(Model.BinaryPayload));
                }
                else
                {
                    FormAdapter.HideView(nameof(Model.TextPayload));
                    FormAdapter.HideView(nameof(Model.BinaryPayload));
                }
            }

            if (e.Key == nameof(Model.Transport))
            {
                if (e.Value == LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_RestHttp)
                {
                    SetForREST();
                }
            }
        }
    }
}