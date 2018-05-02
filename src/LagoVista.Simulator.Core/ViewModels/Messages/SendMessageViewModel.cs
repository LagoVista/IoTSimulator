#define IOTHUB
#define SERVICEBUS
#define EVENTHUB

using LagoVista.Client.Core.Net;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Networking.Interfaces;
using LagoVista.IoT.Simulator.Admin.Models;
using LagoVista.Simulator.Core.Resources;
#if IOTHUB
using Microsoft.Azure.Devices.Client;
#endif

#if EVENTHUB
using Microsoft.Azure.EventHubs;
#endif

#if SERVICEBUS
using Microsoft.Azure.ServiceBus;
#endif 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core;
using System.Text.RegularExpressions;
using System.Diagnostics;
using LagoVista.Simulator.Core.Models;
using LagoVista.Client;
using LagoVista.Client.Core;
using System.Threading;

namespace LagoVista.Simulator.Core.ViewModels.Messages
{
    public class SendMessageViewModel : XPlatViewModel
    {
        Random _random = new Random();
        Timer _timer;

        int _pointIndex;


        #region Constructor and Initialization
        public SendMessageViewModel()
        {
            SendCommand = new RelayCommand(Send);
            ApplySettingsCommand = new RelayCommand(ApplySettings);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowReceivedMessagesCommand = new RelayCommand(() => ShowReceivedMessages());
            ShowSendStatusCommand = new RelayCommand(() => ShowSendStatus());
        }

        public override Task InitAsync()
        {
            MsgTemplate = LaunchArgs.Parent as MessageTemplate;
            Simulator = LaunchArgs.GetParam<LagoVista.IoT.Simulator.Admin.Models.Simulator>("simulator");
            ReceivedMessageList = LaunchArgs.GetParam<ObservableCollection<ReceivedMessage>>("receviedmessages");

            BuildRequestContent();

            switch (Simulator.DefaultTransport.Value)
            {
                //                case TransportTypes.AMQP:
                case TransportTypes.MQTT:
                case TransportTypes.AzureIoTHub:
                case TransportTypes.TCP:
                case TransportTypes.UDP:
                    //              case TransportTypes.RabbitMQ:
                    ViewSelectorVisible = true;
                    break;
                case TransportTypes.AzureEventHub:
                case TransportTypes.AzureServiceBus:
                case TransportTypes.RestHttp:
                case TransportTypes.RestHttps:
                    ViewSelectorVisible = false;
                    break;
            }

            ShowSendStatus();

            return base.InitAsync();
        }

        public void ShowSendStatus()
        {
            SendStatusVisible = true;
            ReceivedMessagesVisible = false;
        }

        public void ShowReceivedMessages()
        {
            SendStatusVisible = false;
            ReceivedMessagesVisible = true;
        }
        #endregion

        #region Message Token Replacement
        private String ReplaceTokens(String input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return String.Empty;
            }

            foreach (var attr in MsgTemplate.DynamicAttributes)
            {
                input = input.Replace($"~{attr.Key}~", attr.DefaultValue);
            }

            input = input.Replace($"~deviceid~", Simulator.DeviceId);
            input = input.Replace($"~datetime~", DateTime.Now.ToString());
            input = input.Replace($"~username~", Simulator.UserName);
            input = input.Replace($"~password~", Simulator.Password);
            input = input.Replace($"~accesskey~", Simulator.AccessKey);
            input = input.Replace($"~password~", Simulator.Password);
            input = input.Replace($"~datetimeiso8601~", DateTime.UtcNow.ToJSONString());

            var floatRegEx = new Regex(@"~random-float,(?'min'[+-]?(([0-9]*[.]?)?[0-9]+)),(?'max'[+-]?([0-9]*[.])?[0-9]+)~");
            var intRegEx = new Regex(@"~random-int,(?'min'[+-]?\d+),(?'max'[+-]?\d+)~");
            var floatMatches = floatRegEx.Matches(input);

