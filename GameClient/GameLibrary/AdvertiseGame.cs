using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public class AdvertiseGame : BaseMessage
    {
        public new Client Body { get; set; }
    }
}
