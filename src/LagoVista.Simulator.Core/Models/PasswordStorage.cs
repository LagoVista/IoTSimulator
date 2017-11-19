using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Simulator.Core.Models
{
    public class PasswordStorage
    {
        public List<PasswordValues> Passwords { get; set; }

        public void AddPassword(string simulatorId, string passwordOrKey)
        {
         
        }
    }

    public class PasswordValues
    {
        public string SimulatorId { get; set; }
        public string PasswordOrKey { get; set; }
    }
}
