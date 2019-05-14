using snmpmonitor.DATA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using SnmpSharpNet;
using System.Net;

namespace snmpmonitor.SVC
{
    public partial class Service1 : ServiceBase
    {
        //Service Timer Info
        private System.Timers.Timer m_mainTimer;
        private int interval = 15 * 1000; //How often to run in milliseconds (seconds * 1000)

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //Create the Main timer
            m_mainTimer = new System.Timers.Timer
            {
                //Set the timer interval
                Interval = interval
            };
            //Dictate what to do when the event fires
            m_mainTimer.Elapsed += m_mainTimer_Elapsed;
            //Something to do with something, I forgot since it's been a while
            m_mainTimer.AutoReset = true;

#if DEBUG
#else
            m_mainTimer.Start(); //Start timer only in Release
#endif
            //Run 1st Tick Manually
            Console.Beep();
            Routine();
        }

        protected override void OnStop()
        {
        }

        public void OnDebug()
        {
            //Manually kick off the service when debugging
            OnStart(null);
        }

        void m_mainTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Each interval run the UpdateUsers() function
            Routine();
        }

        private void Routine()
        {
            var d = CreateTestDevice();

            foreach(var monitor in d.Monitors)
            {
                monitor.Up = CheckMonitor(d.IPAddress, monitor);
            }


        }

        private bool CheckMonitor(string IP, Monitor Monitor)
        {
            try
            {
                var community = new OctetString("public");

                var param = new AgentParameters(community)
                {
                    Version = SnmpVersion.Ver1
                };

                var agent = new IpAddress(IP);

                using (var target = new UdpTarget((IPAddress)agent, 161, 2000, 1))
                {
                    var pdu = new Pdu(PduType.Get);
                    pdu.VbList.Add(Monitor.Oid);

                    var result = (SnmpV1Packet)target.Request(pdu, param);

                    if (result != null)
                    {
                        // ErrorStatus other then 0 is an error returned by 
                        // the Agent - see SnmpConstants for error definitions
                        if (result.Pdu.ErrorStatus != 0)
                        {
                            // agent reported an error with the request
                            Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                                result.Pdu.ErrorStatus,
                                result.Pdu.ErrorIndex);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else { Console.WriteLine("No response received from SNMP agent.");
                    }
                    target.Close();
                    return false;
                }
            }
            catch (Exception e) { Console.WriteLine(e); return false; }
        }
        
        private Device CreateTestDevice()
        {
            var device = new Device()
            {
                Id = new Guid(),
                IPAddress = "192.168.119.2",
                Site = "Hackensack",
                Monitors = new Monitor[]
                {
                    new Monitor()
                    {
                        Id = new Guid(),
                        Name = "Cable PRI",
                        Oid = ".1.3.6.1.2.1.10.20.1.3.4.1.2",
                        Up = null
                    }
                }
            };

            return device;
        }
    }
}
