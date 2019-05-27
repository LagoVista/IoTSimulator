using Android.App;
using System;
using Android.Content.PM;
using Android.OS;
using LagoVista.Core.IOC;
using LagoVista.MQTT.Core;
using LagoVista.MQTT.Droid;
using System.Reflection;
using static LagoVista.Simulator.Droid.Resource;
using LagoVista.Core.Models;

namespace LagoVista.Simulator.Droid
{
    [Activity(Label = "IoT Simulator", Icon = "@mipmap/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public const string MOBILE_CENTER_KEY = "1211d9f6-eed2-44a1-bfac-e7b080e000c1";

        protected override void OnCreate(Bundle bundle)
        { 
            TabLayoutResource = Layout.Tabbar;
            ToolbarResource = Layout.Toolbar;

            //https://play.google.com/apps/publish/?dev_acc=12258406958683843289
            LagoVista.XPlat.Droid.Startup.Init(BaseContext, MOBILE_CENTER_KEY);

            SLWIOC.Register<IMqttNetworkChannel, MqttNetworkChannel>();

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            var packageInfo = PackageManager.GetPackageInfo(PackageName, 0);
            var version = packageInfo.VersionName;

            var versionParts = version.Split('.');
            if(versionParts.Length == 3)
            {
                version += ".0";
                versionParts = version.Split('.');
            }

            var versionInfo = new VersionInfo();
            if (versionParts.Length != 4)
            {
                throw new Exception("Expecting android:versionName in AndroidManifest.xml to be a version conisting of four parts 1.0.218.1231 [Major].[Minor].[Build].[Revision]");
            }

            /* if this blows up our versionName in AndroidManaifest.xml is borked...make sure all version numbers are intergers like 1.0.218.1231 */
            versionInfo.Major = Convert.ToInt32(versionParts[0]);
            versionInfo.Minor = Convert.ToInt32(versionParts[1]);
            versionInfo.Build = Convert.ToInt32(versionParts[2]);
            versionInfo.Revision = Convert.ToInt32(versionParts[3]);

            var app = new LagoVista.Simulator.App();            
            app.SetVersion(versionInfo);

            LoadApplication(app);
        }
    }
}

