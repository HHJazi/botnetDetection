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

    [XmlRoot("ConversationPair", Namespace = "http://www.plurilock.com/biotracker", IsNullable = false)]
    [Serializable]
    public class ConversationPair: IComparable
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

        public Packet.Protocol Protocol
        {
            get;
            set;
        }

        [XmlIgnore]
        public ConversationPair BackwardDirection
        {
            get
            {
                return new ConversationPair(this.DestIP, this.SrcIP, this.Protocol);
            }
        }



        public ConversationPair()
        {

        }

        /// <summary>
        /// Constructs a ConversationPair object
        /// </summary>
        /// <param name="srcIp"></param>
        /// <param name="srcPort"></param>
        /// <param name="destIp"></param>
        /// <param name="destPort"></param>
          public ConversationPair(IPAddress SrcIP, IPAddress DestIP, Packet.Protocol proto)
        {
            this.SrcIP = SrcIP;
            this.DestIP = DestIP;
            this.Protocol = proto;
        }

        public ConversationPair(Flow2 flw)
        {
            this.SrcIP = flw.Connection.SrcEnd.Address;
            this.DestIP = flw.Connection.DestEnd.Address;
            this.Protocol = flw.Connection.Protocol;
        }

        public override int GetHashCode()
        {
            return SrcIP.GetHashCode() + DestIP.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ConversationPair))
                return false;

            ConversationPair cpObj = (ConversationPair)obj;

            if (cpObj.SrcIP.AddressFamily.Equals(this.SrcIP.AddressFamily))
            {
                if (cpObj.SrcIP.Equals(this.SrcIP)
                    && cpObj.DestIP.Equals(this.DestIP)
                    && (cpObj.Protocol == this.Protocol))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
          public int CompareTo(object obj)
          {
              if (obj is ConversationPair)
              {
                  ConversationPair cp = (ConversationPair)obj;

                  if (this.Equals(cp))
                  {
                      return 0;
                  }
                  else
                  {
                      if (cp.Protocol == this.Protocol)
                      {
                          if (Protocol != Packet.Protocol.UDP)
                          {
                              return 1;
                          }
                      }
                      else
                      {
                          return this.Protocol == Packet.Protocol.TCP ? 1 : -1;
                      }
                  }
              }
              
                  return -1;
          }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}:<-->{1}",
                SrcIP,
                DestIP
               );
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
