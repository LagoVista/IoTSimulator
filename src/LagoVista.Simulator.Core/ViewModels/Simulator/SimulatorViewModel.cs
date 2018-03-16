//#define IOTHUB
//#define SERVICEBUS
#define EVENTHUB

using LagoVista.Client.Core;
using LagoVista.Client.Core.Auth;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.Networking.Models;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using LagoVista.Simulator.Core.Models;
#if IOTHUB
using Microsoft.Azure.Devices.Client;
#endif

#if EVENTHUB
using Microsoft.Azure.EventHubs;
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.Simulator
{
    public class SimulatorViewModel : AppViewModelBase
    {
#region Fields
        IMQTTDeviceClient _mqttClient;
        ITCPClient _tcpClient;
        IUDPClient _udpClient;
        ISecureStorage _secureStorage;
#if IOTHUB
        DeviceClient _azureIoTHubClient;
#endif
        bool _isConnected;
        bool _isEditing;

        //Task ReceivingTask;
#endregion

#region Constructor
        public SimulatorViewModel(ISecureStorage secureStorage)
        {
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            ShowMessageTemplatesCommand = new RelayCommand(ShowMessageTemplates);
            ShowReceivedMessagesCommand = new RelayCommand(ShowReceivedMessages);
            ReceivedMessageList = new ObservableCollection<ReceivedMessage>();
            _secureStorage = secureStorage;
        }
#endregion

#region Utility Methods
        private bool HasPersistentConnection()
        {
            switch (Model.DefaultTransport.Value)
            {
                case TransportTypes.TCP:
                case TransportTypes.UDP:
                //case TransportTypes.AMQP:
                case TransportTypes.AzureIoTHub:
                //case TransportTypes.RabbitMQ:
                case TransportTypes.MQTT: return true;
                default: return false;
            }
        }

        private void SetConnectedState()
        {
            _isConnected = true;
            ViewSelectorVisible = true;
            ConnectionColor = "green";
            DisconnectButtonVisible = true;
            ConnectButtonVisible = false;
            ShowMessageTemplates();
        }

        private void SetDisconnectedState()
        {
            _isConnected = false;
            ViewSelectorVisible = false;
            ConnectionColor = "red";
            DisconnectButtonVisible = false;
            ConnectButtonVisible = true;
            ReceivedMessagesVisible = false;
            MessageTemplatesVisible = false;
        }
#endregion

#region View Life Cycle
        private async Task<InvokeResult> LoadSimulator()
        {
            var simulatorResponse = await RestClient.GetAsync<DetailResponse<LagoVista.IoT.Simulator.Admin.Models.Simulator>>($"/api/simulator/{LaunchArgs.ChildId}");
            if (simulatorResponse.Successful)
            {
                Model = simulatorResponse.Result.Model;
                MessageTemplates = simulatorResponse.Result.Model.MessageTemplates;

                ConnectionIconVisible = HasPersistentConnection();
                ConnectButtonVisible = HasPersistentConnection();
                MessageTemplatesVisible = !HasPersistentConnection();
                DisconnectButtonVisible = false;

                PortVisible = Model.DefaultTransport.Value != TransportTypes.AzureEventHub && Model.DefaultTransport.Value != TransportTypes.AzureServiceBus && Model.DefaultTransport.Value != TransportTypes.AzureIoTHub;

                return InvokeResult.Success;
            }
            else
            {
                return simulatorResponse.ToInvokeResult();
            }
        }

        public async void EditSimulator()
        {
            if (_isConnected)
            {
                await DisconnectAsync();
            }
            await ViewModelNavigation.NavigateAndEditAsync<SimulatorEditorViewModel>(this, Model.Id);
            _isEditing = true;
        }

        public override async Task<bool> CanCancelAsync()
        {
            if (_isConnected)
            {
                await DisconnectAsync();
            }

            return true;
        }

        public async override void Edit()
        {
            if (_isConnected)
            {
                if (await Popups.ConfirmAsync("IoT Simulator", Resources.SimulatorCoreResources.Simulator_EditDisconnect))
                {
                    EditSimulator();
                }
            }
            else
            {
                EditSimulator();
            }
        }

        public override async Task InitAsync()
        {
            var result = await PerformNetworkOperation(LoadSimulator);
            if (!result.Successful)
            {
                await this.ViewModelNavigation.GoBackAsync();
                return;
            }

            if (!EntityHeader.IsNullOrEmpty(this.Model.CredentialStorage))
            {
                switch (this.Model.CredentialStorage.Value)
                {
                    case CredentialsStorage.OnDevice:
                        {
                            switch (this.Model.DefaultTransport.Value)
                            {
                                case TransportTypes.AzureIoTHub:
                                case TransportTypes.AzureServiceBus:
                                case TransportTypes.AzureEventHub:
                                    if (SecureStorage.IsUnlocked)
                                    {
                                        if (_secureStorage.Contains(this.Model.Id))
                                        {
                                            this.Model.AccessKey = _secureStorage.Retrieve(this.Model.Id);
                                        }
                                        else
                                        {
                                            await Popups.ShowAsync(Resources.SimulatorCoreResources.PasswordEntry_NotFound);
                                            PromptForAccessKey();
                                        }
                                    }
                                    else
                                    {
                                        UnlockStorageAndGetAccessKey();
                                    }
                                    break;
                                case TransportTypes.RestHttp:
                                case TransportTypes.RestHttps:
                                case TransportTypes.MQTT:
                                    if (!this.Model.Anonymous)
                                    {
                                        if (SecureStorage.IsUnlocked)
                                        {
                                            if (SecureStorage.Contains(this.Model.Id))
                                            {
                                                this.Model.Password = _secureStorage.Retrieve(this.Model.Id);
                                            }
                                            else
                                            {
                                                await Popups.ShowAsync(Resources.SimulatorCoreResources.PasswordEntry_NotFound);
                                                PromptForPassword();
                                            }
                                        }
                                        else
                                        {
                                            UnlockStorageAndGetpassword();
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    case CredentialsStorage.Prompt:
                        {
                            switch (this.Model.DefaultTransport.Value)
                            {
                                case TransportTypes.AzureIoTHub:
                                case TransportTypes.AzureServiceBus:
                                case TransportTypes.AzureEventHub:
                                    PromptForAccessKey();
                                    break;
                                case TransportTypes.RestHttp:
                                case TransportTypes.RestHttps:
                                case TransportTypes.MQTT:
                                    PromptForPassword();
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        public override async Task ReloadedAsync()
        {
            /* If we are editing, reload the simulator, otherwise don't bother, we are coming back from sending a message */
            if (_isEditing)
            {
                /* If prompted for creds, make sure they are restored after reloading the sim after editing 
                   if they are on the server, it will grab them, if they are stored localliy it will grab them. */
                String pwd = null, accessKey = null;
                if (!EntityHeader.IsNullOrEmpty(this.Model.CredentialStorage) && this.Model.CredentialStorage.Value == CredentialsStorage.Prompt)
                {
                    pwd = this.Model.Password;
                    accessKey = this.Model.AccessKey;
                }

                await InitAsync();

                if (!EntityHeader.IsNullOrEmpty(this.Model.CredentialStorage) && this.Model.CredentialStorage.Value == CredentialsStorage.Prompt)
                {
                    this.Model.Password = pwd;
                    this.Model.AccessKey = accessKey;
                }


                _isEditing = false;
            }
        }
#endregion

#region Added security for simulators.
        public async void UnlockStorageAndGetpassword()
        {
            if (!SecureStorage.IsSetup)
            {
                await ViewModelNavigation.NavigateAndPickAsync<SetStoragePasswordViewModel>(this, StorageUnlocked, CancelCredentialsEntry);
            }
            else
            {
                await ViewModelNavigation.NavigateAndPickAsync<UnlockStorageViewModel>(this, StorageUnlocked, CancelCredentialsEntry);
            }
        }

        public async void UnlockStorageAndGetAccessKey()
        {
            if (!SecureStorage.IsSetup)
            {
                await ViewModelNavigation.NavigateAndPickAsync<SetStoragePasswordViewModel>(this, StorageUnlocked, CancelCredentialsEntry);
            }
            else
            {
                await ViewModelNavigation.NavigateAndPickAsync<UnlockStorageViewModel>(this, StorageUnlocked, CancelCredentialsEntry);
            }
        }

        public void StorageUnlocked(object ob)
        {
            if (Model.DefaultTransport.Value == TransportTypes.AzureEventHub ||
                Model.DefaultTransport.Value == TransportTypes.AzureIoTHub ||
                Model.DefaultTransport.Value == TransportTypes.AzureServiceBus)
            {
                if (SecureStorage.Contains(Model.Id))
                {
                    Model.AccessKey = SecureStorage.Retrieve(Model.Id);
                }
                else
                {
                    PromptForAccessKey();
                }
            }
            else if (Model.DefaultTransport.Value == TransportTypes.RestHttp ||
                    Model.DefaultTransport.Value == TransportTypes.RestHttps ||
                    Model.DefaultTransport.Value == TransportTypes.MQTT)
            {
                if (SecureStorage.Contains(Model.Id))
                {
                    Model.Password = SecureStorage.Retrieve(Model.Id);
                }
                else
                {
                    PromptForPassword();
                }
            }
        }

        public async void PromptForPassword()
        {
            await ViewModelNavigation.NavigateAndPickAsync<PasswordEntryViewModel>(this, SetPassword, CancelCredentialsEntry);
        }

        public async void PromptForAccessKey()
        {

            await ViewModelNavigation.NavigateAndPickAsync<PasswordEntryViewModel>(this, SetAccessKey, CancelCredentialsEntry);
        }

        public void SetAccessKey(object password)
        {
            this.Model.AccessKey = password.ToString();
            if (this.Model.CredentialStorage.Value == CredentialsStorage.OnDevice)
            {
                SecureStorage.Store(Model.Id, Model.AccessKey);
            }
        }

        public void SetPassword(object password)
        {
            this.Model.Password = password.ToString();
            if(this.Model.CredentialStorage.Value == CredentialsStorage.OnDevice)
            {
                SecureStorage.Store(Model.Id, Model.Password);
            }
        }

        public void CancelCredentialsEntry()
        {
            this.CloseScreen();
        }
#endregion

#region
        private async Task ReceiveDataFromAzure()
        {
#if IOTHUB
            while (_azureIoTHubClient != null)
            {
                var message = await _azureIoTHubClient.ReceiveAsync();
                if (message != null)
                {
                    try
                    {
                        var msg = new Models.ReceivedMessage(message.GetBytes());
                        msg.MessageId = message.MessageId;
                        msg.Topic = message.To;
                        DispatcherServices.Invoke(() => ReceivedMessageList.Insert(0, msg));
                        // Received a new message, display it
                        // We received the message, indicate IoTHub we treated it
                        await _azureIoTHubClient.CompleteAsync(message);
                    }
                    catch
                    {
                        await _azureIoTHubClient.RejectAsync(message);
                    }
                }
            }
#else
            await Task.FromResult(default(object));
#endif
        }

        private void StartReceiveThread()
        {
            Task.Run(async () =>
            {
                while (_isConnected)
                {
                    switch (Model.DefaultTransport.Value)
                    {
                        case TransportTypes.TCP:
                            {
                                var response = await _tcpClient.ReceiveAsync();

                            }

                            break;
                        case TransportTypes.UDP:
                            {
                                var response = await _udpClient.ReceiveAsync();

                            }
                            break;
                    }
                }
            });
        }

        private void _mqttClient_CommandReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var msg = new Models.ReceivedMessage(e.Message);
            msg.Topic = e.Topic;
            msg.MessageId = e.MessageId;
            DispatcherServices.Invoke(() => ReceivedMessageList.Insert(0, msg));
        }
#endregion

#region Connect to specified protocol
        public async void Connect()
        {
            try
            {
                IsBusy = true;
                switch (Model.DefaultTransport.Value)
                {
                    /*                    case TransportTypes.AMQP:
                                            {
                                                var connectionString = $"Endpoint=sb://{Model.DefaultEndPoint}.servicebus.windows.net/;SharedAccessKeyName={Model.AccessKeyName};SharedAccessKey={Model.AccessKey}";
                                                var bldr = new EventHubsConnectionStringBuilder(connectionString)
                                                {
                                                    EntityPath = Model.HubName
                                                };

                                                _isConnected = true;
                                            }

                                            break;*/

                    case TransportTypes.AzureIoTHub:
#if IOTHUB
                        var connectionString = $"HostName={Model.DefaultEndPoint};DeviceId={Model.DeviceId};SharedAccessKey={Model.AccessKey}";
                        _azureIoTHubClient = DeviceClient.CreateFromConnectionString(connectionString, Microsoft.Azure.Devices.Client.TransportType.Amqp_Tcp_Only);
                        await _azureIoTHubClient.OpenAsync();
                        ReceivingTask = Task.Run(ReceiveDataFromAzure);
                        SetConnectedState();
#endif
                        break;
                    case TransportTypes.MQTT:
                        _mqttClient = SLWIOC.Create<IMQTTDeviceClient>();
                        _mqttClient.ShowDiagnostics = true;
                        _mqttClient.BrokerHostName = Model.DefaultEndPoint;
                        _mqttClient.BrokerPort = Model.DefaultPort;
                        _mqttClient.DeviceId = Model.UserName;
                        _mqttClient.Password = Model.Password;
                        var result = await _mqttClient.ConnectAsync();
                        if (result.Result == ConnAck.Accepted)
                        {
                            _isConnected = true;
                            if (!String.IsNullOrEmpty(Model.Subscription))
                            {
                                var subscription = new MQTTSubscription()
                                {
                                    Topic = Model.Subscription.Replace("~deviceid~", Model.DeviceId),
                                    QOS = EntityHeader<QOS>.Create(QOS.QOS2)
                                };
                                await _mqttClient.SubscribeAsync(subscription);
                                _mqttClient.MessageReceived += _mqttClient_CommandReceived;
                            }

                            SetConnectedState();
                        }
                        else
                        {
                            await Popups.ShowAsync($"{Resources.SimulatorCoreResources.Simulator_ErrorConnecting}: {result.Result.ToString()}");
                        }

                        break;
                    case TransportTypes.TCP:
                        _tcpClient = SLWIOC.Create<ITCPClient>();
                        await _tcpClient.ConnectAsync(Model.DefaultEndPoint, Model.DefaultPort);
                        StartReceiveThread();
                        SetConnectedState();
                        break;
                    case TransportTypes.UDP:
                        _udpClient = SLWIOC.Create<IUDPClient>();
                        await _udpClient.ConnectAsync(Model.DefaultEndPoint, Model.DefaultPort);
                        StartReceiveThread();
                        SetConnectedState();
                        break;
                }

                RightMenuIcon = Client.Core.ViewModels.RightMenuIcon.None;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await Popups.ShowAsync(ex.Message);
                if (_mqttClient != null)
                {
                    _mqttClient.Dispose();
                    _mqttClient = null;
                }

#if IOTHUB
                if (_azureIoTHubClient != null)
                {
                    await _azureIoTHubClient.CloseAsync();
                    _azureIoTHubClient.Dispose();
                    _azureIoTHubClient = null;
                }
#endif

                if (_tcpClient != null)
                {
                    await _tcpClient.DisconnectAsync();
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }

                if (_udpClient != null)
                {
                    await _udpClient.DisconnectAsync();
                    _udpClient.Dispose();
                    _udpClient = null;
                }

                SetDisconnectedState();
            }
            finally
            {

                IsBusy = false;
            }
        }
#endregion

#region Disconnect from Current Transport
        public async void Disconnect()
        {
            await DisconnectAsync();
        }


        public async Task DisconnectAsync()
        {
            switch (Model.DefaultTransport.Value)
            {
                /*case TransportTypes.AMQP:
                    ConnectButtonVisible = true;
                    break;*/
                case TransportTypes.AzureIoTHub:
#if IOTHUB
                    if (_azureIoTHubClient != null)
                    {
                        await _azureIoTHubClient.CloseAsync();
                        _azureIoTHubClient.Dispose();
                        _azureIoTHubClient = null;
                    }
#endif

                    ConnectButtonVisible = true;
                    break;
                case TransportTypes.MQTT:
                    if (_mqttClient != null)
                    {
                        _mqttClient.Disconnect();
                        _mqttClient = null;
                    }

                    ConnectButtonVisible = true;
                    break;
                case TransportTypes.TCP:
                    if (_tcpClient != null)
                    {
                        await _tcpClient.DisconnectAsync();
                        _tcpClient.Dispose();
                    }

                    ConnectButtonVisible = true;
                    break;
                case TransportTypes.UDP:
                    if (_udpClient != null)
                    {
                        await _udpClient.DisconnectAsync();
                        _udpClient.Dispose();
                    }
                    ConnectButtonVisible = true;
                    break;
            }

            SetDisconnectedState(); ;

            RightMenuIcon = Client.Core.ViewModels.RightMenuIcon.None;
        }
#endregion

#region Message Support
        List<MessageTemplate> _messageTemplates;
        public List<MessageTemplate> MessageTemplates
        {
            get { return _messageTemplates; }
            set { Set(ref _messageTemplates, value); }
        }

        private ObservableCollection<ReceivedMessage> _receivedMessageList;
        public ObservableCollection<ReceivedMessage> ReceivedMessageList
        {
            get { return _receivedMessageList; }
            set { Set(ref _receivedMessageList, value); }
        }

        MessageTemplate _selectedMessageTemplate;
        public MessageTemplate SelectedMessageTemplate
        {
            get { return _selectedMessageTemplate; }
            set
            {
                if (value != null && _selectedMessageTemplate != value)
                {
                    if (HasPersistentConnection())
                    {
                        if (!_isConnected)
                        {
                            Popups.ShowAsync(Resources.SimulatorCoreResources.Simulator_EditDisconnect);
                            _selectedMessageTemplate = null;
                            RaisePropertyChanged();
                            return;
                        }
                    }

                    var launchArgs = new ViewModelLaunchArgs()
                    {
                        ViewModelType = typeof(Messages.SendMessageViewModel),
                        Parent = value,
                        LaunchType = LaunchTypes.Other,
                    };

                    if (HasPersistentConnection())
                    {
                        if (_mqttClient != null) launchArgs.Parameters.Add("mqttclient", _mqttClient);
                        if (_tcpClient != null) launchArgs.Parameters.Add("tcpclient", _tcpClient);
                        if (_udpClient != null) launchArgs.Parameters.Add("udpclient", _udpClient);
#if IOTHUB
                        if (_azureIoTHubClient != null) launchArgs.Parameters.Add("azureIotHubClient", _azureIoTHubClient);
#endif
                    }

                    launchArgs.Parameters.Add("receviedmessages", ReceivedMessageList);
                    launchArgs.Parameters.Add("simulator", Model);

                    ViewModelNavigation.NavigateAsync(launchArgs);
                }

                _selectedMessageTemplate = null;
                RaisePropertyChanged();
            }
        }
#endregion

#region Command Support
        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }

        public RelayCommand ShowReceivedMessagesCommand { get; set; }
        public RelayCommand ShowMessageTemplatesCommand { get; set; }
#endregion

#region Properties and Methods to Control Rendering
        public void ShowReceivedMessages()
        {
            ReceivedMessagesVisible = true;
            MessageTemplatesVisible = false;
        }

        public void ShowMessageTemplates()
        {
            ReceivedMessagesVisible = false;
            MessageTemplatesVisible = true;
        }


        private string _connectionColor = "red";
        public string ConnectionColor
        {
            get { return _connectionColor; }
            set { Set(ref _connectionColor, value); }
        }


        private bool _viewSelectorVisible = false;
        public bool ViewSelectorVisible
        {
            get { return _viewSelectorVisible; }
            set
            {
                _viewSelectorVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool _connetionIconVisible = false;
        public bool ConnectionIconVisible
        {
            get { return _connetionIconVisible; }
            set
            {
                _connetionIconVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool _connectButtonVisible = false;
        public bool ConnectButtonVisible
        {
            get { return _connectButtonVisible; }
            set
            {
                _connectButtonVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool _disconnectButtonVisbile = false;
        public bool DisconnectButtonVisible
        {
            get { return _disconnectButtonVisbile; }
            set
            {
                _disconnectButtonVisbile = value;
                RaisePropertyChanged();
            }
        }

        private bool _messageTemplatesVisible = false;
        public bool MessageTemplatesVisible
        {
            get { return _messageTemplatesVisible; }
            set
            {
                _messageTemplatesVisible = value;
                RaisePropertyChanged();
            }
        }



        private bool _portVisible = false;
        public bool PortVisible
        {
            get { return _portVisible; }
            set
            {
                _portVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool _receiveeMessageVisible = false;
        public bool ReceivedMessagesVisible
        {
            get { return _receiveeMessageVisible; }
            set
            {
                _receiveeMessageVisible = value;
                RaisePropertyChanged();
            }
        }

        LagoVista.IoT.Simulator.Admin.Models.Simulator _model;
        public LagoVista.IoT.Simulator.Admin.Models.Simulator Model
        {
            get { return _model; }
            set { Set(ref _model, value); }
        }
#endregion
    }
}
