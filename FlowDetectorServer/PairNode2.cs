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
    /// A pairNode is used to reprensent a pair of nodes 
    /// It has 2 elements of a connection (srcIP, destIP)
    /// </summary>
    [XmlRoot("PairNode", Namespace = "http://www.plurilock.com/biotracker", IsNullable = false)]
    [Serializable]
    public class PairNode2 : IComparable
    {

        /// <summary>
        /// A serie of flows belomgs to a pair of nodes
        /// </summary>
        private List<Flow2> _flowList;

        #region Properties
        /// <summary>
        /// ID of the pair
        /// </summary>
        private int _id;

        [XmlElement]
        public int PairID { get { return _id; } }

        [XmlElement]
        public string ProcessName { get; set; }

        private PrimaryConnection2 _pairConn;
        /// <summary>
        /// ConnectionPair object for a network flow
        /// </summary>
        [XmlElement]
        public PrimaryConnection2 PairConnection
        {
            get
            {
                return _pairConn;
            }
            set
            { _pairConn = value; }
        }

        //[XmlIgnore]
        //public Packet2.Protocol Protocol
        //{
        //    get
        //    {
        //        return Connection.Protocol;
        //    }
        //}
        [XmlIgnore]
        List<Flow2> FlowList {
            get {
                return _flowList;
            }
        }

        [XmlIgnore]
        public IPAddress SrcIP
        {
            get
            {
                return PairConnection.SrcIP;
            }
        }

        [XmlIgnore]
        public IPAddress DstIP
        {
            get
            {
                return PairConnection.DestIP;
            }
        }



        /// <summary>
        /// size of each pairnode in packet
        /// </summary>
        private List<int> _NbOfPkt;

        [XmlIgnore]
        public List<int> NbOfPkt
        {

            get
            {
                for (int index = 0; index < _flowList.Count; index++)
                    _NbOfPkt[index] = _flowList[index].NbOfPkt;
                return _NbOfPkt;
            }
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
                return _flowList.Count > 0 ?
                    (from f in _flowList
                     select f.StartTime).Min()
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
                return _flowList.Count > 0 ?
                     (from f in _flowList
                      select f.StopTime).Max()
                      : DateTime.MinValue;
            }
        }



        /// <summary>
        /// pairnode type: Malicious/Normal. 
        /// </summary>
        private int _pairNodeType=0;
        [XmlElement]
        public int PairNodeType
        {
            get { return _pairNodeType; }
            set { _pairNodeType = value; }
        }

        #endregion

        #region Constructors

        public PairNode2()
        {
            this.ProcessName = default(string);
        }

        public PairNode2(int id, PrimaryConnection2 pairConnection)
        {
            this._id = id;

            this._flowList = new List<Flow2>();

            this._pairConn = pairConnection;

            this.ProcessName = default(string);
        }

        public PairNode2(int id, PrimaryConnection2 connection, IEnumerable<Flow2> flowList)
        {
            this._id = id;

            this._pairConn = connection;

            this._flowList = new List<Flow2>();

            this._flowList.AddRange(flowList);

            this.ProcessName = default(string);
        }

        #endregion

        #region Public Methods
        public void AddFlow(Flow2 flow)
        {
            lock (_flowList)
            {
                this._flowList.Add(flow);
            }
        }


        /// <summary>
        /// find the first packet in a flow 
        /// </summary>

        //public void FirstPacket()
        //{
        //     _firstPacket = _pktList[0];
        //    foreach (Packet2 p in _pktList)
        //    {
        //        _firstPacket = p.ReceivingTime == this.StartTime ? p : _pktList[0];
        //    }

        //}


        /// <summary>
        /// calculates the average inter arrival time of flow in a pairnode  
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
        public void AddFlow(IEnumerable<Flow2> flows)
        {
            lock (_flowList)
            {
                this._flowList.AddRange(flows);
            }
        }

        public IEnumerable<Flow2> GetFlows()
        {
            return this._flowList;
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

            sb.AppendFormat("{0} {1}-->{2} Owner: {3} {4}flow",
                _pairConn.ToString(),
                // Protocol == Packet2.Protocol.TCP ? "TCP" : "UDP",
                StartTime.ToString("HH:mm:ss.f"),
                StopTime.ToString("HH:mm:ss.f"),
                ProcessName,
                _flowList.Count
                //      Flow2.GetFlowTypeName(this.Type)
            );

            return sb.ToString();
        }



        /// <summary>
        /// Returns a list of flows 
        /// </summary>
        /// <param name="packets"></param>
        /// <returns></returns>
        public static List<PairNode2> GeneratePairNodes(IEnumerable<Flow2> flows)
        {

            //System.Console.WriteLine("flows2/GenerateFlows");
            SortedList<PrimaryConnection2, PairNode2> pairNodes = new SortedList<PrimaryConnection2, PairNode2>();

            int id = 0;

            foreach (Flow2 f in flows)
            {
                PrimaryConnection2 conn = new PrimaryConnection2(f);
                PrimaryConnection2 revConn = conn.BackwardDirection; // the reverse direction of the flow

                if (!pairNodes.ContainsKey(conn))
                {
                    if (!pairNodes.ContainsKey(revConn))
                        pairNodes.Add(conn, new PairNode2(id++, conn));
                    else
                        conn = revConn;
                }

                pairNodes[conn].AddFlow(f);
            }

            return pairNodes.Values.ToList();
        }

        #endregion

        public int CompareTo(object obj)
        {


            if (!(obj is PairNode2))
                throw new ArgumentException("Object is not an instance of class PairNode.");

            PairNode2 pair = (PairNode2)obj;

            if (pair.PairID == this.PairID)
                return 0;
            else if (PairID > pair.PairID)
                return 1;
            else
                return -1;
        }

        public string XmlSerialize()
        {

            string ret = default(string);

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(PairNode2));
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

        public static PairNode2 XmlDeserialize(string xml)
        {
            try
            {
                PairNode2 pair = (PairNode2)XmlDeserialize(xml);

                return pair;
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
        public PairNodeFeature GenerateFeatuesInTimeWindow()
        {

            //collecting same length packet
            SortedList<double, int> sameFlowLength = new SortedList<double, int>();

            int flEX = 0; // number of flows created between two nodes
            int pktEx = 0; // number of packets exchanged between a pair of nodes
            double totalPayloadLength = 0;
            long totalTCPUDPPayloadLength = 0;
            int pktExTCPUDP = 0;
            int noNullPacket = 0;
            int noSmallPacket = 0;
            double timeWindow = new TimeSpan(0, 0, Properties.Settings.Default.TimeWindow).TotalSeconds;


            PairNodeFeature pairFeature = new PairNodeFeature();

            pairFeature.SrcIP = this._pairConn.SrcIP.GetAddressBytes();
            //   pairFeature.SrcPort = this._conn.SrcEnd.Port;

            pairFeature.DestIP = this._pairConn.DestIP.GetAddressBytes();
            //     pairFeature.DestPort = this._conn.DestEnd.Port;

            //getting the MAC address of the first packet for the flow
            //we assume that the MAC address is consistant per flow
            pairFeature.SrcMAC = this._flowList[0].GetPackets().First().SrcMAC;

            pairFeature.DestMAC = this._flowList[0].GetPackets().First().DestMAC;



            DateTime[] interArrivalTime = new DateTime[_flowList.Count];
            int noIncommingFlows = 0;
            int noOutgoingFlows = 0;

            uint NoOutgoingPkt = 0; // from dest to src
            uint NoIncommingPkt = 0; // from node src to dest



            for (int i = 0; i < _flowList.Count; i++)
            {
                double flowLength = 0; 
                FlowFeature flowFeature = _flowList[i].GenerateFeatuesInTimeWindow();


                NoIncommingPkt += flowFeature.NoIncommingPkt;


                flowLength = (double)_flowList[i].NbOfPkt * flowFeature.APL;  //total payload leangth of that flow
                totalPayloadLength += flowLength;
                pktEx += _flowList[i].NbOfPkt;

                interArrivalTime[i] = flowFeature.DetectionTimeStamp;   // arrival time of each flow = recieving time of first packet in that flow

                if (_flowList[i].SrcIP == _pairConn.SrcIP)
                {
                    noIncommingFlows++;
                }
                else
                    noOutgoingFlows++;

                if (sameFlowLength.ContainsKey(flowLength))
                {
                    sameFlowLength[flowLength] += 1;
                }
                else
                {
                    sameFlowLength.Add(flowLength, 1);
                }
            }

     


            pairFeature.Type = GetLabelIndex(_flowList[0].SrcIP.GetAddressBytes());

            pairFeature.NofFlows = _flowList.Count;



            pairFeature.ConnectionDegree = pairFeature.NoIncommingPkt + pairFeature.NoOutgoingPkt + _flowList.Count;



            // we do reseat all the calculation to be used by next flow because the function calling this is in a loop 
            sameFlowLength.Clear();
            sameFlowLength = null;
            _flowList.Clear();


            return pairFeature;
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
            this._pairConn = null;
            this._flowList.Clear();
            this._flowList = null;
        }


        #region Static  Methods

        public static IEnumerable<string> GetFlowTypeNames()
        {
            //return new string[] { "Normal", "SMTPSpam", "UDPStorm", "Zeus", "ZeusControl" };
            if (_labelMap == null)
                throw new InvalidOperationException("Flow label table is not set yet.");

            List<string> labels = new List<string>();

            for (int i = 0; i < _labelMap.Count; i++)
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
            if (_labelMap == null)
                throw new InvalidOperationException("Flow label table is not set yet.");

            return _labelMap[index];
        }

        #endregion

    }



    //public class FlowComparer : IEqualityComparer<Flow2>
    //{

    //    public bool Equals(Flow2 x, Flow2 y)
    //    {
    //        if (Object.ReferenceEquals(x, y))
    //            return true;

    //        if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
    //            return false;

    //        return x.Connection.Equals(y.Connection);

    //    }

    //    public int GetHashCode(Flow2 obj)
    //    {
    //        if (Object.ReferenceEquals(obj, null)) return 0;

    //        int hashConnection = obj.Connection == null ? 0 : obj.Connection.GetHashCode();

    //        int hashProcessname = obj.ProcessName == null ? 0 : obj.ProcessName.GetHashCode();

    //        int hashStartTime = obj.StartTime == null ? 0 : obj.StartTime.GetHashCode();

    //        return hashConnection ^ hashProcessname ^ hashStartTime;
    //    }
    //}
}
