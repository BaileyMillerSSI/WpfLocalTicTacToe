using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GameLibrary
{
    public class BaseMessage
    {
        public IPEndPoint RemoteHost { get; set; }
        public DateTime TimeSent { get; set; }
        public virtual object Body { get; set; }

        public BaseMessage()
        {
            TimeSent = DateTime.UtcNow;
        }

    }
}
