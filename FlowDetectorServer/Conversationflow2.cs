using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace Biotracker.Client.ProcessMonitor
{
    /// <summary>
    /// A Network Flow is used to reprensent a network session.
    /// 
    /// </summary>
    [XmlRoot("Flow", Namespace = "http://www.plurilock.com/biotracker", IsNullable = false)]
    [Serializable]
    public class Conversationflow2: IComparable
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
        // private List<Packet> _pktList;
        private List<Flow2> _flows;
        #region Properties
        /// <summary>
        /// ID of the Conversation
        /// </summary>
        private int _id;

        [XmlElement]
        public int ConversationID { get { return _id; } }

        [XmlElement]
        public string ProcessName { get; set; }

        private ConversationPair _conn;
        /// <summary>
        /// ConversationPair object for a network flow
        /// </summary>
        [XmlElement]
        public ConversationPair Connection
        {
            get
            {
                return _conn;
            }
            set 
            { _conn = value; }
        }

        [XmlIgnore]
        public Packet.Protocol Protocol
        {
            get
            {
                return Connection.Protocol;
            }
        }

        private List<int> _NbOfPkt;
        [XmlIgnore]
        public List<int> NbOfPkt 
        {

            get
            {
                for (int i = 0; i < _flows.Count; i++)
                    _NbOfPkt[i] = _flows[i].NbOfPkt;
                return NbOfPkt;
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
                return _flows.Count > 0 ?
                    (from f in _flows
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
                return _flows.Count > 0 ?
                    (from f in _flows
                     select f.StopTime).Max()
                     : DateTime.MinValue;
            }
        }

  
        /// <summary>
        /// Flow type: Malicious/Normal. 
        /// </summary>
        private int _ConversationflowType = 0;
        [XmlElement]
        public int Type
        {
            get { return _ConversationflowType; }
            set { _ConversationflowType = value; }
        }

        #endregion

        #region Constructors

        public Conversationflow2()
        {
            this.ProcessName = default(string);
        }

        public Conversationflow2(int id, ConversationPair connection)
        {
            this._id = id;

            this._flows = new List<Flow2>();

            this._conn = connection;

            this.ProcessName = default(string);
        }

        public Conversationflow2(int id, ConversationPair connection, IEnumerable<Flow2> flwList)
        {
            this._id = id;

            this._conn = connection;

            this._flows = new List<Flow2>();

            this._flows.AddRange(flwList);

            this.ProcessName = default(string);
        }

        #endregion

        #region Public Methods
        public void AddFlow(Flow2 Flw)
        {
            lock (_flows)
            {
                this._flows.Add(Flw);
            }
        }

        /// <summary>
        /// Add multiple packets into the 
        /// </summary>
        /// <param name="packets"></param>
        public void AddFlow(IEnumerable<Flow2> flows)
        {
            lock (_flows)
            {
                this._flows.AddRange(flows);
            }
        }

        public IEnumerable<Flow2> GetFlows()
        {
            return this._flows;
        }

        /// <summary>
        /// Generate a list of FlowFeature objects
        /// </summary>
        /// <param name="timeWindow"></param>
        /// <returns></returns>
     /*   public IEnumerable<FlowFeature> GenerateFeatures(TimeSpan timeWindow)
        {
            List<FlowFeature> features = new List<FlowFeature>();
            if (_pktList.Count == 0)
                return features;
            System.Console.WriteLine("avalies");
            uint firstPktSize = _pktList[0].PayloadLength;
            DateTime startTime = _pktList[0].ReceivingTime;
            //collecting same length packet
            SortedList<uint, int> samePktLength = new SortedList<uint, int>();
            //Exchanged packet number
            int pktEx = 0;
            // Total payload length
            uint totalPayloadLength = 0;

            for (int i = 0; i< _pktList.Count; i++)
            {
                Packet pkt = _pktList[i];

                if ((pkt.ReceivingTime - startTime).TotalSeconds <= timeWindow.TotalSeconds)
                {
                    totalPayloadLength += pkt.PayloadLength;
                    pktEx++;

                    if (samePktLength.ContainsKey(pkt.PayloadLength))
                    {
                        samePktLength[pkt.PayloadLength] += 1;
                    }
                    else
                    {
                        samePktLength.Add(pkt.PayloadLength, 1);
                    }

                    if (i < _pktList.Count - 1)
                        continue;
                }

                FlowFeature flowFeature = new FlowFeature();
                flowFeature.Protocol = (int)(this.Protocol);

                flowFeature.FPS = (double)firstPktSize;
                flowFeature.PX = (double)pktEx;
                
                flowFeature.APL = (double)(pkt.PayloadLength) / (double)pktEx;

                //flowFeature.StartTime = startTime;

                flowFeature.DPL = (double)(samePktLength.Count) / pktEx;

                flowFeature.PPS = pktEx < 2 ? 
                    0.0 
                    : (double)(pktEx) * 1000.0d / (pkt.ReceivingTime - startTime).TotalSeconds;  

                if (Double.IsInfinity(flowFeature.PPS))
                    flowFeature.PPS = 0.0d;

                flowFeature.PV =  CalVariance(samePktLength, flowFeature.APL);

                flowFeature.Type = this.Type;

                //Add feature to the list;
                features.Add(flowFeature);

                //Reset a new flow
                firstPktSize = pkt.PayloadLength; //reset the first packet
              
                totalPayloadLength = pkt.PayloadLength;
                
                pktEx = 1;

                samePktLength = new SortedList<uint, int>();
                samePktLength.Add(pkt.PayloadLength, 1);
                
                startTime = pkt.ReceivingTime;
            }

            return features;
        }
*/
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
                Protocol == Packet.Protocol.TCP ? "TCP" : "UDP",
                StartTime.ToString("HH:mm:ss.f"),
                StopTime.ToString("HH:mm:ss.f"), 
                ProcessName,
                _flows.Count,
                Conversationflow2.GetFlowTypeName(this.Type)
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
   /*     public static List<Flow2> GenerateFlows(IEnumerable<Packet> packets)
        {
            SortedList<ConnectionPair, Flow2> flows = new SortedList<ConnectionPair, Flow2>();

            int id = 0;

            foreach (Packet pkt in packets)
            {
                ConnectionPair conn = new ConnectionPair(pkt);
                ConnectionPair revConn = conn.BackwardDirection; // the reverse direction of the flow

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
    */

        /// <summary>
        /// Returns a list of conversations
        /// was created by hossein
        /// </summary>
        /// <param name="packets"></param>
        /// <returns></returns>
        public static List<Conversationflow2> GenerateConversations(IEnumerable<Flow2> flows)
        {
            SortedList<ConversationPair, Conversationflow2> conversationflows = new SortedList<ConversationPair, Conversationflow2>();

            int id = 0;

            foreach (Flow2 flw in flows)
            {
                ConversationPair conn = new ConversationPair(flw);
                ConversationPair revConn = conn.BackwardDirection; // the reverse direction of the flow

                if (!conversationflows.ContainsKey(conn))
                {
                    if (!conversationflows.ContainsKey(revConn))
                        conversationflows.Add(conn, new Conversationflow2(id++, conn));
                    else
                        conn = revConn;
                }

                conversationflows[conn].AddFlow(flw);
            }

            return conversationflows.Values.ToList();
        }
        #endregion

        public int CompareTo(object obj)
        {
            if (!(obj is Conversationflow2))
                throw new ArgumentException("Object is not an instance of class Flow.");

            Conversationflow2 conversationflow = (Conversationflow2)obj;

            if (conversationflow.ConversationID == this.ConversationID)
                return 0;
            else if (ConversationID > conversationflow.ConversationID)
                return 1;
            else
                return -1;
        }

        public string XmlSerialize()
        {
            
            string ret =default(string);

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Conversationflow2));
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

        public static Conversationflow2 XmlDeserialize(string xml)
        {
            try
            {
                Conversationflow2 conversationflow = (Conversationflow2)XmlDeserialize(xml);

                return conversationflow;
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
        public ConversationFeature GenerateConversationFeatuesInTimeWindow()
        {     
            ConversationFeature conversationFlowFeature = new ConversationFeature();
            conversationFlowFeature.SrcIP = this._conn.SrcIP.GetAddressBytes();

            conversationFlowFeature.DestIP = this._conn.DestIP.GetAddressBytes();
            conversationFlowFeature.SrcMAC = this._flows[0].GetPackets().First().SrcMAC;
            conversationFlowFeature.DestMAC = this._flows[0].GetPackets().First().DestMAC;

            conversationFlowFeature.Protocol = (int)(this.Protocol);

            conversationFlowFeature.sizeofFirstPkt = _flows[0].GetPackets().First().PayloadLength;
            //flowFeature.DetectionTimeStamp = _pktList[0].ReceivingTime;

           //total number of packet transmitted in the conversation
           

            for (int i = 0; i < _flows.Count; i++)
            {
                conversationFlowFeature.PPC += _flows[i].NbOfPkt;

            }
            

            uint totalPayloadLength = 0;
            uint PayloadLengthSmaller146 = 0;
            uint PayloadLengthLarger146 = 0;
            int pktEx = 0;
            int less = 0;
            int large = 0;
            int noIncomingPkts = 0;
            int noOutgoingPkt = 0;
            uint noIncomingByte = 0;
            uint noOutgoingByte = 0;
            for (int j = 0; j < _flows.Count; j++)
            {
                for (int i = 0; i < _flows[j].NbOfPkt; i++)
                {
                    totalPayloadLength += _flows[j].GetPackets().ElementAt(i).PayloadLength;
                    pktEx++;
                    if (_flows[j].GetPackets().ElementAt(i).PayloadLength < 146)
                    {
                        PayloadLengthSmaller146 += _flows[j].GetPackets().ElementAt(i).PayloadLength;
                        less++;
                    }
                    else if (_flows[j].GetPackets().ElementAt(i).PayloadLength > 146)
                    {
                       PayloadLengthLarger146+= _flows[j].GetPackets().ElementAt(i).PayloadLength;
                       large++;
                    }
                    if (_flows[j].GetPackets().ElementAt(i).SrcEnd.Address.GetAddressBytes() == _conn.SrcIP.GetAddressBytes())
                    {
                        noIncomingPkts++;
                        noIncomingByte += _flows[j].GetPackets().ElementAt(i).PayloadLength;
                    }
                    else
                    {
                        noOutgoingPkt++;
                        noOutgoingByte += _flows[j].GetPackets().ElementAt(i).PayloadLength;
                    }

                }
            }
            conversationFlowFeature.NPS146 = PayloadLengthSmaller146;
            conversationFlowFeature.NPL146 = PayloadLengthLarger146;
            conversationFlowFeature.PPL146 = (double)PayloadLengthLarger146 / (double)large;
            conversationFlowFeature.PPS146 = (double)PayloadLengthSmaller146 / (double)less;
            conversationFlowFeature.BPC = totalPayloadLength;
            conversationFlowFeature.ALP = (double)totalPayloadLength / (double)pktEx;
            conversationFlowFeature.differences = Math.Abs(noIncomingPkts - noOutgoingPkt);
            conversationFlowFeature.byteDifferences = (uint)Math.Abs(noIncomingByte - noOutgoingByte);
            conversationFlowFeature.ratioPkts = (double)conversationFlowFeature.differences / (double)conversationFlowFeature.PPC;
            conversationFlowFeature.ratioBytes = (double)conversationFlowFeature.byteDifferences / (double)conversationFlowFeature.BPC;
            conversationFlowFeature.Type = Flow2.GetLabelIndex(this._flows[0].GetPackets().First().SrcMAC);
            double temp = (double)noIncomingPkts / (double)noOutgoingPkt;
            conversationFlowFeature.ratio1 = (double) temp + ((double)1/ (double)temp);
            double temp1 = (double)noIncomingByte / (double)noOutgoingByte;
            conversationFlowFeature.ratio2 = (double)temp1 + ((double)1 / (double)temp1);
            double temp2 = (double)conversationFlowFeature.BPC / (double)conversationFlowFeature.PPC;
            conversationFlowFeature.ratio3 = (double)temp2 + ((double)1 / (double)temp2);
            double temp3 = (double) noIncomingByte/ (double) noIncomingPkts;
            double temp4 = (double) noOutgoingByte/ (double)noOutgoingPkt;
            conversationFlowFeature.averrageDifference = (uint)Math.Abs(temp3 - temp4);
           



            _flows.Clear();

            return conversationFlowFeature;
      
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
         //   this._pktList.Clear();
         //   this._pktList = null;
        }
           

        #region Static  Methods

        public static IEnumerable<string> GetConversationFlowTypeNames()
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

        public static int GetLabelIndex(byte[] mac)
        {
            // when there is no Label table set, return all type as Normal.
            if (_labelIndex == null)
                return 0;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                sb.Append(mac[i].ToString("X2"));
                sb.Append(":");
            }

            sb.Append(mac[5].ToString("X2"));

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
    
    public class ConversationFlowComparer : IEqualityComparer<Conversationflow2>
    {

        public bool Equals(Conversationflow2 x, Conversationflow2 y)
        {
            if (Object.ReferenceEquals(x, y))
                return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Connection.Equals(y.Connection);

        }

        public int GetHashCode(Conversationflow2 obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            int hashConnection = obj.Connection == null ? 0 : obj.Connection.GetHashCode();

            int hashProcessname = obj.ProcessName == null ? 0 : obj.ProcessName.GetHashCode();

            int hashStartTime = obj.StartTime == null ? 0 : obj.StartTime.GetHashCode();

            return hashConnection ^ hashProcessname ^ hashStartTime;
        }
    }
}