            foreach (Match match in floatMatches)
            {
                if (float.TryParse(match.Groups["min"].Value, out float minValue) && float.TryParse(match.Groups["max"].Value, out float maxValue))
                {
                    if (minValue > maxValue)
                    {
                        var tmp = maxValue;
                        maxValue = minValue;
                        minValue = tmp;
                    }

                    Debug.WriteLine(minValue + " " + maxValue);
                    var delta = maxValue - minValue;
                    var value = delta * _random.NextDouble() + minValue;
                    input = input.Replace(match.Value, Math.Round(value, 2).ToString());
                }
            }

            var intMatches = intRegEx.Matches(input);

            foreach (Match match in intMatches)
            {
                if (int.TryParse(match.Groups["min"].Value, out int minValue) && int.TryParse(match.Groups["max"].Value, out int maxValue))
                {
                    if (minValue > maxValue)
                    {
                        var tmp = maxValue;
                        maxValue = minValue;
                        minValue = tmp;
                    }
                    var delta = maxValue - minValue;
                    var value = _random.Next(minValue, maxValue);
                    input = input.Replace(match.Value, value.ToString());
                }
            }

            if (MsgTemplate.AppendCR) input += "\r";
            if (MsgTemplate.AppendLF) input += "\n";

            return input;
        }

        public void ShowSettings()
        {
            SettingsVisible = true;
        }

        public void ApplySettings()
        {
            SettingsVisible = false;
            BuildRequestContent();
        }

