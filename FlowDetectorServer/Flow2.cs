using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace Biotracker.Client.ProcessMonitor
{
    /// <summary>
    /// A Network Flow is used to reprensent a network session.
    /// It has 5 elements of a connection (srcIP, destIP, srcPort, destPort, protocol)
    /// </summary>
    [XmlRoot("Flow", Namespace="http://www.plurilock.com/biotracker", IsNullable=false)]
    [Serializable]
    public class Flow2 : IComparable
    {
        //public enum FlowType
        //{
        //    Normal = 0,
        //    SMTPSpam,
        //    UDPStorm,
        //    Zeus,
        //    ZeusControl,
        //    Num
        //}

        /// <summary>
        /// A serie of packets belongs to a flow.
        /// </summary>
        private List<Packet2> _pktList;
        private Packet2 _firstPacket;
        

        #region Properties
        /// <summary>
        /// ID of the flow
        /// </summary>
        private int _id;

        [XmlElement]
        public int FlowID { get { return _id; } }

        [XmlElement]
        public string ProcessName { get; set; }

        private ConnectionPair2 _conn;
        /// <summary>
        /// ConnectionPair object for a network flow
        /// </summary>
        [XmlElement]
        public ConnectionPair2 Connection
        {
            get
            {
                return _conn;
            }
            set 
            { _conn = value; }
        }

        [XmlIgnore]
        public Packet2.Protocol Protocol
        {
            get
            {
                return Connection.Protocol;
            }
        }

        [XmlIgnore]
        public IPAddress SrcIP
        {
            get
            {
                return Connection.SrcEnd.Address;
            }
        }

        [XmlIgnore]
        public IPAddress DstIP
        {
            get
            {
                return Connection.DestEnd.Address;
            }
        }

        [XmlIgnore]
        public int reconnect =0;


        [XmlIgnore]
        private int NoBackwardPackets = 0;

        public void addBackwardPacket(int NumberOfBackwardPkt) {
            NoBackwardPackets += NumberOfBackwardPkt;
        }



        [XmlIgnore]
        public int SrcPort
        {
            get
            {
                return Connection.SrcEnd.Port;
            }
        }


        [XmlIgnore]
        public int DstPort
        {
            get
            {
                return Connection.DestEnd.Port;
            }
        }
       

        [XmlIgnore]
        public int NbOfPkt 
        { 
            get { return _pktList != null ? _pktList.Count : 0; } 
        }

 

        /// <summary>
        /// The start time of the flow. Here, we use the "receiving" time of the first packet in the flow.
        /// </summary>
        private DateTime _startTime = DateTime.MinValue;
        [XmlElement]
        public DateTime StartTime
        {
            get
            {
                return _pktList.Count > 0 ?
                    (from p in _pktList
                     select p.ReceivingTime).Min()
                     : DateTime.MinValue;  
            }
        }
        /// <summary>
        /// The stop time of the flow. Here, we use the "receiving" time of the last packet in the flow. 
        /// </summary>
        private DateTime _stopTime = DateTime.MinValue;
        [XmlElement]
        public DateTime StopTime
        {
            get
            {
                return _pktList.Count > 0 ?
                    (from p in _pktList
                     select p.ReceivingTime).Max()
                     : DateTime.MinValue;
            }
        }


        /// <summary>
        /// Flow type: Malicious/Normal. 
        /// </summary>
        private int _flowType = 0;
        [XmlElement]
        public int Type
        {
            get { return _flowType; }
            set { _flowType = value; }
        }

        #endregion

        #region Constructors

        public Flow2()
        {
            this.ProcessName = default(string);
        }

        public Flow2(int id, ConnectionPair2 connection)
        {
            this._id = id;

            this._pktList = new List<Packet2>();

            this._conn = connection;

            this.ProcessName = default(string);
        }

        public Flow2(int id, ConnectionPair2 connection, IEnumerable<Packet2> pktList)
        {
            this._id = id;

            this._conn = connection;

            this._pktList = new List<Packet2>();

            this._pktList.AddRange(pktList);

            this.ProcessName = default(string);
        }

        #endregion

        #region Public Methods

        public int getReconnect() {
            return this.reconnect;
        }


        public void AddPacket(Packet2 pkt)
        {
            lock (_pktList)
            {
                this._pktList.Add(pkt);
            }
        }


        /// <summary>
        /// find the first packet in a flow 
        /// </summary>

        public void FirstPacket()
        {
             _firstPacket = _pktList[0];
            foreach (Packet2 p in _pktList)
            {
                _firstPacket = p.ReceivingTime == this.StartTime ? p : _pktList[0];
            }
         
        }


        /// <summary>
        /// calculates the average inter arrival time of packets in a flow  
        /// </summary>
        /// <param name="packets"></param>
        public double AverageIntervalTime(DateTime[] interArrivalTime)
        {
            Array.Sort(interArrivalTime);
            double average = 0;
            double sum = 0;
            for (int i = 0; i < interArrivalTime.Length - 1; i++)
            {
                sum = (interArrivalTime[i + 1] - interArrivalTime[i]).TotalSeconds;
            }
            average = (sum / interArrivalTime.Length - 1);
            return average;
        }


        /// <summary>
        /// Add multiple packets into the 
        /// </summary>
        /// <param name="packets"></param>
        public void AddPacket(IEnumerable<Packet2> packets)
        {
            lock (_pktList)
            {
                this._pktList.AddRange(packets);
            }
        }

        public IEnumerable<Packet2> GetPackets()
        {
            return this._pktList;
        }

   

        /// <summary>
        /// Set the ProcessName parameter by ID.
        /// </summary>
        /// <param name="processID"></param>
        public void SetProcessName(int processID)
        {
            try
            {
                string name = Process.GetProcessById(processID).ProcessName;

                this.ProcessName = string.IsNullOrEmpty(name) ? "<" + processID + ">" : name;
            }
            catch (Exception ex)
            {
                this.ProcessName = "unknown";
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0} {1} {2}-->{3} Owner: {4} {5}pkt ({6})", 
                _conn.ToString(),
                Protocol == Packet2.Protocol.TCP ? "TCP" : "UDP",
                StartTime.ToString("HH:mm:ss.f"),
                StopTime.ToString("HH:mm:ss.f"), 
                ProcessName,
                _pktList.Count,
                Flow2.GetFlowTypeName(this.Type)
            );

            //sb.AppendFormat(" {0} ", Protocol == Packet.Protocol.TCP ? "TCP" : "UDP");
            //sb.AppendFormat("{0} ", StartTime.ToString("HH:mm:ss.f"));
            //sb.AppendFormat("End at {0} ", StopTime.ToString("HH:mm:ss.f"));
            //sb.AppendFormat("Process Name: {0} ", ProcessName);
            //sb.AppendFormat("Total packets: {0} packets ", _pktList.Count);
            //sb.AppendFormat("Total payload: {0} bytes\n", TotalTraffic);
            //sb.AppendFormat("First Packet Size: {0} bytes\n", FPS);
            //sb.AppendFormat("Packet per second: {0:n2} pps\n", PPS);
            //sb.AppendFormat("Average payload length: {0:n2} bytes\n", APL);
            //sb.AppendFormat("Payload Variance : {0:n2} \n", PV);
            return sb.ToString();
        }

  

        /// <summary>
        /// Returns a list of flows 
        /// </summary>
        /// <param name="packets"></param>
        /// <returns></returns>
        public static List<Flow2> GenerateFlows(IEnumerable<Packet2> packets)
        {

           // System.Console.WriteLine("flows2/GenerateFlows");
            SortedList<ConnectionPair2, Flow2> flows = new SortedList<ConnectionPair2, Flow2>();

            int id = 0;

            foreach (Packet2 pkt in packets)
            {
                ConnectionPair2 conn = new ConnectionPair2(pkt);
                ConnectionPair2 revConn = conn.BackwardDirection; // the reverse direction of the flow

                if (!flows.ContainsKey(conn))
                {
                    if (!flows.ContainsKey(revConn))
                        flows.Add(conn, new Flow2(id++, conn));
                    else
                        conn = revConn;
                }

                flows[conn].AddPacket(pkt);
            }

            return flows.Values.ToList();
        }
        
        #endregion

        public int CompareTo(object obj)
        {

           
            if (!(obj is Flow2))
                throw new ArgumentException("Object is not an instance of class Flow.");

            Flow2 flow = (Flow2)obj;

            if (flow.FlowID == this.FlowID)
                return 0;
            else if (FlowID > flow.FlowID)
                return 1;
            else
                return -1;
        }

        public string XmlSerialize()
        {
            
            string ret =default(string);

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Flow2));
                using (MemoryStream ms = new MemoryStream())
                {
                    xs.Serialize(ms, this);

                    using (StreamReader sr = new StreamReader(ms))
                    {
                        ret = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                ;
            }

            return ret;
        }

        public static Flow2 XmlDeserialize(string xml)
        {
            try
            {
                Flow2 flow = (Flow2)XmlDeserialize(xml);

                return flow;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Generate flow features for the flow.The current flow is in a TimeWindow already.
        /// </summary>
        /// <returns></returns>
        public FlowFeature GenerateFeatuesInTimeWindow()  
        {

          //
            
         //   System.Console.WriteLine(" inside GenerateFeatuesInTimeWindow ");
            if (_pktList.Count < 3)
                return null;


            //collecting same length packet
            SortedList<uint, int> samePktLength = new SortedList<uint, int>();
            int pktEx = 0;
            long totalPayloadLength = 0;
            long totalTCPUDPPayloadLength = 0;
            int pktExTCPUDP = 0;
            int noNullPacket = 0;
            int noSmallPacket = 0;
            double timeWindow = new TimeSpan(0, 0, Properties.Settings.Default.TimeWindow).TotalSeconds;


            FlowFeature flowFeature = new FlowFeature();

            flowFeature.SrcIP = this._conn.SrcEnd.Address.GetAddressBytes();
            flowFeature.SrcPort = this._conn.SrcEnd.Port;

            flowFeature.DestIP = this._conn.DestEnd.Address.GetAddressBytes();
            flowFeature.DestPort = this._conn.DestEnd.Port;

            //getting the MAC address of the first packet for the flow
            //we assume that the MAC address is consistant per flow
            flowFeature.SrcMAC = this._pktList[0].SrcMAC;
            flowFeature.DestMAC = this._pktList[0].DestMAC;
            
            flowFeature.Protocol = (int)(this.Protocol);
            flowFeature.Reconnect = this.reconnect;

            flowFeature.FPS = (double)_pktList[0].PayloadLength; /// should be revised
           // flowFeature.tcpFlag = 0;

            flowFeature.DetectionTimeStamp = _pktList[0].ReceivingTime;

            

            DateTime[] interArrivalTime = new DateTime[_pktList.Count];
            uint noIncommingPackets = 0;
            uint noOutgoingPackets = 0;

            for (int i = 0; i < _pktList.Count; i++)
            {
                totalPayloadLength += _pktList[i].PayloadLength;
                pktEx++;
                
                interArrivalTime[i] = _pktList[i].ReceivingTime;

                if (_pktList[i].SrcEnd.Address == _conn.SrcEnd.Address)
                {
                    noIncommingPackets++;
                }
                else
                    noOutgoingPackets++;

                Packet2.Protocol temp = new Packet2.Protocol();
                if (temp != Packet2.Protocol.Mixed)//to capture TCP/UDP flows
                { //either TCP/UDP
                    totalTCPUDPPayloadLength += _pktList[i].PayloadLength;
                    pktExTCPUDP++;
                }

                if (_pktList[i].PayloadLength == 0)
                {
                    noNullPacket += 1;
                }

                if (63 < _pktList[i].PayloadLength && _pktList[i].PayloadLength < 399) // counting th enumber of small packets
                {
                    noSmallPacket += 1;
                }

                if (samePktLength.ContainsKey(_pktList[i].PayloadLength))
                {
                    samePktLength[_pktList[i].PayloadLength] += 1;
                }
                else
                {
                    samePktLength.Add(_pktList[i].PayloadLength, 1);
                }
            }

            ////
            
            //////


           // flowFeature.Type = Flow2.GetLabelIndex(this._conn.SrcEnd.Address.GetAddressBytes());
            flowFeature.Type = Flow2.GetLabelIndex_ME(this._conn, flowFeature.SrcMAC, flowFeature.DestMAC);

//            if (flowFeature.Type == 3 || flowFeature.Type == 4 || flowFeature.Type == 5)
//                System.Diagnostics.Debug.WriteLine(flowFeature.ToString());
            flowFeature.PX = (double)pktEx;
            flowFeature.APL = (double)(totalPayloadLength) / (double)pktEx;
           // flowFeature.PX = (double)pktEx;
            flowFeature.PV = CalVariance(samePktLength, flowFeature.APL);
            flowFeature.DPL = (double)(samePktLength.Count) / pktEx;

            flowFeature.AB = (double)totalTCPUDPPayloadLength / (double)pktExTCPUDP;
            flowFeature.TBT = totalPayloadLength;

            flowFeature.BS = (double)totalPayloadLength / (double)timeWindow;
            flowFeature.PS = (double)pktEx / (double)timeWindow;

            flowFeature.NNP = noNullPacket;
            flowFeature.NSP = noSmallPacket;
            flowFeature.PSP = ((double)noSmallPacket / (double)pktEx) * 100;

            flowFeature.Duration = (this.StopTime-this.StartTime).TotalSeconds;

            flowFeature.AIT = AverageIntervalTime(interArrivalTime);

            if (this.NoBackwardPackets != 0)
            {
                flowFeature.IOPR = (double)this.NbOfPkt / (double)this.NoBackwardPackets;
            }
            else
                flowFeature.IOPR = -1;
         //   flowFeature.SDNP = Variance(samePktLength, flowFeature.APL);

            flowFeature.PPS = pktEx < 2 ?
                    0.0
                    : (double)(pktEx) * 1000.0d
                    /(_pktList.Last().ReceivingTime - _pktList.First().ReceivingTime).TotalSeconds;

            if (Double.IsInfinity(flowFeature.PPS))
                flowFeature.PPS = 0.0d;


            // we do reseat all the calculation to be used by next flow because the function calling this is in a loop 
            samePktLength.Clear();
            samePktLength = null;
           // _pktList.Clear();

            return flowFeature;
        }

        internal double CalVariance(SortedList<uint, int> slist, double mean)
        {
            double sum = 0.0d;
            int cnt = 0;
            foreach (KeyValuePair<uint, int> kv in slist)
            {
                sum += ((double)(kv.Key) - mean) * ((double)(kv.Key) - mean) * kv.Value;
                cnt += kv.Value;
            }

            return sum / cnt;
        }

        public void Dispose()
        {
            this._conn = null;
            this._pktList.Clear();
            this._pktList = null;
        }
           

        #region Static  Methods

        public static IEnumerable<string> GetFlowTypeNames()
        {
            //return new string[] { "Normal", "SMTPSpam", "UDPStorm", "Zeus", "ZeusControl" };
            if (_labelMap == null)
                throw new InvalidOperationException("Flow label table is not set yet.");

            List<string> labels = new List<string>();

            for(int i=0; i < _labelMap.Count; i++)
            {
                labels.Add(_labelMap[i]);
            }

            return labels;
        }

        private static SortedList<String, int> _labelIndex = null;
        private static SortedList<int, string> _labelMap = null;
        public static void SetLabelTable(IEnumerable<KeyValuePair<string, string>> labelTable)
        {
            int i = 0;

            _labelIndex = new SortedList<string, int>();

            _labelMap = new SortedList<int, string>();

            _labelMap.Add(0, "Normal");

            foreach (KeyValuePair<string, string> kv in labelTable)
            {
                i++;
                _labelIndex.Add(kv.Value, i);
                _labelMap.Add(i, kv.Key);
            }
        }



        public static int GetLabelIndex_ME(ConnectionPair2 conn, byte [] srcMAc, byte [] dstMAC) {
           

            ///we first chech the value by IP

            byte[] srcIP = conn.SrcEnd.Address.GetAddressBytes();
            byte[] dstIP = conn.DestEnd.Address.GetAddressBytes();
            StringBuilder SRCip = new StringBuilder();
            StringBuilder DSTip = new StringBuilder();
            int index = 0;
          //  int indexDST = 0;

            for (int i = 0; i < 3; i++)
            {
                SRCip.Append(srcIP[i].ToString());
                SRCip.Append(".");
            }

            SRCip.Append(srcIP[3].ToString());


            if (_labelIndex.TryGetValue(SRCip.ToString(), out index))
                return index;

            else /// search by dest ip

            for (int i = 0; i < 3; i++)
            {
                DSTip.Append(dstIP[i].ToString());
                DSTip.Append(".");
            }

            DSTip.Append(dstIP[3].ToString());


            if (_labelIndex.TryGetValue(DSTip.ToString(), out index))
                return index;

           // search by src mac 

            /// then check by mac if it is not in IP labels
            StringBuilder SRCmac = new StringBuilder();
            StringBuilder DSTmac = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                SRCmac.Append(srcMAc[i].ToString("X2"));
                SRCmac.Append(":");
            }

            SRCmac.Append(srcMAc[5].ToString("X2"));

            if (SRCmac.ToString().CompareTo(SRCmac.ToString().ToUpper()) != 0)
                System.Diagnostics.Debug.WriteLine(SRCmac.ToString() + "<==>" + SRCmac.ToString().ToUpper());


            if (_labelIndex.TryGetValue(SRCmac.ToString().ToUpper(), out index))
                return index;
            
            ///search by dest mac
            for (int i = 0; i < 5; i++)
            {
                DSTmac.Append(srcMAc[i].ToString("X2"));
                DSTmac.Append(":");
            }

            DSTmac.Append(srcMAc[5].ToString("X2"));

            if (DSTmac.ToString().CompareTo(DSTmac.ToString().ToUpper()) != 0)
                System.Diagnostics.Debug.WriteLine(DSTmac.ToString() + "<==>" + DSTmac.ToString().ToUpper());

           
            if (_labelIndex.TryGetValue(DSTmac.ToString().ToUpper(), out index))
                return index;

            else 
                return 0;
        }


        public static int GetLabelIndex(byte[] SrcIP)
        {
            // when there is no Label table set, return all type as Normal.
            if (_labelIndex == null)
                return 0;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 3; i++)
            {
                sb.Append(SrcIP[i].ToString());
                sb.Append(".");
            }

            sb.Append(SrcIP[3].ToString());

            if (sb.ToString().CompareTo(sb.ToString().ToUpper()) != 0)
                System.Diagnostics.Debug.WriteLine(sb.ToString() + "<==>" + sb.ToString().ToUpper());

            int index = 0;
            if (_labelIndex.TryGetValue(sb.ToString().ToUpper(), out index))
                return index;
            else
                return 0;

        }

        public static string GetFlowTypeName(int index)
        {
            if(_labelMap == null)
                throw new InvalidOperationException("Flow label table is not set yet.");

            return _labelMap[index];
        }

        #endregion

    }

  

    public class FlowComparer : IEqualityComparer<Flow2>
    {

        public bool Equals(Flow2 x, Flow2 y)
        {
            if (Object.ReferenceEquals(x, y))
                return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Connection.Equals(y.Connection);

        }

        public int GetHashCode(Flow2 obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            int hashConnection = obj.Connection == null ? 0 : obj.Connection.GetHashCode();

            int hashProcessname = obj.ProcessName == null ? 0 : obj.ProcessName.GetHashCode();

            int hashStartTime = obj.StartTime == null ? 0 : obj.StartTime.GetHashCode();

            return hashConnection ^ hashProcessname ^ hashStartTime;
        }


       
        
    }
}
