using LagoVista.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Simulator
{
    public class ClientAppInfo : IClientAppInfo
    {
        public Type MainViewModel { get; set; }
    }
}