        private void BuildRequestContent()
        {
            var sentContent = new StringBuilder();

            switch (MsgTemplate.Transport.Value)
            {

                case TransportTypes.TCP:
                    sentContent.AppendLine($"Host   : {Simulator.DefaultEndPoint}");
                    sentContent.AppendLine($"Port   : {MsgTemplate.Port}");
                    sentContent.AppendLine($"Body");
                    sentContent.AppendLine($"---------------------------------");
                    sentContent.Append(ReplaceTokens(MsgTemplate.TextPayload));

                    break;
                case TransportTypes.UDP:
                    sentContent.AppendLine($"Host   : {Simulator.DefaultEndPoint}");
                    sentContent.AppendLine($"Port   : {MsgTemplate.Port}");
                    sentContent.AppendLine($"Body");
                    sentContent.AppendLine($"---------------------------------");
                    sentContent.Append(ReplaceTokens(MsgTemplate.TextPayload));

                    break;
                case TransportTypes.AzureIoTHub:
                    sentContent.AppendLine($"Host   : {Simulator.DefaultEndPoint}");
                    sentContent.AppendLine($"Port   : {MsgTemplate.Port}");
                    sentContent.AppendLine($"Body");
                    sentContent.AppendLine($"---------------------------------");
                    sentContent.Append(ReplaceTokens(MsgTemplate.TextPayload));

                    break;

                case TransportTypes.AzureEventHub:
                    sentContent.AppendLine($"Host   : {Simulator.DefaultEndPoint}");
                    sentContent.AppendLine($"Body");
                    sentContent.AppendLine($"---------------------------------");
                    sentContent.Append(ReplaceTokens(MsgTemplate.TextPayload));

                    break;

                case TransportTypes.AzureServiceBus:
                    sentContent.AppendLine($"Host   : {MsgTemplate.Name}");
                    //sentContent.AppendLine($"Queue   : {MsgTemplate.Qu}");
                    sentContent.AppendLine($"Body");
                    sentContent.AppendLine($"---------------------------------");
                    sentContent.Append(ReplaceTokens(MsgTemplate.TextPayload));

                    break;
                case TransportTypes.MQTT:
                    sentContent.AppendLine($"Host         : {Simulator.DefaultEndPoint}");
                    sentContent.AppendLine($"Port         : {MsgTemplate.Port}");
                    sentContent.AppendLine($"Topics");
                    sentContent.AppendLine($"Publish      : {ReplaceTokens(MsgTemplate.Topic)}");
                    sentContent.AppendLine($"Subscription : {ReplaceTokens(Simulator.Subscription)}");

                    sentContent.Append(ReplaceTokens(MsgTemplate.TextPayload));

                    break;
                case TransportTypes.RestHttps:
                case TransportTypes.RestHttp:
                    {
                        var protocol = MsgTemplate.Transport.Value == TransportTypes.RestHttps ? "https" : "http";
                        var uri = $"{protocol}://{Simulator.DefaultEndPoint}:{Simulator.DefaultPort}{ReplaceTokens(MsgTemplate.PathAndQueryString)}";
                        sentContent.AppendLine($"Method       : {MsgTemplate.HttpVerb}");
                        sentContent.AppendLine($"Endpoint     : {uri}");
                        var contentType = String.IsNullOrEmpty(MsgTemplate.ContentType) ? "text/plain" : MsgTemplate.ContentType;
                        sentContent.AppendLine($"Content Type : {contentType}");

                        if (Simulator.BasicAuth)
                        {
                            var authCreds = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Simulator.UserName + ":" + Simulator.Password));
                            sentContent.AppendLine($"Authorization: Basic {authCreds}");
                        }
                        if (MsgTemplate.MessageHeaders.Any())
                        {
                            sentContent.AppendLine($"Custom Headers");
                        }

                        var idx = 1;
                        foreach (var hdr in MsgTemplate.MessageHeaders)
                        {
                            sentContent.AppendLine($"\t{idx++}. {hdr.HeaderName}={ReplaceTokens(hdr.Value)}");
                        }

                        if (MsgTemplate.HttpVerb == "POST" || MsgTemplate.HttpVerb == "PUT")
                        {
                            sentContent.AppendLine("");
                            sentContent.AppendLine("Post Content");
                            sentContent.AppendLine("=========================");
                            sentContent.AppendLine(ReplaceTokens(MsgTemplate.TextPayload));
                        }
                    }
                    break;
            }

            SentContent = sentContent.ToString();
        }
        #endregion

        #region Send Messages for protocols
        private async Task SendTCPMessage()
        {
            var buffer = GetMessageBytes();
            await LaunchArgs.GetParam<ITCPClient>("tcpclient").WriteAsync(buffer, 0, buffer.Length);

        }

        private async Task SendUDPMessage()
        {
            var buffer = GetMessageBytes();
            await LaunchArgs.GetParam<IUDPClient>("udpclient").WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task SendServiceBusMessage()
        {
#if SERVICEBUS
            if (String.IsNullOrEmpty(Simulator.AccessKey))
            {
                await Popups.ShowAsync("Access Key is missing");
                return;
            }

            var connectionString = $"Endpoint=sb://{Simulator.DefaultEndPoint}.servicebus.windows.net/;SharedAccessKeyName={Simulator.AccessKeyName};SharedAccessKey={Simulator.AccessKey}";
            var bldr = new ServiceBusConnectionStringBuilder(connectionString)
            {
                EntityPath = MsgTemplate.QueueName
            };

            var client = new QueueClient(bldr, ReceiveMode.PeekLock, Microsoft.Azure.ServiceBus.RetryExponential.Default);

            var msg = new Microsoft.Azure.ServiceBus.Message()
            {
                Body = GetMessageBytes(),
                To = MsgTemplate.To,
                ContentType = MsgTemplate.ContentType
            };

            if (!String.IsNullOrEmpty(MsgTemplate.MessageId))
            {
                msg.MessageId = MsgTemplate.MessageId;
            }

            await client.SendAsync(msg);
            await client.CloseAsync();
            
            ReceivedContennt = $"{DateTime.Now} {SimulatorCoreResources.SendMessage_MessagePublished}"; 
#else
            await Task.FromResult(default(object));
#endif
        }

        private async Task SendEventHubMessage()
        {
#if EVENTHUB
            if (String.IsNullOrEmpty(Simulator.AccessKey))
            {
                await Popups.ShowAsync("Access Key is missing");
                return;
            }

            var connectionString = $"Endpoint=sb://{Simulator.DefaultEndPoint}.servicebus.windows.net/;SharedAccessKeyName={Simulator.AccessKeyName};SharedAccessKey={Simulator.AccessKey}";
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(connectionString) { EntityPath = Simulator.HubName };

            var client = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            await client.SendAsync(new EventData(GetMessageBytes()));
            ReceivedContennt = $"{DateTime.Now} {SimulatorCoreResources.SendMessage_MessageSent}";

#else
            await Task.FromResult(default(object));
#endif
        }

        private async Task SendIoTHubMessage()
        {
#if IOTHUB
            var textPayload = ReplaceTokens(MsgTemplate.TextPayload);
            var msg = new Microsoft.Azure.Devices.Client.Message(GetMessageBytes());
            await LaunchArgs.GetParam<DeviceClient>("azureIotHubClient").SendEventAsync(msg);

            ReceivedContennt = $"{DateTime.Now} {SimulatorCoreResources.SendMessage_MessagePublished}";
#else
            await Task.FromResult(default(object));
#endif
        }

        private async Task SendMQTTMessage()
        {
            var qos = QOS.QOS0;

            if (!EntityHeader.IsNullOrEmpty(MsgTemplate.QualityOfServiceLevel))
            {
                switch (MsgTemplate.QualityOfServiceLevel.Value)
                {
                    case QualityOfServiceLevels.QOS1: qos = QOS.QOS1; break;
                    case QualityOfServiceLevels.QOS2: qos = QOS.QOS2; break;
                }
            }

            await LaunchArgs.GetParam<IMQTTDeviceClient>("mqttclient").PublishAsync(ReplaceTokens(MsgTemplate.Topic), GetMessageBytes(), qos, MsgTemplate.RetainFlag);

            ReceivedContennt = $"{DateTime.Now} {SimulatorCoreResources.SendMessage_MessagePublished}";
        }

        private async Task SendGeoMessage()
        {
            var pointArray = this._message.TextPayload.Split('\r');
            var geoLocation = pointArray[_pointIndex++];
            var parts = geoLocation.Split(',');
            var lat = Convert.ToDouble(parts[0]);
            var lon = Convert.ToDouble(parts[1]);
            var delay = Convert.ToInt32(parts[2]) * 1000;

            using (var client = new HttpClient())
            {
                var protocol = MsgTemplate.Transport.Value == TransportTypes.RestHttps ? "https" : "http";
                var uri = $"{protocol}://{Simulator.DefaultEndPoint}:{Simulator.DefaultPort}{ReplaceTokens(MsgTemplate.PathAndQueryString)}";

                if (!this.Simulator.Anonymous)
                {
                    if (String.IsNullOrEmpty(Simulator.Password))
                    {
                        await Popups.ShowAsync("Password is missing");
                        return;
                    }

                    var authCreds = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Simulator.UserName + ":" + Simulator.Password));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authCreds);
                }

                HttpResponseMessage responseMessage = null;

                foreach (var hdr in MsgTemplate.MessageHeaders)
                {
                    client.DefaultRequestHeaders.Add(hdr.HeaderName, ReplaceTokens(hdr.Value));
                }


                var messageBody = ReplaceTokens(MsgTemplate.TextPayload);
                messageBody = $"{{'latitude':{lat}, 'longitude':{lon}}}";
                try
                {
                    switch (MsgTemplate.HttpVerb)
                    {
                        case MessageTemplate.HttpVerb_GET:
                            responseMessage = await client.GetAsync(uri);
                            break;
                        case MessageTemplate.HttpVerb_POST:
                            responseMessage = await client.PostAsync(uri, new StringContent(messageBody, Encoding.UTF8, String.IsNullOrEmpty(MsgTemplate.ContentType) ? "text/plain" : MsgTemplate.ContentType));
                            break;
                        case MessageTemplate.HttpVerb_PUT:
                            responseMessage = await client.PutAsync(uri, new StringContent(messageBody, Encoding.UTF8, String.IsNullOrEmpty(MsgTemplate.ContentType) ? "text/plain" : MsgTemplate.ContentType));
                            break;
                        case MessageTemplate.HttpVerb_DELETE:
                            responseMessage = await client.DeleteAsync(uri);
                            break;
                    }
                }
                catch (HttpRequestException ex)
                {
                    var fullResponseString = new StringBuilder();
                    fullResponseString.AppendLine(ex.Message);
                    if (ex.InnerException != null)
                    {
                        fullResponseString.AppendLine(ex.InnerException.Message);
                    }

                    ReceivedContennt = fullResponseString.ToString();
                    return;
                }

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var fullResponseString = new StringBuilder();
                    fullResponseString.AppendLine($"{DateTime.Now} {SimulatorCoreResources.SendMessage_MessageSent}");
                    fullResponseString.AppendLine($"Response Code: {(int)responseMessage.StatusCode} ({responseMessage.ReasonPhrase})");
                    foreach (var hdr in responseMessage.Headers)
                    {
                        fullResponseString.AppendLine($"{hdr.Key}\t:{hdr.Value.FirstOrDefault()}");
                    }
                    fullResponseString.AppendLine();
                    fullResponseString.Append(responseContent);
                    ReceivedContennt = fullResponseString.ToString();
                }
                else
                {
                    ReceivedContennt = $"{responseMessage.StatusCode} - {responseMessage.ReasonPhrase}";
                }
            }

            if (this._pointIndex < pointArray.Length)
            {
                _timer = new Timer(SentNextPoint, null, delay, Timeout.Infinite);
            }
        }

        private async Task SendRESTRequest()
        {
            if(MsgTemplate.PayloadType.Id == MessageTemplate.PayloadTypes_GeoPath)
            {
                await SendGeoMessage();
            }

            using (var client = new HttpClient())
            {
                var protocol = MsgTemplate.Transport.Value == TransportTypes.RestHttps ? "https" : "http";
                var uri = $"{protocol}://{Simulator.DefaultEndPoint}:{Simulator.DefaultPort}{ReplaceTokens(MsgTemplate.PathAndQueryString)}";

                if (!this.Simulator.Anonymous)
                {
                    if (String.IsNullOrEmpty(Simulator.Password))
                    {
                        await Popups.ShowAsync("Password is missing");
                        return;
                    }

                    var authCreds = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Simulator.UserName + ":" + Simulator.Password));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authCreds);
                }

                HttpResponseMessage responseMessage = null;

                foreach (var hdr in MsgTemplate.MessageHeaders)
                {
                    client.DefaultRequestHeaders.Add(hdr.HeaderName, ReplaceTokens(hdr.Value));
                }

            
                var messageBody = ReplaceTokens(MsgTemplate.TextPayload);
                try
                {
                    switch (MsgTemplate.HttpVerb)
                    {
                        case MessageTemplate.HttpVerb_GET:
                            responseMessage = await client.GetAsync(uri);
                            break;
                        case MessageTemplate.HttpVerb_POST:
                            responseMessage = await client.PostAsync(uri, new StringContent(messageBody, Encoding.UTF8, String.IsNullOrEmpty(MsgTemplate.ContentType) ? "text/plain" : MsgTemplate.ContentType));
                            break;
                        case MessageTemplate.HttpVerb_PUT:
                            responseMessage = await client.PutAsync(uri, new StringContent(messageBody, Encoding.UTF8, String.IsNullOrEmpty(MsgTemplate.ContentType) ? "text/plain" : MsgTemplate.ContentType));
                            break;
                        case MessageTemplate.HttpVerb_DELETE:
                            responseMessage = await client.DeleteAsync(uri);
                            break;
                    }
                }
                catch (HttpRequestException ex)
                {
                    var fullResponseString = new StringBuilder();
                    fullResponseString.AppendLine(ex.Message);
                    if (ex.InnerException != null)
                    {
                        fullResponseString.AppendLine(ex.InnerException.Message);
                    }

                    ReceivedContennt = fullResponseString.ToString();
                    return;
                }

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var fullResponseString = new StringBuilder();
                    fullResponseString.AppendLine($"{DateTime.Now} {SimulatorCoreResources.SendMessage_MessageSent}");
                    fullResponseString.AppendLine($"Response Code: {(int)responseMessage.StatusCode} ({responseMessage.ReasonPhrase})");
                    foreach (var hdr in responseMessage.Headers)
                    {
                        fullResponseString.AppendLine($"{hdr.Key}\t:{hdr.Value.FirstOrDefault()}");
                    }
                    fullResponseString.AppendLine();
                    fullResponseString.Append(responseContent);
                    ReceivedContennt = fullResponseString.ToString();
                }
                else
                {
                    ReceivedContennt = $"{responseMessage.StatusCode} - {responseMessage.ReasonPhrase}";
                }
            }
        }

        private async void SentNextPoint(Object obj)
        {
            await SendRESTRequest();
        }

        public async void Send()
        {
            IsBusy = true;

            try
            {
                switch (MsgTemplate.Transport.Value)
                {
                    case TransportTypes.TCP: await SendTCPMessage(); break;
                    case TransportTypes.UDP: await SendUDPMessage(); break;
                    case TransportTypes.AzureServiceBus: await SendServiceBusMessage(); break;
                    case TransportTypes.AzureEventHub: await SendEventHubMessage(); break;
                    case TransportTypes.AzureIoTHub: await SendIoTHubMessage(); break;
                    case TransportTypes.MQTT: await SendMQTTMessage(); break;
                    case TransportTypes.RestHttps:
                    case TransportTypes.RestHttp: await SendRESTRequest(); break;
                }

                BuildRequestContent();
            }
            catch (Exception ex)
            {
                await Popups.ShowAsync(ex.Message);
                ReceivedContennt = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion

        #region Utitlity Methods
        public override Task<bool> CanCancelAsync()
        {
            if (SettingsVisible)
            {
                SettingsVisible = false;
                return Task.FromResult(false);
            }

            return base.CanCancelAsync();
        }

        private byte[] GetMessageBytes()
        {
            if (EntityHeader.IsNullOrEmpty(MsgTemplate.PayloadType) || MsgTemplate.PayloadType.Value == PaylodTypes.Binary)
            {
                return GetBinaryPayload(MsgTemplate.BinaryPayload);
            }
            else
            {
                var msgText = ReplaceTokens(MsgTemplate.TextPayload);
                return System.Text.UTF8Encoding.UTF8.GetBytes(msgText);
            }
        }

        public byte[] GetBinaryPayload(string binaryPayload)
        {
            if (String.IsNullOrEmpty(binaryPayload))
            {
                return new byte[0];
            }

            try
            {
                var bytes = new List<Byte>();

                if (binaryPayload.Length % 2 == 0 && !binaryPayload.StartsWith("0x"))
                {
                    for (var idx = 0; idx < binaryPayload.Length; idx += 2)
                    {
                        var byteStr = binaryPayload.Substring(idx, 2);
                        bytes.Add(Byte.Parse(byteStr, System.Globalization.NumberStyles.HexNumber));
                    }
                }
                else
                {
                    var bytesList = binaryPayload.Split(' ');
                    foreach (var byteStr in bytesList)
                    {
                        var lowerByteStr = byteStr.ToLower();
                        if (lowerByteStr.Contains("soh"))
                        {
                            bytes.Add(0x01);
                        }
                        else if (lowerByteStr.Contains("stx"))
                        {
                            bytes.Add(0x02);
                        }
                        else if (lowerByteStr.Contains("etx"))
                        {
                            bytes.Add(0x03);
                        }
                        else if (lowerByteStr.Contains("eot"))
                        {
                            bytes.Add(0x04);
                        }
                        else if (lowerByteStr.Contains("ack"))
                        {
                            bytes.Add(0x06);
                        }
                        else if (lowerByteStr.Contains("cr"))
                        {
                            bytes.Add(0x0d);
                        }
                        else if (lowerByteStr.Contains("lf"))
                        {
                            bytes.Add(0x0a);
                        }
                        else if (lowerByteStr.Contains("nak"))
                        {
                            bytes.Add(0x15);
                        }
                        else if (lowerByteStr.Contains("esc"))
                        {
                            bytes.Add(0x1b);
                        }
                        else if (lowerByteStr.Contains("del"))
                        {
                            bytes.Add(0x1b);
                        }
                        else if ((lowerByteStr.StartsWith("x")))
                        {
                            bytes.Add(Byte.Parse(byteStr.Substring(1), System.Globalization.NumberStyles.HexNumber));
                        }
                        else if (lowerByteStr.StartsWith("0x"))
                        {
                            bytes.Add(Byte.Parse(byteStr.Substring(2), System.Globalization.NumberStyles.HexNumber));
                        }
                        else
                        {
                            bytes.Add(Byte.Parse(byteStr, System.Globalization.NumberStyles.HexNumber));
                        }
                    }
                }

                return bytes.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception(SimulatorCoreResources.SendMessage_InvalidBinaryPayload + " " + ex.Message);
            }
        }
        #endregion

        #region Properties for UI
        private string _sentContent;
        public String SentContent
        {
            get { return _sentContent; }
            set { Set(ref _sentContent, value); }
        }

        private string _receivedContent;
        public String ReceivedContennt
        {
            get { return _receivedContent; }
            set { Set(ref _receivedContent, value); }
        }


        MessageTemplate _message;
        public MessageTemplate MsgTemplate
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        LagoVista.IoT.Simulator.Admin.Models.Simulator _simulator;

        public LagoVista.IoT.Simulator.Admin.Models.Simulator Simulator
        {
            get { return _simulator; }
            set { Set(ref _simulator, value); }
        }

        private bool _success;
        public bool Success
        {
            get { return _success; }
            set { Set(ref _success, value); }
        }

        private bool _settingsVisible = false;
        public bool SettingsVisible
        {
            get { return _settingsVisible; }
            set { Set(ref _settingsVisible, value); }
        }

        private ObservableCollection<ReceivedMessage> _receivedMessageList;
        public ObservableCollection<ReceivedMessage> ReceivedMessageList
        {
            get { return _receivedMessageList; }
            set { Set(ref _receivedMessageList, value); }
        }


        private bool _viewSelectorVisible;
        public bool ViewSelectorVisible
        {
            get { return _viewSelectorVisible; }
            set
            {
                _viewSelectorVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool _receivedMessagesVisible;
        public bool ReceivedMessagesVisible
        {
            get { return _receivedMessagesVisible; }
            set
            {
                _receivedMessagesVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool _sendStatusVisible;
        public bool SendStatusVisible
        {
            get { return _sendStatusVisible; }
            set
            {
                _sendStatusVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        public RelayCommand ShowSettingsCommand { get; set; }

        public RelayCommand ApplySettingsCommand { get; set; }

        public RelayCommand SendCommand { get; set; }

        public RelayCommand ShowReceivedMessagesCommand { get; set; }

        public RelayCommand ShowSendStatusCommand { get; set; }
        #endregion
    }
}
