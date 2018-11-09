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
    [XmlRoot("ConnectionPair", Namespace = "http://www.plurilock.com/biotracker", IsNullable = false)]
    [Serializable]
    public class ConnectionPair2 : IComparable
    {
        /// <summary>
        /// Source IP endpoint (IPAddress, Port, AddressFamily)
        /// </summary>
        [XmlElement]
        public IPEndPoint SrcEnd
        {
            get;
            set;
        }
        /// <summary>
        /// Destination IP endpoint (IPAddress, Port, AddressFamily)
        /// </summary>
        [XmlElement]
        public IPEndPoint DestEnd
        {
            get;
            set;
        }

        public Packet2.Protocol Protocol
        {
            get;
            set;
        }

        [XmlIgnore]
        public ConnectionPair2 BackwardDirection
        {
            get
            {
                return new ConnectionPair2(this.DestEnd, this.SrcEnd, this.Protocol);
            }
        }



        public ConnectionPair2()
        {

        }

        /// <summary>
        /// Constructs a ConnectionPair object
        /// </summary>
        /// <param name="srcIp"></param>
        /// <param name="srcPort"></param>
        /// <param name="destIp"></param>
        /// <param name="destPort"></param>
        public ConnectionPair2(IPAddress srcIp, int srcPort, IPAddress destIp, int destPort, Packet2.Protocol proto)
        {
            this.SrcEnd = new IPEndPoint(srcIp, srcPort);
            this.DestEnd = new IPEndPoint(destIp, destPort);
            this.Protocol = proto;
        }

        public ConnectionPair2(IPEndPoint srcEnd, IPEndPoint destEnd, Packet2.Protocol proto)
        {
            this.SrcEnd = srcEnd;
            this.DestEnd = destEnd;
            this.Protocol = proto;
        }

        public ConnectionPair2(Packet2 pkt)
        {
            this.SrcEnd = pkt.SrcEnd;
            this.DestEnd = pkt.DestEnd;
            this.Protocol = pkt.PacketProtocol;
        }

        public override int GetHashCode()
        {
            return SrcEnd.GetHashCode() + DestEnd.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ConnectionPair2))
                return false;

            ConnectionPair2 cpObj = (ConnectionPair2)obj;

            if (cpObj.SrcEnd.AddressFamily.Equals(this.SrcEnd.AddressFamily))
            {
                if (cpObj.SrcEnd.Address.Equals(this.SrcEnd.Address)
                    && cpObj.DestEnd.Address.Equals(this.DestEnd.Address)
                    && (cpObj.SrcEnd.Port == this.SrcEnd.Port)
                    && (cpObj.DestEnd.Port == this.DestEnd.Port)
                    && (cpObj.Protocol == this.Protocol))
                {
                    return true;
                }
            }

            return false;
        }
        public int CompareTo(object obj)
        {
            if (obj is ConnectionPair2)
            {
                ConnectionPair2 cp = (ConnectionPair2)obj;

                if (this.Equals(cp))
                {
                    return 0;
                }
                else
                {
                    if (cp.Protocol == this.Protocol)
                    {
                        if (Protocol != Packet2.Protocol.UDP)
                        {
                            if (SrcEnd.Port > cp.SrcEnd.Port)
                            {
                                return 1;
                            }
                            else if (SrcEnd.Port == cp.SrcEnd.Port)
                            {
                                return DestEnd.Port >= cp.DestEnd.Port ? 1 : -1;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            return SrcEnd.Port >= cp.SrcEnd.Port ? 1 : -1;
                        }
                    }
                    else
                    {
                        return this.Protocol == Packet2.Protocol.TCP ? 1 : -1;
                    }
                }
            }
            else
                return -1;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}<-->{2}:{3}",
                SrcEnd.Address,
                SrcEnd.Port,
                DestEnd.Address,
                DestEnd.Port);
        }

        internal bool IsAddressEqual(IPAddress a, IPAddress b)
        {
            try
            {
                byte[] aAddr = a.GetAddressBytes();
                byte[] bAddr = b.GetAddressBytes();

                for (int i = 0; i < aAddr.Length; i++)
                {
                    if (aAddr[i] != bAddr[i])
                        return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
