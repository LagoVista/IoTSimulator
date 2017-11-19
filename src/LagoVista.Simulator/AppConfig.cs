using LagoVista.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Simulator
{
    public class AppConfig : IAppConfig
    {
        public PlatformTypes PlatformType => PlatformTypes.WindowsUWP;

        public Environments Environment => Environments.Local;

        public string WebAddress => "http://localhost:5000";

        public string AppName => "Remote Simulator";

        public string AppLogo => "";

        public string CompanyLogo => "";

        public bool EmitTestingCode => true;

        public string AppId => "C2781A0A72DB4634975F868F0C0405C3";
        public string ClientType => "mobileapp";
    }
}
