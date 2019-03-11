using LagoVista.Client.Core.Auth;
using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using LagoVista.Simulator.Core.Resources;
using LagoVista.Simulator.Core.ViewModels.Messages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.Simulator
{
    public class SimulatorEditorViewModel : FormViewModelBase<IoT.Simulator.Admin.Models.Simulator>
    {
        ISecureStorage _secureStorage;

        const string EDIT_PASSWORD_CONTROL = "EditPassword";
        const string EDIT_ACCESSKEY_CONTROL = "EditAccessKey";

        public SimulatorEditorViewModel(ISecureStorage secureStorage)
        {
            _secureStorage = secureStorage;
            EditPasswordCommand = new RelayCommand(EditPassword);
            EditAccessKeyCommand = new RelayCommand(EditAccessKey);
        }

        public async override Task<InvokeResult> SaveRecordAsync()
        {
            if (IsCreate)
            {
                if (LaunchArgs.ParentViewModel is MainViewModel parent)
                {
                    if (parent.ListItems.Where(sim => sim.Key == this.Model.Key).Any())
                    {
                        await this.Popups.ShowAsync(ClientResources.Common_KeyInUse);
                        return InvokeResult.FromErrors(ClientResources.Common_KeyInUse.ToErrorMessage());
                    }
                }
            }

            /* Will only be locked for Android, iOS/UWP just returns "true" */
            if (!SecureStorage.IsUnlocked &&
                !EntityHeader.IsNullOrEmpty(Model.CredentialStorage) &&
                Model.CredentialStorage.Value == CredentialsStorage.OnDevice)
            {
                if (SecureStorage.IsSetup)
                {
                    await ViewModelNavigation.NavigateAsync<UnlockStorageViewModel>(this);
                }
                else
                {
                    await ViewModelNavigation.NavigateAsync<SetStoragePasswordViewModel>(this);
                }

                return InvokeResult.FromErrors(Resources.SimulatorCoreResources.SimulatorEdit_UnlockRequired.ToErrorMessage());
            }

            var validationResult = Validator.Validate(this.Model);
            if (!validationResult.Successful)
            {
                await this.ShowValidationErrorsAsync(validationResult);
                return InvokeResult.FromError("Please correct errors");
            }

            return await PerformNetworkOperation(async () =>
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
                                case TransportTypes.RestHttps:
                                case TransportTypes.MQTT:
                                    if (!this.Model.Anonymous)
                                    {
                                        _secureStorage.Store(this.Model.Id, this.Model.Password);
                                    }

                                    break;
                                case TransportTypes.AzureIoTHub:
                                case TransportTypes.AzureServiceBus:
                                case TransportTypes.AzureEventHub:
                                    _secureStorage.Store(this.Model.Id, this.Model.AccessKey);
                                    break;
                            }

                            /* If we store locally or on the device set to null so they won't be sent to the server */
                            this.Model.Password = null;
                            this.Model.AccessKey = null;
                            break;
                        case CredentialsStorage.Prompt:
                            /* If we prompt the user each time for the access key password, set to null so won't send to server */
                            this.Model.Password = null;
                            this.Model.AccessKey = null;

                            break;

                    }
                }

                if (LaunchArgs.LaunchType == LaunchTypes.Create)
                {
                    return await FormRestClient.AddAsync("/api/simulator", this.Model);
                }
                else
                {
                    return await FormRestClient.UpdateAsync("/api/simulator", this.Model);
                }
            });
        }

        public void EditPassword(object obj)
        {
            FormAdapter.ShowView(nameof(Model.Password));
            FormAdapter.HideView(EDIT_PASSWORD_CONTROL);
        }

        public RelayCommand EditPasswordCommand { get; private set; }


        public void EditAccessKey(object obj)
        {
            FormAdapter.ShowView(nameof(Model.AccessKey));
            FormAdapter.HideView(EDIT_ACCESSKEY_CONTROL);
        }

        public RelayCommand EditAccessKeyCommand { get; private set; }

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

            var frmEditPasswordLink = FormField.Create(EDIT_PASSWORD_CONTROL, new LagoVista.Core.Attributes.FormFieldAttribute(FieldType: LagoVista.Core.Attributes.FieldTypes.LinkButton));
            frmEditPasswordLink.Label = SimulatorCoreResources.SimulatorEdit_EditPassword;
            frmEditPasswordLink.Name = EDIT_PASSWORD_CONTROL.ToFieldKey();
            frmEditPasswordLink.Watermark = SimulatorCoreResources.SimulatorEdit_EditPassword_Link;
            frmEditPasswordLink.Command = EditPasswordCommand;
            View.Add(EDIT_PASSWORD_CONTROL.ToFieldKey(), frmEditPasswordLink);

            var frmEditAccessKey = FormField.Create(EDIT_PASSWORD_CONTROL, new LagoVista.Core.Attributes.FormFieldAttribute(FieldType: LagoVista.Core.Attributes.FieldTypes.LinkButton));
            frmEditAccessKey.Label = SimulatorCoreResources.SimulatorEdit_EditAccessKey;
            frmEditAccessKey.Name = EDIT_ACCESSKEY_CONTROL.ToFieldKey();
            frmEditAccessKey.Watermark = SimulatorCoreResources.SimulatorEdit_EditAccesKey_Link;
            frmEditAccessKey.Command = EditAccessKeyCommand;
            View.Add(EDIT_ACCESSKEY_CONTROL.ToFieldKey(), frmEditAccessKey);

            form.AddViewCell(nameof(Model.Name));
            form.AddViewCell(nameof(Model.Key));
            form.AddViewCell(nameof(Model.DefaultTransport));
            form.AddViewCell(nameof(Model.DefaultEndPoint));
            form.AddViewCell(nameof(Model.DefaultPort));
            form.AddViewCell(nameof(Model.DeviceId));

            form.AddViewCell(nameof(Model.Anonymous));
            form.AddViewCell(nameof(Model.BasicAuth));

            form.AddViewCell(nameof(Model.UserName));
            form.AddViewCell(nameof(Model.CredentialStorage));
            form.AddViewCell(nameof(Model.Password));
            form.AddViewCell(EDIT_PASSWORD_CONTROL);
            form.AddViewCell(nameof(Model.AccessKeyName));
            form.AddViewCell(EDIT_ACCESSKEY_CONTROL);
            form.AddViewCell(nameof(Model.AccessKey));
            form.AddViewCell(nameof(Model.HubName));
            form.AddViewCell(nameof(Model.QueueName));
            form.AddViewCell(nameof(Model.Subscription));
            form.AddViewCell(nameof(Model.DefaultPayloadType));
            form.AddViewCell(nameof(Model.Description));
            form.AddChildList<MessageEditorViewModel>(nameof(Model.MessageTemplates), Model.MessageTemplates);
            form.AddChildList<SimulatorStateViewModel>(nameof(Model.SimulatorStates), Model.SimulatorStates);
            ModelToView(Model, form);
            FormAdapter = form;
        }

        public async override Task InitAsync()
        {
            await base.InitAsync();

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
            _currentTransport = transportType;
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
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_MQTT: return "http://support.nuviot.com/help.html#/simulator/mqtt";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_Azure_EventHub: return "http://support.nuviot.com/help.html#/simulator/azureeventhub";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_AzureServiceBus: return "http://support.nuviot.com/help.html#/simulator/azureservicebus";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_IOT_HUB: return "http://support.nuviot.com/help.html#/simulator/azureiothub";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_RestHttp:
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_RestHttps: return "http://support.nuviot.com/help.html#/simulator/rest";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_TCP: return "http://support.nuviot.com/help.html#/simulator/tcp";
                    case LagoVista.IoT.Simulator.Admin.Models.Simulator.Transport_UDP: return "http://support.nuviot.com/help.html#/simulator/udp";
                }
            }

            return "http://support.nuviot.com/help.html#/Simulator/Index.md";
        }

        TransportTypes? _currentTransport;

        protected override void OptionSelected(string name, string value)
        {
            if (value != null)
            {
                if (name == nameof(Model.DefaultTransport))
                {
                    ShowFieldsForTransport((TransportTypes)Enum.Parse(typeof(TransportTypes), value, true));
                }
                else if (name == nameof(Model.Anonymous))
                {
                    if (value == "true")
                    {
                        HideRow(nameof(Model.UserName));
                        HideRow(nameof(Model.Password));
                        HideRow(nameof(Model.CredentialStorage));
                        HideRow(nameof(Model.BasicAuth));
                        FormAdapter.HideView(EDIT_PASSWORD_CONTROL);
                        HideRow(nameof(Model.Password));

                    }
                    else
                    {
                        ShowRow(nameof(Model.UserName));

                        if (IsEdit)
                        {
                            FormAdapter.ShowView(EDIT_PASSWORD_CONTROL);
                        }
                        else
                        {
                            ShowRow(nameof(Model.Password));
                        }

                        ShowRow(nameof(Model.CredentialStorage));

                        if (_currentTransport.Value == TransportTypes.RestHttp ||
                            _currentTransport.Value == TransportTypes.RestHttps)
                        {
                            ShowRow(nameof(Model.BasicAuth));
                        }
                    }
                }
                else if (name == nameof(Model.CredentialStorage))
                {
                    switch (value)
                    {
                        case IoT.Simulator.Admin.Models.Simulator.CredentialsStorage_InCloud:
                        case IoT.Simulator.Admin.Models.Simulator.CredentialsStorage_OnDevice:
                            if (_currentTransport.Value == TransportTypes.RestHttp ||
                                _currentTransport.Value == TransportTypes.MQTT ||
                                _currentTransport.Value == TransportTypes.AMQP ||
                                _currentTransport.Value == TransportTypes.RestHttps)
                            {
                                if (IsEdit)
                                {
                                    FormAdapter.ShowView(EDIT_PASSWORD_CONTROL);                                    
                                }
                                else
                                {
                                    ShowRow(nameof(Model.Password));
                                }
                            }
                            else if (_currentTransport.Value == TransportTypes.AzureEventHub ||
                                     _currentTransport.Value == TransportTypes.AzureIoTHub ||
                                     _currentTransport.Value == TransportTypes.AzureServiceBus)
                            {
                                if (IsEdit)
                                {
                                    FormAdapter.ShowView(EDIT_ACCESSKEY_CONTROL);                                    
                                }
                                else
                                {
                                    ShowRow(nameof(Model.AccessKey));
                                }
                            }

                            break;
                        case IoT.Simulator.Admin.Models.Simulator.CredentialsStorage_Prompt:
                            HideRow(nameof(Model.Password));
                            FormAdapter.HideView(EDIT_PASSWORD_CONTROL);
                            HideRow(nameof(Model.AccessKey));
                            FormAdapter.HideView(EDIT_ACCESSKEY_CONTROL);

                            break;
                    }
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
            HideRow(nameof(Model.DefaultEndPoint));
            HideRow(nameof(Model.UserName));
            HideRow(nameof(Model.Password));
            HideRow(nameof(Model.QueueName));
            HideRow(nameof(Model.AccessKeyName));
            HideRow(nameof(Model.AccessKey));
            HideRow(nameof(Model.Subscription));
            HideRow(nameof(Model.CredentialStorage));
            HideRow(EDIT_ACCESSKEY_CONTROL);
            HideRow(EDIT_PASSWORD_CONTROL);

            HideRow(nameof(Model.Anonymous));
            HideRow(nameof(Model.BasicAuth));
        }

        private void SetForAzureServiceBus()
        {
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.AccessKeyName));
            ShowRow(nameof(Model.QueueName));
            ShowRow(nameof(Model.CredentialStorage));

            if (LaunchArgs.LaunchType == LaunchTypes.Edit)
            {
                ShowRow(EDIT_ACCESSKEY_CONTROL);
                HideRow(nameof(Model.AccessKey));
            }
            else
            {
                ShowRow(nameof(Model.AccessKey));
                HideRow(EDIT_ACCESSKEY_CONTROL);
            }
        }

        private void SetForAzureEventHub()
        {
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.AccessKeyName));
            ShowRow(nameof(Model.HubName));
            ShowRow(nameof(Model.CredentialStorage));

            if (LaunchArgs.LaunchType == LaunchTypes.Edit)
            {
                ShowRow(EDIT_ACCESSKEY_CONTROL);
                HideRow(nameof(Model.AccessKey));
            }
            else
            {
                ShowRow(nameof(Model.AccessKey));
                HideRow(EDIT_ACCESSKEY_CONTROL);
            }
        }

        private void SetForIoTHub()
        {
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.CredentialStorage));

            if (LaunchArgs.LaunchType == LaunchTypes.Edit)
            {
                ShowRow(EDIT_ACCESSKEY_CONTROL);
                HideRow(nameof(Model.AccessKey));
            }
            else
            {
                ShowRow(nameof(Model.AccessKey));
                HideRow(EDIT_ACCESSKEY_CONTROL);
            }
        }

        private void SetForMQTT()
        {
            SetValue(nameof(Model.DefaultPort), 1883.ToString());

            ShowRow(nameof(Model.DefaultPort));
            ShowRow(nameof(Model.DefaultEndPoint));
            ShowRow(nameof(Model.DefaultPayloadType));
            ShowRow(nameof(Model.Subscription));
            ShowRow(nameof(Model.Anonymous));

            if (Model.Anonymous)
            {
                HideRow(nameof(Model.UserName));
                HideRow(nameof(Model.Password));
                HideRow(nameof(Model.CredentialStorage));
                HideRow(EDIT_PASSWORD_CONTROL);
            }
            else
            {
                ShowRow(nameof(Model.UserName));

                ShowRow(nameof(Model.CredentialStorage));

                if (LaunchArgs.LaunchType == LaunchTypes.Edit)
                {
                    ShowRow(EDIT_PASSWORD_CONTROL);
                    HideRow(nameof(Model.Password));
                }
                else
                {
                    ShowRow(nameof(Model.Password));
                    HideRow(EDIT_PASSWORD_CONTROL);
                }
            }
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

            if (Model.Anonymous)
            {
                HideRow(nameof(Model.UserName));
                HideRow(nameof(Model.Password));
                HideRow(nameof(Model.CredentialStorage));
                HideRow(EDIT_PASSWORD_CONTROL);
            }
            else
            {
                ShowRow(nameof(Model.UserName));
                ShowRow(nameof(Model.CredentialStorage));

                if (LaunchArgs.LaunchType == LaunchTypes.Edit)
                {
                    HideRow(nameof(Model.Password));
                    ShowRow(EDIT_PASSWORD_CONTROL);

                }
                else
                {
                    ShowRow(nameof(Model.Password));
                    HideRow(EDIT_PASSWORD_CONTROL);
                }
            }
        }
    }
}