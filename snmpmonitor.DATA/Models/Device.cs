using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snmpmonitor.DATA.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public string IPAddress { get; set; }
        public string Site { get; set; }
        public Monitor[] Monitors { get; set; }
    }
}
