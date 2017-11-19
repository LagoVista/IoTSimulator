using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core;
using LagoVista.Core.ViewModels;
using LagoVista.Simulator.Core.ViewModels.Messages;
using LagoVista.Core.Validation;
using LagoVista.IoT.Simulator.Admin.Models;
using LagoVista.Client.Core.ViewModels;
using System;
using System.Threading.Tasks;
using System.Linq;
using LagoVista.Client.Core.Resources;
using LagoVista.Core.Models;
using LagoVista.Client.Core.Auth;

namespace LagoVista.Simulator.Core.ViewModels.Simulator
{
    public class SimulatorEditorViewModel : FormViewModelBase<IoT.Simulator.Admin.Models.Simulator>
    {
        ISecureStorage _secureStorage;

        public SimulatorEditorViewModel(ISecureStorage secureStorage)
        {
            _secureStorage = secureStorage;
        }

        public async override Task<InvokeResult> SaveRecordAsync()
        {
            if (IsCreate)
            {
                var parent = LaunchArgs.ParentViewModel as MainViewModel;
                if (parent != null)
                {
                    if (parent.ListItems.Where(sim => sim.Key == this.Model.Key).Any())
                    {
                        await this.Popups.ShowAsync(ClientResources.Common_KeyInUse);
                        return InvokeResult.FromErrors(ClientResources.Common_KeyInUse.ToErrorMessage());
                    }
                }
            }

            return await PerformNetworkOperation(() =>
            {
                if (!EntityHeader.IsNullOrEmpty(this.Model.CredentialStorage))
                {
                    switch (this.Model.CredentialStorage.Value)
                    {
                        case CredentialsStorage.InCloud: /* NOP */ break;
                        case CredentialsStorage.OnDevice:
                            switch (this.Model.DefaultTransport.Value)
                            {
                                case TransportTypes.RestHttp:
                                    if (!this.Model.Anonymous) _secureStorage.Store(this.Model.Id, this.Model.Password);
                                    break;
                                case TransportTypes.MQTT:
                                    if (!this.Model.Anonymous) _secureStorage.Store(this.Model.Id, this.Model.Password);
                                    break;
                                case TransportTypes.AzureIoTHub:
                                case TransportTypes.AzureServiceBus:
                                case TransportTypes.AzureEventHub:
                                    _secureStorage.Store(this.Model.Id, this.Model.AccessKey);
                                    break;
                            }

                            this.Model.Password = null;
                            this.Model.AccessKey = null;
                            break;
                        case CredentialsStorage.Prompt:
                            this.Model.Password = null;
                            this.Model.AccessKey = null;

                            break;
                            
                    }
                }

                if (LaunchArgs.LaunchType == LaunchTypes.Create)
                {
                    return FormRestClient.AddAsync("/api/simulator", this.Model);
                }
                else
                {
                    return FormRestClient.UpdateAsync("/api/simulator", this.Model);
                }
            });
        }

        public override bool CanSave()
        {
            return true;
        }

        protected override string GetRequestUri()
        {
            return this.LaunchArgs.LaunchType == LaunchTypes.Edit ? $"/api/simulator/{LaunchArgs.ChildId}" : "/api/simulator/factory";
        }

        protected override void BuildForm(EditFormAdapter form)
        {
            View[nameof(Model.Key).ToFieldKey()].IsUserEditable = LaunchArgs.LaunchType == LaunchTypes.Create;
            View[nameof(Model.DefaultTransport).ToFieldKey()].IsEnabled = LaunchArgs.LaunchType == LaunchTypes.Create;
            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.Key));
            form.AddViewCell(nameof(Model.DefaultTransport));
            form.AddViewCell(nameof(Model.DefaultEndPoint));
            //            form.AddViewCell(nameof(Model.TLSSSL));
            form.AddViewCell(nameof(Model.DefaultPort));
            form.AddViewCell(nameof(Model.DeviceId));
            form.AddViewCell(nameof(Model.Anonymous));
            form.AddViewCell(nameof(Model.BasicAuth));
            form.AddViewCell(nameof(Model.UserName));
            form.AddViewCell(nameof(Model.CredentialStorage));
            form.AddViewCell(nameof(Model.Password));
            form.AddViewCell(nameof(Model.AccessKeyName));
            form.AddViewCell(nameof(Model.AccessKey));
            form.AddViewCell(nameof(Model.HubName));
            form.AddViewCell(nameof(Model.QueueName));
            form.AddViewCell(nameof(Model.Subscription));
            form.AddViewCell(nameof(Model.DefaultPayloadType));
            form.AddViewCell(nameof(Model.Description));
            form.AddChildList<MessageEditorViewModel>(nameof(Model.MessageTemplates), Model.MessageTemplates);
            ModelToView(Model, form);
            FormAdapter = form;

