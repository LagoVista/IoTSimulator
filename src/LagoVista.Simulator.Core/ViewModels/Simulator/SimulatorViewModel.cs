using LagoVista.Client;
using LagoVista.Client.Core;
using LagoVista.Client.Core.Auth;
using LagoVista.Client.Core.Net;
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
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.EventHubs;
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
        DeviceClient _azureIoTHubClient;
        bool _isConnected;

        Task ReceivingTask;
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
                                    this.Model.AccessKey = _secureStorage.Retrieve(this.Model.Id);
                                    break;
                                case TransportTypes.RestHttp:
                                case TransportTypes.RestHttps:
                                case TransportTypes.MQTT:
                                    if (!this.Model.Anonymous)
                                    {
                                        this.Model.Password = _secureStorage.Retrieve(this.Model.Id);
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
                                   if(! await PromptForAccessKey())
                                    {
                                        await this.CloseScreenAsync();
                                    }
                                    break;
                                case TransportTypes.RestHttp:
                                case TransportTypes.RestHttps:
                                case TransportTypes.MQTT:
                                    if (!await PromptForPassword())
                                    {
                                        await this.CloseScreenAsync();
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        public override async Task ReloadedAsync()
        {
            if (!_isConnected)
            {
                await PerformNetworkOperation(LoadSimulator);
            }
        }
        #endregion

        #region Handle Messages from Server
        public async Task<bool> PromptForPassword()
        {
            var result = await Popups.PromptForStringAsync(Resources.SimulatorCoreResources.Simulator_PromptPassword);
            if (String.IsNullOrEmpty(result))
            {
                await Popups.ShowAsync(Resources.SimulatorCoreResources.Simulator_PasswordIsRequired);
                return false;
            }
            else
            {
                this.Model.Password = result;
                return true;
            }
        }

        public async Task<bool> PromptForAccessKey()
        {
            var result = await Popups.PromptForStringAsync(Resources.SimulatorCoreResources.Simulator_PromptAccessKey);
            if (String.IsNullOrEmpty(result))
            {
                await Popups.ShowAsync(Resources.SimulatorCoreResources.Simulator_AccessKeyIsRequired);
                return false;
            }
            else
            {
                this.Model.AccessKey = result;
                return true;
            }
        }

        private async Task ReceiveDataFromAzure()
        {
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
                        {
                            if (String.IsNullOrEmpty(Model.AccessKey))
                            {
                                if (!await PromptForAccessKey()) return;
                            }

                            var connectionString = $"HostName={Model.DefaultEndPoint};DeviceId={Model.DeviceId};SharedAccessKey={Model.AccessKey}";
                            _azureIoTHubClient = DeviceClient.CreateFromConnectionString(connectionString, Microsoft.Azure.Devices.Client.TransportType.Amqp_Tcp_Only);
                            await _azureIoTHubClient.OpenAsync();
                            ReceivingTask = Task.Run(ReceiveDataFromAzure);
                            SetConnectedState();
                        }
                        break;
                    case TransportTypes.MQTT:
                        if (!Model.Anonymous && String.IsNullOrEmpty(Model.Password))
                        {
                            if (!await PromptForPassword()) return;
                        }

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
                            await Popups.ShowAsync($"{Resources.SimulatorCoreResources.Simulator_ErrorConnecting}: {result.Message}");
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

                if (_azureIoTHubClient != null)
                {
                    await _azureIoTHubClient.CloseAsync();
                    _azureIoTHubClient.Dispose();
                    _azureIoTHubClient = null;
                }

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
                    if (_azureIoTHubClient != null)
                    {
                        await _azureIoTHubClient.CloseAsync();
                        _azureIoTHubClient.Dispose();
                        _azureIoTHubClient = null;
                    }

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
                        if (_azureIoTHubClient != null) launchArgs.Parameters.Add("azureIotHubClient", _azureIoTHubClient);
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
