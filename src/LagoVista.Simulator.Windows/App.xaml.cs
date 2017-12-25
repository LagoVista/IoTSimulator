using LagoVista.Client.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.UWP.Services;
using LagoVista.MQTT.Core;
using LagoVista.MQTT.UWP;
using LagoVista.XPlat.UWP.Network;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LagoVista.Simulator.Windows
{
    sealed partial class App : Application
    {
        private const string MOBILE_CENTER_KEY = "f4def9b7-eb9f-465f-a474-1956ab779936";

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            UnhandledException += (sender, e) =>
            {
                Debug.WriteLine("EXCPETION");
                Debug.WriteLine(e.Exception);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.Exception.StackTrace);
                if (global::System.Diagnostics.Debugger.IsAttached) global::System.Diagnostics.Debugger.Break();
            };
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                var logger = SLWIOC.Get<ILogger>();

                var protocolActivatedEventArgs = (args as ProtocolActivatedEventArgs);
                if (protocolActivatedEventArgs == null)
                {
                    logger.AddCustomEvent(LogLevel.Error, "App_OnActivated", "EventArgs Not ProtocolActivatedEventArgs", new System.Collections.Generic.KeyValuePair<string, string>("type", args.GetType().Name));
                }
                else
                {                    
                    logger.AddCustomEvent(LogLevel.Message, "App_OnActivated", "URI App Activation", new System.Collections.Generic.KeyValuePair<string, string>("uri", protocolActivatedEventArgs.Uri.ToString()));
                    LagoVista.Simulator.App.Instance.HandleURIActivation(protocolActivatedEventArgs.Uri);
                }
            }

            base.OnActivated(args);
        }


        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                Xamarin.Forms.Forms.Init(e);

                LagoVista.Core.UWP.Startup.Init(this, rootFrame.Dispatcher, MOBILE_CENTER_KEY);

                SLWIOC.Register<ITCPClient, TCPClient>();
                SLWIOC.Register<IUDPClient, UDPClient>();

                SLWIOC.Register<IMqttNetworkChannel, MqttNetworkChannel>();

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) { }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
         {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
