using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Client.ProcessMonitor
{
    [Serializable]
    public class UDPConnection : ConnectionPair2, IComparable
    {


        public UDPConnection() :base()
        { 
        
        }

        public UDPConnection(IPEndPoint srcEnd)
            : base(srcEnd, new IPEndPoint(IPAddress.Any, 0), Packet2.Protocol.UDP)
        { 
            
        }

        public override int GetHashCode()
        {
            int hashSrc = SrcEnd.GetHashCode();
            return hashSrc ^ base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is UDPConnection)
            {
                UDPConnection udp = (UDPConnection)obj;

                if (IPAddress.Equals(udp.SrcEnd.Address, IPAddress.Any)
                    || IPAddress.Equals(this.SrcEnd.Address, IPAddress.Any))
                {
                    return SrcEnd.Port == udp.SrcEnd.Port;
                }
                else
                {
                    if (IPAddress.Equals(SrcEnd.Address, udp.SrcEnd.Address)
                        && SrcEnd.Port == udp.SrcEnd.Port)
                        return true;
                }
            }

            return false;
        }
        
    }
}
