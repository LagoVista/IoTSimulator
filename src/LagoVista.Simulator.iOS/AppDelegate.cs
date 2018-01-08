using Foundation;
using LagoVista.Client.Core.Auth;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.MQTT.Core;
using LagoVista.XPlat.iOS.Services;
using System;
using UIKit;

namespace LagoVista.Simulator.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //DEV Key Replaced on Server as Part of Build Process
        private const string MOBILE_CENTER_KEY = "f9ae5b45-bc21-43ff-af8a-60a66fc48c6c";

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {       
            LagoVista.XPlat.iOS.Startup.Init(app, MOBILE_CENTER_KEY);

            SLWIOC.Register<IMqttNetworkChannel, MqttNetworkChannel>();

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            app.StatusBarHidden = false;

            global::Xamarin.Forms.Forms.Init();

            var version = NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleVersion")].ToString();
            Console.WriteLine($"NSLog Version {version}");

            var versionParts = version.Split('.');
            var versionInfo = new VersionInfo();
            if (versionParts.Length != 4)
            {
                throw new Exception("Expecting CFBundleVersion to be a version consisting of four parts 1.0.218.1231 [Major].[Minor].[Build].[Revision]");
            }

            var formsApp = new App();

            /* if this blows up our build version is borked...make sure all version numbers are intergers like 1.0.218.1231 */
            versionInfo.Major = Convert.ToInt32(versionParts[0]);
            versionInfo.Minor = Convert.ToInt32(versionParts[1]);
            versionInfo.Build = Convert.ToInt32(versionParts[2]);
            versionInfo.Revision = Convert.ToInt32(versionParts[3]);
            formsApp.SetVersion(versionInfo);

            LoadApplication(formsApp);

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            LagoVista.Simulator.App.Instance.HandleURIActivation(new Uri(url.AbsoluteString));
            return true;
        }
    }
}
