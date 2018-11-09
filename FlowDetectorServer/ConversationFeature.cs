using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

namespace Biotracker.Client.ProcessMonitor
{
    /// <summary>
    /// This class implement features of a network flow.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ConversationFeature
    {
        //public enum FlowType
        //{
        //    Normal = 0,
        //    SMTPSpam,
        //    UDPStorm,
        //    Zeus,
        //    ZeusControl,
        //}

        #region DataMember Properties
        [DataMember]
        public byte[] SrcMAC
        {
            get;
            set;
        }

        [DataMember]
        public byte[] DestMAC
        {
            get;
            set;
        }

        [DataMember]
        public byte[] SrcIP
        {
            get;
            set;
        }
        
        [DataMember]
        public byte[] DestIP
        {
            get;
            set;
        }

        //[DataMember]
        //public int SrcPort
        //{
        //    get;
        //    set;
        //}

        //[DataMember]
        //public int DestPort
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Protocol of this flow. Values: 0 - TCP; 1 - UDP; 2 - mixed
        /// </summary>
        [DataMember]
        public int Protocol
        {
            get;
            set;
        }

        /// <summary>
        /// Total Byte Per Convesation
        /// </summary>
        [DataMember]
        public Int64 BPC
        {
            get;
            set;
        }

        /// <summary>
        /// total packet per conversation
        /// </summary>
        [DataMember]
        public Int64 PPC
        {
            get;
            set;
        }

        /// <summary>
        /// average length of packet in conversation
        /// </summary>
        [DataMember]
        public double ALP
        {
            get;
            set;
        }

        /// <summary>
        /// the number of the packets whose size smaller than 146 byte in a conversation
        /// </summary>
        [DataMember]
        public Int64 NPS146
        {
            get;
            set;
        }

        /// <summary>
        /// the proportion of the packets whose size smaller than 146 byte in a conversation
        /// </summary>
        [DataMember]
        public double PPS146
        {
            get;
            set;
        }

        /// <summary>
        /// the proportion of the packets whose size larger than 146 byte in a conversation
        /// </summary>
        [DataMember]
        public double PPL146
        {
            get;
            set;
        }
        /// <summary>
        /// the number of the packets whose size larger than 146 byte in a conversation
        /// </summary>
        [DataMember]
        public Int64 NPL146
        {
            get;
            set;
        }

        /// <summary>
        /// The size of first packets in conversation
        /// </summary>
        [DataMember]
        public Int64 sizeofFirstPkt
        {
            get;
            set;
        }
        /// <summary>
        /// the difference between the number of packets in one directon and other direction in a conversation
        /// </summary>
        [DataMember]
        public Int64 differences
        {
            get;
            set;
        }
        /// <summary>
        /// the difference between the number of byte in one directon and other direction in a conversation
        /// </summary>
        [DataMember]
        public uint byteDifferences
        {
            get;
            set;
        }
        /// <summary>
        ///the difference between the number of packets in one directon and other direction in a conversation / total number of pkt
        /// </summary>
        [DataMember]
        public double ratioPkts
        {
            get;
            set;
        }
        /// <summary>
        /// the difference between the number of byte in one directon and other direction in a conversation / total number of bytes
        /// </summary>
        [DataMember]
        public double ratioBytes
        {
            get;
            set;
        }
        /// <summary>
        /// the difference between the number of byte in one directon and other direction in a conversation / total number of bytes
        /// </summary>
        [DataMember]
        public double ratio1
        {
            get;
            set;
        }
        /// <summary>
        /// the difference between the number of byte in one directon and other direction in a conversation / total number of bytes
        /// </summary>
        [DataMember]
        public double ratio2
        {
            get;
            set;
        }
        /// <summary>
        /// the difference between the number of byte in one directon and other direction in a conversation / total number of bytes
        /// </summary>
        [DataMember]
        public double ratio3
        {
            get;
            set;
        }
        /// <summary>
        /// the difference between the number of byte in one directon and other direction in a conversation / total number of bytes
        /// </summary>
        [DataMember]
        public double averrageDifference
        {
            get;
            set;
        }


        #endregion

        #region Non-DataMember Properties

        /// <summary>
        /// The classification result
        /// </summary>
        [IgnoreDataMember]
        public int Type
        {
            get;
            set;
        }

        /// <summary>
        /// Timestamp of when the event is classified.
        /// </summary>
        [IgnoreDataMember]
        public DateTime DetectionTimeStamp
        {
            get;
            set;
        }

        /// <summary>
        /// The machine ip of the logger
        /// </summary>
        [IgnoreDataMember]
        public string LoggerIp
        {
            get;
            set;
        }



        #endregion

        /// <summary>
        /// Parameterless constructor for XML serialization
        /// </summary>
        public ConversationFeature()
        {

        }
              #region Static methods
 
        #endregion
    }
}
