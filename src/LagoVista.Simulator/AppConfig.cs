using LagoVista.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using LagoVista.Core.Models;
using System.Reflection;

namespace LagoVista.Simulator
{
    public class AppConfig : IAppConfig
    {
        public AppConfig()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android: PlatformType = PlatformTypes.Android; break;
                case Device.iOS: PlatformType = PlatformTypes.iPhone; break;
                case Device.UWP: PlatformType = PlatformTypes.WindowsUWP; break;
            }

        }

        public PlatformTypes PlatformType { get; private set; }

        public Environments Environment { get; set; }

        public string WebAddress { get; set; }

        public string AppName => "IoT Simulator";

        public string AppLogo => "";

        public string CompanyLogo => "";
#if DEBUG
        public bool EmitTestingCode => true;
#else
        public bool EmitTestingCode => true;
#endif

        public string AppId => "C2781A0A72DB4634975F868F0C0405C3";
        public string ClientType => "mobileapp";

        public VersionInfo Version { get; set; }
    }
}
