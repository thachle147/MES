using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MESWeb.Models
{
    public class BroadcastDeviceStateModel
    {
        public Device Device { get; set; }
        public DateTime DateReceived { get; set; }
    }
}