            if (Model.DefaultTransport != null)
            {
                ShowFieldsForTransport(Model.DefaultTransport.Value);
            }
            else
            {
                HideAll();
                this.HideRow(nameof(Model.MessageTemplates));
            }
        }

        private void ShowFieldsForTransport(TransportTypes transportType)
        {
            HideAll();
            switch (transportType)
            {
                case TransportTypes.MQTT: SetForMQTT(); break;
                case TransportTypes.TCP: SetForTCP(); break;
                case TransportTypes.UDP: SetForUDP(); break;
                case TransportTypes.RestHttp:
                case TransportTypes.RestHttps: SetForREST(transportType); break;
                case TransportTypes.AzureEventHub: SetForAzureEventHub(); break;
                case TransportTypes.AzureIoTHub: SetForIoTHub(); break;
                case TransportTypes.AzureServiceBus: SetForAzureServiceBus(); break;
                default: HideAll(); break;
            }
        }

        protected override string GetHelpLink()
        {
            if (View != null && View.ContainsKey(nameof(Model.DefaultTransport).ToFieldKey()))
            {
                switch (View[nameof(Model.DefaultTransport).ToFieldKey()].Value)
                {
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_MQTT: return "http://support.nuviot.com/help.html#/Simulator/MQTT.md";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_Azure_EventHub: return "http://support.nuviot.com/help.html#/Simulator/AzureEventHub.md";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_AzureServiceBus: return "http://support.nuviot.com/help.html#/Simulator/AzureServiceBus.md";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_IOT_HUB: return "http://support.nuviot.com/help.html#/Simulator/AzureIoTHub.md";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_RestHttp:
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_RestHttps: return "http://support.nuviot.com/help.html#/Simulator/REST.md";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_TCP: return "http://support.nuviot.com/help.html#/Simulator/TCP.md";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_UDP: return "http://support.nuviot.com/help.html#/Simulator/UDP.md";
                }
            }

            return "http://support.nuviot.com/help.html#/Simulator/Index.md";
        }

        protected override void OptionSelected(string name, string value)
        {
            if (value != null)
            {
                if (name == nameof(Model.DefaultTransport))
                {
                    ShowFieldsForTransport((TransportTypes)Enum.Parse(typeof(TransportTypes), value, true));
                }
            }
            else
            {
                HideAll();
            }
        }

        private void HideAll()
        {
            HideRow(nameof(Model.DefaultPayloadType));
            HideRow(nameof(Model.HubName));
            HideRow(nameof(Model.DefaultPort));
            //HideRow(nameof(Model.TLSSSL));
            HideRow(nameof(Model.DefaultEndPoint));
            HideRow(nameof(Model.UserName));
            HideRow(nameof(Model.Anonymous));
            HideRow(nameof(Model.BasicAuth));
            HideRow(nameof(Model.Password));
            HideRow(nameof(Model.QueueName));
            HideRow(nameof(Model.AccessKeyName));
            HideRow(nameof(Model.AccessKey));
            HideRow(nameof(Model.Subscription));
            HideRow(nameof(Model.CredentialStorage));
        }

        private void SetForAzureServiceBus()
        {
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.AccessKeyName));
            ShowRow(nameof(Model.AccessKey));
            ShowRow(nameof(Model.QueueName));
            ShowRow(nameof(Model.CredentialStorage));
        }

        private void SetForAzureEventHub()
        {
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.AccessKeyName));
            ShowRow(nameof(Model.AccessKey));
            ShowRow(nameof(Model.HubName));
            ShowRow(nameof(Model.CredentialStorage));
        }

        private void SetForIoTHub()
        {
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.AccessKey));
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.CredentialStorage));
        }

        private void SetForMQTT()
        {
            SetValue(nameof(Model.DefaultPort), 1883.ToString());

            ShowRow(nameof(Model.DefaultPort));
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.Subscription));
            ShowRow(nameof(Model.UserName));
            ShowRow(nameof(Model.Password));
            ShowRow(nameof(Model.Anonymous));
            ShowRow(nameof(Model.CredentialStorage));
        }

        private void SetForTCP()
        {
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.DefaultPort));

        }

        private void SetForUDP()
        {
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.DefaultPort));
        }

        private void SetForREST(TransportTypes transportType)
        {
            SetValue(nameof(Model.DefaultPort), transportType == TransportTypes.RestHttp ? 80.ToString() : 443.ToString());
            SetValue(nameof(Model.DefaultPayloadType), "text");
            ShowRow(nameof(Model.Anonymous));
            ShowRow(nameof(Model.BasicAuth));

            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.DefaultPort));
            ShowRow(nameof(Model.UserName));
            ShowRow(nameof(Model.Password));
            ShowRow(nameof(Model.CredentialStorage));
        }
    }
}