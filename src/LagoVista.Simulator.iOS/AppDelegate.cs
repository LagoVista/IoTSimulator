using Foundation;
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

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            app.StatusBarHidden = false;

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            LagoVista.Simulator.App.Instance.HandleURIActivation(new Uri(url.AbsoluteString));
            return true;
        }

    }
}
