using LagoVista.Core.Interfaces;
using Xamarin.Forms;
using LagoVista.Core.Models;

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

            WebAddress = "https://www.IoTAppStudio.com";
        }

        public PlatformTypes PlatformType { get; private set; }

        public Environments Environment { get; set; }

        public string WebAddress { get; set; }

        public string AppName => "IoT Simulator";

        public string APIToken => "";

        public string AppLogo => "applogo.png";

        public string CompanyLogo => "companylogo.png";

#if DEBUG
        public bool EmitTestingCode => true;
#else
        public bool EmitTestingCode => true;
#endif

        public string AppId => "C2781A0A72DB4634975F868F0C0405C3";
        public string ClientType => "mobileapp";

        public VersionInfo Version { get; set; }

        public string CompanyName => "Software Logistics, LLC";

        public string CompanySiteLink => "https://support.nuviot.com/help.html#/information/company";

        public string AppDescription => "IoT Simulator is a free product provided by Software Logistics, LLC to simulate devices that send data to server side IoT applications built with IoT App Studio.\r\n\r\nAlthough we hope you decide that IoT App Studio will meet your needs to develop your IoT applications, we place no restrictions on using this simualtor to tests apps on other platforms.";

        public string TermsAndConditionsLink => "https://app.termly.io/document/terms-of-use-for-saas/90eaf71a-610a-435e-95b1-c94b808f8aca";

        public string PrivacyStatementLink => "https://app.termly.io/document/privacy-policy-for-website/f0b67cde-2a08-4fe8-a35e-5e4571545d00";

        public AuthTypes AuthType => AuthTypes.User;

        public string InstanceId { get; set; }
        public string InstanceAuthKey { get; set; }
        public string DeviceId { get; set; }
        public string DeviceRepoId { get; set; }
    }
}
