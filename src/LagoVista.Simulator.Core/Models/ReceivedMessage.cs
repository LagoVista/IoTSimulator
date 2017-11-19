using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Simulator.Core.Models
{
    public class ReceivedMessage
    {
        public ReceivedMessage(byte[] payload)
        {
            TextPayload = System.Text.UTF8Encoding.UTF8.GetString(payload);

            foreach(var ch in payload)
            {
                BinaryPayload += $"{ch:x2} ";
            }

            ReceivedTimeStamp = DateTime.Now;
        }

        public string MessageId { get; set; }

        public DateTime ReceivedTimeStamp { get; set; }
        public String Topic { get; set; }

        public String TextPayload { get; set; }

        public string BinaryPayload { get; set; }
    }
}
