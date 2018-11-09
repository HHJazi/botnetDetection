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
    public class FlowFeature
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

        [DataMember]
        public int SrcPort
        {
            get;
            set;
        }

        [DataMember]
        public int DestPort
        {
            get;
            set;
        }

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
        /// total number of bytes per flow
        /// </summary>
        [DataMember]
        public long TBT
        {
            get;
            set;
        }
        


        /// <summary>
        /// First Packet size
        /// </summary>
        [DataMember]
        public double FPS
        {
            get;
            set;
        }

        /// <summary>
        /// Average payload length.
        /// </summary>
        [DataMember]
        public double APL
        {
            get;
            set;
        }

        /// <summary>
        /// Average byte per packet in each TCP/UDP flow
        /// </summary>
        [DataMember]
        public double AB
        {
            get;
            set;
        }

        /// <summary>
        /// Average bits per second
        /// </summary>
        [DataMember]
        public double BS
        {
            get;
            set;
        }

        /// <summary>
        /// Average packet per second
        /// </summary>
        [DataMember]
        public double PS
        {
            get;
            set;
        }

        /// <summary>
        /// Same packet length ratio
        /// </summary>
        [DataMember]
        public double DPL
        {
            get;
            set;
        }

        /// <summary>
        /// Packet rate (pkt/second)
        /// </summary>
        [DataMember]
        public double PPS
        {
            get;
            set;
        }

        /// <summary>
        /// Payload length vaiance.
        /// </summary>
        [DataMember]
        public double PV
        {
            get;
            set;
        }

        /// <summary>
        /// The number of packets exchanged.
        /// </summary>
        [DataMember]
        public double PX
        {
            get;
            set;
        }

        /// <summary>
        /// The number of null packet exchanged.
        /// </summary>
        [DataMember]
        public int NNP
        {
            get;
            set;
        }

        /// <summary>
        /// The number of small packet exchanged.
        /// </summary>
        [DataMember]
        public int NSP
        {
            get;
            set;
        }

        /// <summary>
        /// The percentage of small packet exchanged.
        /// </summary>
        [DataMember]
        public double PSP
        {
            get;
            set;
        }

        /// <summary>
        /// flow duration, difference beween receipt time of the first and last packet
        /// </summary>
        [DataMember]
        public double Duration
        {
            get;
            set;
        }

        /// <summary>
        /// tcp flags, is null if it is not tcp 
        /// </summary>
        [DataMember]
        public byte[] tcpFlag
        {
            get;
            set;
        }


        /// <summary>
        /// tcp flags, is null if it is not tcp 
        /// </summary>
        [DataMember]
        public int Reconnect
        {
            get;
            set;
        }

        /// <summary>
        /// average inter arrival time of packets within a flow 
        /// </summary>
        [DataMember]
        public double AIT
        {
            get;
            set;
        }

        /// <summary>
        /// ratio of incomming packets over outgoing packets in a flow 
        /// </summary>
        [DataMember]
        public double IOPR
        {
            get;
            set;
        }

        /// <summary>
        /// standard deviation of number of packets in a flow 
        /// </summary>
        [DataMember]
        public double SDNP
        {
            get;
            set;
        }


        [DataMember]
        public uint NoIncommingPkt
        {
            get;
            set;
        }


        [DataMember]
        public uint NoOutgoingPkt
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
        public FlowFeature()
        {

        }

        #region Static methods
 
        #endregion

        #region Public Methods
        public string featuresToString()
        {
            String features = "";
            String SrcIP2="";
            String DestIP2 = "";
            String SrcMAC2 = "";
            String DestMAC2 = "";

            Utilities convertor = new Utilities();

            if (SrcMAC != null)
                SrcMAC2 = convertor.convertMACtoString(SrcMAC);

            if (DestMAC != null)
                DestMAC2 = convertor.convertMACtoString(DestMAC);
                    //DestMAC[0].ToString() + "-" + DestMAC[1].ToString() + "-" + DestMAC[2].ToString() + "-" + DestMAC[3].ToString()+ "-" + DestMAC[4].ToString() + "-" + DestMAC[5].ToString();

            if (SrcIP != null)
                SrcIP2 = convertor.convertIPtoString(SrcIP);
                //SrcIP[0].ToString() + "." + SrcIP[1].ToString() + "." + SrcIP[2].ToString()+"." + SrcIP[3].ToString();
           
            if (DestIP != null)
                DestIP2 = convertor.convertIPtoString(DestIP);
          

            features += SrcMAC2 + "," + DestMAC2 + "," + SrcIP2 + "," + DestIP2 + "," + SrcPort.ToString() + "," + DestPort.ToString() + "," + Protocol.ToString() + "," + TBT.ToString() + "," + FPS.ToString() + "," +
                APL.ToString() + ","  + AB.ToString()+ "," + BS.ToString()+ "," + PS.ToString()+ "," + DPL.ToString() + "," +PPS.ToString()+ "," + PV.ToString()+ "," + PX.ToString()+ "," +
            NNP.ToString() + "," + NSP.ToString() + "," + PSP.ToString() + "," + Duration.ToString() + "," + this.Type.ToString() + "," + IOPR+ ","+ AIT + "," + Reconnect;

            return features;
        }
        #endregion
    }
}
