using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace Biotracker.Client.ProcessMonitor
{
    public class NetworkMonitor
    {
        private static List<IPAddress> _localIP = null;
        public  static List<IPAddress> LocalIP
        {
            get
            {
                if (_localIP == null)
                {
                    _localIP = new List<IPAddress>();
                    foreach (NetworkInterface nic in ActiveNIC)
                    {
                        foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                        {
                            _localIP.Add(ip.Address);
                        }
                    }
                }
                
                return _localIP;
            }
        }

        public static NetworkInterface[] ActiveNIC 
        {
            get 
            {
                var query = from nic in NetworkInterface.GetAllNetworkInterfaces()
                            where nic.OperationalStatus == OperationalStatus.Up
                            && nic.IsReceiveOnly == false
                            && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
                            select nic;

                return query.ToArray();
            } 
             
        }

        public static bool IsLocalAddress(IPAddress ip)
        {
            foreach (IPAddress localIp in LocalIP)
            {
                if (localIp.Equals(ip))
                    return true;
            }

            return false;
        }

        void NetworkAddressChangeCallback(object sender, EventArgs e)
        {
            _localIP = null;
        }
        

        public NetworkMonitor()
        {
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(this.NetworkAddressChangeCallback);
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(this.NetworkAddressChangeCallback);
        }


        

    }
}
