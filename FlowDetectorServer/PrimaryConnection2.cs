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
    [XmlRoot("PrimaryConnection", Namespace = "http://www.plurilock.com/biotracker", IsNullable = false)]
    [Serializable]
    public class PrimaryConnection2 : IComparable
    {
        /// <summary>
        /// Source IP endpoint (IPAddress, Port, AddressFamily)
        /// </summary>
        [XmlElement]
        public IPAddress SrcIP
        {
            get;
            set;
        }
        /// <summary>
        /// Destination IP endpoint (IPAddress, Port, AddressFamily)
        /// </summary>
        [XmlElement]
        public IPAddress DestIP
        {
            get;
            set;
        }

        //public Packet2.Protocol Protocol
        //{
        //    get;
        //    set;
        //}

        [XmlIgnore]
        public PrimaryConnection2 BackwardDirection
        {
            get
            {
                return new PrimaryConnection2(this.DestIP, this.SrcIP);
            }
        }



        public PrimaryConnection2()
        {

        }

        /// <summary>
        /// Constructs a ConnectionPair object
        /// </summary>
        /// <param name="srcIp"></param>
        /// <param name="srcPort"></param>
        /// <param name="destIp"></param>
        /// <param name="destPort"></param>
        public PrimaryConnection2(IPAddress srcIp, IPAddress destIp)
        {
            this.SrcIP = srcIp;
            this.DestIP = destIp;
            
        }

           public PrimaryConnection2(Flow2 f)
        {
            this.SrcIP = f.SrcIP;
            this.DestIP = f.SrcIP;
        }
      

      

        public override int GetHashCode()
        {
            return SrcIP.GetHashCode() + DestIP.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PrimaryConnection2))
                return false;

            PrimaryConnection2 cpObj = (PrimaryConnection2)obj;

            if (cpObj.SrcIP.AddressFamily.Equals(this.SrcIP.AddressFamily))
            {
                if (cpObj.SrcIP.Equals(this.SrcIP)
                    && cpObj.DestIP.Equals(this.DestIP))
                {
                    return true;
                }
            }

            return false;
        }
        public int CompareTo(object obj)
        {
            if (obj is PrimaryConnection2)
            {
                PrimaryConnection2 cp = (PrimaryConnection2)obj;

                if (this.Equals(cp))
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
                return -1;
        }

        public override string ToString()
        {
            return string.Format("{0}<-->{1}",
                SrcIP,
                DestIP);
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
