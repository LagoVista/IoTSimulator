using Android.App;
using Android.Content.PM;
using Android.OS;
using System.Reflection;
using static LagoVista.Simulator.Droid.Resource;

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

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            
            var version = typeof(App).GetTypeInfo().Assembly.GetName().Version;

            var versionInfo = new LagoVista.Core.Models.VersionInfo()
            {
                Major = version.Major,
                Minor = version.Minor,
                Revision = version.Revision,
                Build = version.Build,
            };

            var app = new App();
            app.SetVersion(versionInfo);

            LoadApplication(app);
        }
    }
}

