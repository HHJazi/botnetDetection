using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.NetworkMonitor;
using System.Windows.Forms;

namespace Biotracker.Client.ProcessMonitor
{
    public delegate void NewSessionEventHandler(object sender, SessionEventArgs e);
    public delegate void EndSessionEventHandler(object sender, SessionEventArgs e);

    public class SessionEventArgs : EventArgs
    {
        public enum EventType
        {
            NewSession,
            EndSession
        };

        public EventType EvtType;

        public IPEndPoint DestEnd;

        public DateTime TimeStamp;

        public SessionEventArgs(EventType t, IPEndPoint endPoint)
            : base()
        {
            EvtType = t;
            DestEnd = endPoint;

            TimeStamp = DateTime.UtcNow;
        }

        public SessionEventArgs(EventType t, IPEndPoint endPoint, DateTime timestamp)
        {
            EvtType = t;
            DestEnd = endPoint;
            TimeStamp = timestamp;
        }
    }

    public class NMParser : NMBase, IDisposable
    {
        public enum TCPFlags
        {
            FIN = 0x01,
            SYN = 0x02,
            RST = 0x04,
            PSH = 0x08,
            ACK = 0x10,
            URG = 0x20,
            ECE = 0x40,
            CWR = 0x80
        };

        #region Private Members

        //private bool Error =false;

        private string _traceFile = null;
        private TimeSpan _timeWindow;
        private ProducerConsumer<FlowFeature> _producerConsumerRef = null;
        /// <summary>
        /// Current frame number in the capture trace file.
        /// </summary>
        public uint _curFrameNb = 0;
        public uint _totalFrameCount = 0;

        /// <summary>
        /// Callback for parser loading.
        /// </summary>
        private ParserCallbackDelegate ErrorCallBack = null;

        /// <summary>
        /// NPL Set NMAPI handle
        /// </summary>
        private IntPtr parserNPLSetHandle = IntPtr.Zero;

        /// <summary>
        /// Paser Configuration NMAPI handle
        /// </summary>
        private IntPtr parserConfigHandle = IntPtr.Zero;

        /// <summary>
        /// Parser NMAPI handle
        /// </summary>
        private IntPtr parserHandle = IntPtr.Zero;

        /// <summary>
        /// Ethernet Source Address ID for NmAddField
        /// </summary>
        private uint MACSrcID;

        /// <summary>
        /// IPv4 Destination Address ID for NmAddField
        /// </summary>
        private uint MACDestID;

        /// <summary>
        /// IPv4 Source Address ID for NmAddField
        /// </summary>
        private uint IPv4SrcID;

        /// <summary>
        /// IPv4 Destination Address ID for NmAddField
        /// </summary>
        private uint IPv4DestID;

        /// <summary>
        /// IPv6 Source Address ID for NmAddField
        /// </summary>
        private uint IPv6SrcID;

        /// <summary>
        /// IPv6 Destination Address ID for NmAddField
        /// </summary>
        private uint IPv6DestID;

        /// <summary>
        /// Transportation Layer Protocol ID (TCP/UDP or mixed) 
        /// </summary>
        private uint ProtocolID;

        /// <summary>
        /// TCP source port ID;
        /// </summary>
        private uint TCPSrcPortID;

        /// <summary>
        /// TCP destination port ID
        /// </summary>
        private uint TCPDestPortID;

        /// <summary>
        /// TCP flags
        /// </summary>
        private uint TCPFlagID;

        /// <summary>
        /// UDP source port ID;
        /// </summary>
        private uint UDPSrcPortID;

        /// <summary>
        /// TCP destination port ID
        /// </summary>
        private uint UDPDestPortID;

        /// <summary>
        /// UDP total length field ID;
        /// </summary>
        private uint UDPTotalLengthID;

        /// <summary>
        /// Payload length property ID
        /// </summary>
        private uint TCPPayloadLengthID;

        /// <summary>
        /// Length(payload+header) property ID
        /// </summary>
        private uint Length;

        #endregion

#if false
        #region Imported C++ functions

        /// <summary> 
        /// Enumeration of the states 
        /// </summary> 
        public enum State
        {
            /// <summary> All </summary> 
            All = 0,
            /// <summary> Closed </summary> 
            Closed = 1,
            /// <summary> Listen </summary> 
            Listen = 2,
            /// <summary> Syn_Sent </summary> 
            Syn_Sent = 3,
            /// <summary> Syn_Rcvd </summary> 
            Syn_Rcvd = 4,
            /// <summary> Established </summary> 
            Established = 5,
            /// <summary> Fin_Wait1 </summary> 
            Fin_Wait1 = 6,
            /// <summary> Fin_Wait2 </summary> 
            Fin_Wait2 = 7,
            /// <summary> Close_Wait </summary> 
            Close_Wait = 8,
            /// <summary> Closing </summary> 
            Closing = 9,
            /// <summary> Last_Ack </summary> 
            Last_Ack = 10,
            /// <summary> Time_Wait </summary> 
            Time_Wait = 11,
            /// <summary> Delete_TCB </summary> 
            Delete_TCB = 12
        }

        public enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        public enum UDP_TABLE_CLASS
        {
            UDP_TABLE_BASIC,
            UDP_TABLE_OWNER_PID,
            UDP_TABLE_OWNER_MODULE
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE
        {
            public uint dwEnumEntries;
            public MIB_TCPROW[] table;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW
        {
            public uint dwState;
            public uint dwLocalAddr;
            public uint dwLocalPort;
            public uint dwRemoteAddr;
            public uint dwRemotePort;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDPSTATS
        {
            public int dwInDatagrams;
            public int dwNoPorts;
            public int dwInErrors;
            public int dwOutDatagrams;
            public int dwNumAddrs;
        }

        public struct MIB_UDPTABLE
        {
            public int dwNumEntries;
            public MIB_UDPROW[] table;

        }

        public struct MIB_UDPROW
        {
            public IPEndPoint Local;
        }

        public struct MIB_EXUDPTABLE
        {
            public int dwNumEntries;
            public MIB_EXUDPROW[] table;

        }

        public struct MIB_EXUDPROW
        {
            public IPEndPoint Local;
            public int dwProcessId;
            public string ProcessName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPSTATS
        {
            public int dwRtoAlgorithm;
            public int dwRtoMin;
            public int dwRtoMax;
            public int dwMaxConn;
            public int dwActiveOpens;
            public int dwPassiveOpens;
            public int dwAttemptFails;
            public int dwEstabResets;
            public int dwCurrEstab;
            public int dwInSegs;
            public int dwOutSegs;
            public int dwRetransSegs;
            public int dwInErrs;
            public int dwOutRsts;
            public int dwNumConns;
        }

        //[DllImport("iphlpapi.dll", SetLastError = true)]
        //public extern static uint GetUdpStatistics(ref MIB_UDPSTATS pStats);

        //[DllImport("iphlpapi.dll", SetLastError = true)]
        //public static extern uint GetUdpTable(byte[] UcpTable, out int pdwSize, bool bOrder);

        //[DllImport("iphlpapi.dll", SetLastError = true)]
        //public extern static uint GetTcpStatistics(ref MIB_TCPSTATS pStats);

        //[DllImport("iphlpapi.dll", SetLastError = true)]
        //public static extern uint GetTcpTable(byte[] pTcpTable, out int pdwSize, bool bOrder);

        [DllImport("iphlpapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public extern static uint GetExtendedTcpTable(IntPtr pTable, ref int dwSize, bool bOrder, int ulAdressFamily, TCP_TABLE_CLASS dwFlag, int Reserved);

        [DllImport("iphlpapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public extern static uint GetExtendedUdpTable(IntPtr pTable, ref int dwSize, bool bOrder, int ulAdressFamily, UDP_TABLE_CLASS dwFlags, int Reserved);


        #endregion
#endif

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the NMParser class. Load the parser and filter.
        /// <remarks>The NMParser object can be either Capturing state or Parsing state, which
        /// depends on whether the parameter, capture file, exists. If the Capture trace file
        /// exists, it means to prepare capturing traffic and store frames in the trace file.
        /// Otherwise, it means to prepare for parsing the stored trace file.</remarks>
        /// </summary>
        public NMParser()
            : base()
        {
            try
            {
                //Prepare Parser configuration and parser.
                this.CreateParser();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Initializes a new instance of the NMParser class. Load the parser and filter.
        /// <remarks>The NMParser object can be either Capturing state or Parsing state, which
        /// depends on whether the parameter, capture file, exists. If the Capture trace file
        /// exists, it means to prepare capturing traffic and store frames in the trace file.
        /// Otherwise, it means to prepare for parsing the stored trace file.</remarks>
        /// </summary>
        public NMParser(string traceFile)
            : base(traceFile)
        {
            uint errno;

            if (!File.Exists(traceFile))
            {
                throw new FileNotFoundException("Trace file doesn't exist.");
            }

            try
            {
                //Prepare Parser configuration and parser.
                this.CreateParser();

                // Open a capture file 
                errno = NetmonAPI.NmOpenCaptureFile(traceFile, out this.captureFileHandle);
                if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("NmOpenCaptureFile() failed", errno));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Initializes a new instance of the NMParser class that will operate as 
        /// a producer of FlowFeatures to be consumed by the sonsumer thread.
        /// <remarks>The NMParser object can be either Capturing state or Parsing state, which
        /// depends on whether the parameter, capture file, exists. If the Capture trace file
        /// exists, it means to prepare capturing traffic and store frames in the trace file.
        /// Otherwise, it means to prepare for parsing the stored trace file.</remarks>
        /// </summary>
        public NMParser(string traceFile,TimeSpan timeWindow, ProducerConsumer<FlowFeature> queue)
            : base(traceFile)
        {
            uint errno;

            if (!File.Exists(traceFile))
            {
                throw new FileNotFoundException("Trace file doesn't exist.");
            }

            try
            {
                //Prepare Parser configuration and parser.
                this.CreateParser();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            this._traceFile = traceFile;
            this._timeWindow = timeWindow;
            this._producerConsumerRef = queue;
        }


        /// <summary>
        /// Initialize a NMParser object with a file handle. This handle must be an  
        /// trace file handle created by NMCapture.
        /// </summary>
        /// <param name="fileHandle">Trace file handle.</param>
        public NMParser(IntPtr fileHandle)
            : base()
        {
            try
            {
                //Prepare Parser configuration and parser.
                this.CreateParser();

                if (fileHandle == IntPtr.Zero)
                    throw new ArgumentNullException();

                this.captureFileHandle = fileHandle;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Public Methods

        public new void Dispose()
        {
            if (this.Disposed == false)
            {
                this.Disposed = true;

                if (Disposing)
                {
                    NetmonAPI.NmCloseHandle(this.parserNPLSetHandle);
                    NetmonAPI.NmCloseHandle(this.parserConfigHandle);
                    NetmonAPI.NmCloseHandle(this.parserHandle);
                }

                // Free the unmanaged resource ...
                this.parserNPLSetHandle = IntPtr.Zero;
                this.parserConfigHandle = IntPtr.Zero;
                this.parserHandle = IntPtr.Zero;

                base.Dispose();
            }
        }

        /// <summary>
        /// Get a list of packets from the capture trace file.
        /// </summary>
        /// <returns></returns>
        public List<Packet2> GetPackets()
        {
            if (this.parserNPLSetHandle == IntPtr.Zero || this.parserHandle == IntPtr.Zero)
                throw new Exception("NetMon is not ready.");

            if (this.captureFileHandle == IntPtr.Zero)
            {
                throw new Exception("Capture file is not opened.");
            }

            uint errno;
            IntPtr rawFrame = IntPtr.Zero;
            IntPtr parsedFrame = IntPtr.Zero; ;
            IntPtr insertRawFrame = IntPtr.Zero; ;
            uint frameCnt;

            List<Packet2> pktList = new List<Packet2>();

            errno = NetmonAPI.NmGetFrameCount(this.captureFileHandle, out frameCnt);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetFrameCount() failed", errno));
            }

            //Iterating the captured frames.
            for (uint i = 0; i < frameCnt; i++)
            {
                errno = NetmonAPI.NmGetFrame(this.captureFileHandle, i, out rawFrame);
                if (errno != 0)
                {
                    ErrorMsg += FormatErrMsg("Error getting frame #" + i, errno);
                    continue;
                }

                errno = NetmonAPI.NmParseFrame(
                    this.parserHandle,
                    rawFrame,
                    i,
                    NmFrameParsingOption.FieldDisplayStringRequired
                    | NmFrameParsingOption.UseFrameNumberParameter
                    | NmFrameParsingOption.FrameConversationInfoRequired,
                    out parsedFrame,
                    out insertRawFrame);
                if (errno != 0)
                {
                    ErrorMsg += FormatErrMsg("Error parsing frame #" + i, errno);
                    continue;
                }

                //TODO: add a filter here if it is applied.
                //bool passedFilter = false;
                //errno = NetmonAPI.NmEvaluateFilter(parsedFrame, ulFilterId, out passedFileter);

                ulong frameTimeStamp;
                errno = NetmonAPI.NmGetFrameTimeStamp(parsedFrame, out frameTimeStamp);
                if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("NmGetFrameTimeStamp() failed", errno));
                }

                //for debug:
                //Console.WriteLine(DisplayFieldString(parsedFrame));
                //continue;

                IPAddress destIP;
                IPAddress srcIP;
                //Try IPv4 first
                destIP = GetIPAddrFromFrame(parsedFrame, this.IPv4DestID);
                if (destIP == null)
                {
                    //this is a IPv6 type address
                    destIP = GetIPAddrFromFrame(parsedFrame, this.IPv6DestID);
                    srcIP = GetIPAddrFromFrame(parsedFrame, this.IPv6SrcID);
                }
                else
                {
                    srcIP = GetIPAddrFromFrame(parsedFrame, this.IPv4SrcID);
                }

                //Get the transportation protocol
                Packet2.Protocol protocol = GetTransportProtocol(parsedFrame, this.ProtocolID);

                uint payloadLength = 0;

                //Create a new packet and add it to the list.
                if (destIP != null && srcIP != null)
                {
                    // get port number
                    int srcPort = 0;
                    int destPort = 0;

                    if (protocol == Packet2.Protocol.TCP)
                    {
                        srcPort = GetPortFromFrame(parsedFrame, this.TCPSrcPortID);
                        destPort = GetPortFromFrame(parsedFrame, this.TCPDestPortID);

                        //Get TCP payload length
                        payloadLength = GetTcpPayloadLength(parsedFrame, this.TCPPayloadLengthID);
                    }
                    else if (protocol == Packet2.Protocol.UDP)
                    {
                        srcPort = GetPortFromFrame(parsedFrame, this.UDPSrcPortID);
                        destPort = GetPortFromFrame(parsedFrame, this.UDPDestPortID);

                        //Get UDP payload length
                        payloadLength = GetUdpLength(parsedFrame, this.UDPTotalLengthID);
                    }
                   // uint length = 0;
                    if (destPort > 0)
                    {
                        Packet2 pkt = new Packet2(
                                new IPEndPoint(srcIP, srcPort),
                                new IPEndPoint(destIP, destPort),
                                protocol,
                                payloadLength,
                                DateTime.FromFileTime((long)frameTimeStamp)
                                );

                        pktList.Add(pkt);
                    }
                }

                //Close frame handles
                NetmonAPI.NmCloseHandle(parsedFrame);
                NetmonAPI.NmCloseHandle(rawFrame);
                NetmonAPI.NmCloseHandle(insertRawFrame);
            }

            return pktList;
        }

     
        /// <summary>
        /// Get a list of network flows from the captured trace file. This method is uesd to 
        /// parse a capture trace file and get a list of network flows in a timewindow
        /// </summary>
        /// <param name="traceFile">pcap file or cap file.</param>
        /// <param name="timeWindow">Time window </param>
        /// <returns></returns>
        public IEnumerable<Flow2> GetNetworkFlows(string traceFile, TimeSpan timeWindow) /// for signiture 
        {
            uint errno = 0;
            int totalNumOfFlows = 0;
            int flowId = 0;
            IntPtr rawFrame = IntPtr.Zero;
            IntPtr parsedFrame = IntPtr.Zero;
            IntPtr insertRawFrame = IntPtr.Zero;
            DateTime frameTime = DateTime.MinValue;
            uint frameCnt=0;
            _timeWindow = timeWindow;

            if (this.parserNPLSetHandle == IntPtr.Zero || this.parserHandle == IntPtr.Zero)
                throw new Exception("NetMon is not ready.");

            if (string.IsNullOrEmpty(traceFile))
                throw new ArgumentNullException();

            if (!File.Exists(traceFile))
                throw new FileNotFoundException();


           // captureFileHandle = IntPtr.Zero;
            // Open a capture file 
            errno = NetmonAPI.NmOpenCaptureFile(traceFile, out this.captureFileHandle);
            if (errno != 0)
            {
                if (errno == 3775987721)
                    throw new Exception("Unsupported network trace file type, please use standard libpcap format");
                else
                    throw new Exception(FormatErrMsg("NmOpenCaptureFile() failed", errno));
            }

            errno = NetmonAPI.NmGetFrameCount(this.captureFileHandle, out frameCnt);
            if (errno != 0)
            {
                throw new ArgumentNullException(FormatErrMsg("NmGetFrameCount() failed", errno));
            }

            System.Diagnostics.Debug.WriteLine("Total Number of Frames in tracefile= " + frameCnt);

            Dictionary<ConnectionPair2, Flow2> flowList = new Dictionary<ConnectionPair2, Flow2>();

            DateTime startTime = DateTime.MinValue;

            ConnectionPair2 conn = null ;
            //Iterating the captured frames.
            for (uint i = 0; i < frameCnt; i++)
            {
                if (i % 10000 == 0)
                    System.Diagnostics.Debug.WriteLine("Processing Frame Number: " + i + " from: " + frameCnt + " @[" + DateTime.Now.ToString() + "]");

                errno = NetmonAPI.NmGetFrame(this.captureFileHandle, i, out rawFrame);
                if (errno != 0)
                {
                    ErrorMsg += FormatErrMsg("Error getting frame #" + i, errno);
                    goto EndOfParsing;
                }

                errno = NetmonAPI.NmParseFrame(
                    this.parserHandle,
                    rawFrame,
                    i,
                    NmFrameParsingOption.FieldFullNameRequired
                    | NmFrameParsingOption.ContainingProtocolNameRequired
                    | NmFrameParsingOption.DataTypeNameRequired
                    | NmFrameParsingOption.FieldDisplayStringRequired
                    | NmFrameParsingOption.FrameConversationInfoRequired,
                    out parsedFrame,
                    out insertRawFrame);
                if (errno != 0)
                {
                    ErrorMsg += FormatErrMsg("Error parsing frame #" + i, errno);
                    goto EndOfParsing;
                }

                //TODO: add a filter here if it is applied.
                //bool passedFilter = false;
                //errno = NetmonAPI.NmEvaluateFilter(parsedFrame, ulFilterId, out passedFileter);

                ulong frameTimeStamp;
                errno = NetmonAPI.NmGetFrameTimeStamp(parsedFrame, out frameTimeStamp);
                if (errno != 0)
                {
                    ErrorMsg += FormatErrMsg("NmGetFrameTimeStamp() failed", errno);
                    goto EndOfParsing;
                }

                frameTime = DateTime.FromFileTime((long)frameTimeStamp);

                if (startTime == DateTime.MinValue)
                {
                    startTime = frameTime;
                }
    
                //Get the MAC address of the frame
                byte[] srcMAC = GetMACAddree(parsedFrame, this.MACSrcID);
                byte[] destMAC = GetMACAddree(parsedFrame, this.MACDestID);

                //Get the transportation protocol
                Packet2.Protocol protocol = GetTransportProtocol(parsedFrame, this.ProtocolID);

                IPAddress destIP;
                IPAddress srcIP;

                //Try IPv4 first
                srcIP = GetIPAddrFromFrame(parsedFrame, this.IPv4SrcID);
                if (srcIP == null)
                {
                    //this is a IPv6 type address
                    destIP = GetIPAddrFromFrame(parsedFrame, this.IPv6DestID);
                    srcIP = GetIPAddrFromFrame(parsedFrame, this.IPv6SrcID);
                }
                else
                {
                    destIP = GetIPAddrFromFrame(parsedFrame, this.IPv4DestID);
                }

                uint payloadLength = 0;

                //Create a new packet and add it to the list.
                if (destIP != null && srcIP != null)
                {
                    // get port number
                    int srcPort = 0;
                    int destPort = 0;

                    if (protocol == Packet2.Protocol.TCP)
                    {
                        srcPort = GetPortFromFrame(parsedFrame, this.TCPSrcPortID);
                        destPort = GetPortFromFrame(parsedFrame, this.TCPDestPortID);

                        //Get TCP payload length
                        payloadLength = GetTcpPayloadLength(parsedFrame, this.TCPPayloadLengthID);



                        ////extract tcpflags
                        if (payloadLength == 0)
                        {
                            int flags = (int)GetTcpFlags(parsedFrame, this.TCPFlagID);

                            //if the flag is SYN, which is a good indication of TCP Session.
                            if ((flags | (int)TCPFlags.SYN) == (int)TCPFlags.SYN)
                            {
                                RaiseNewSessionEvent(new IPEndPoint(srcIP, srcPort));
                            }

                            //if the flag is FIN, raise the event of Session expired.
                            if ((flags | (int)TCPFlags.FIN) == (int)TCPFlags.FIN)
                            {
                                RaiseSessionEndEvent(new IPEndPoint(srcIP, srcPort));
                            }
                        }

                        ////

                    }
                    else if (protocol == Packet2.Protocol.UDP)
                    {
                        srcPort = GetPortFromFrame(parsedFrame, this.UDPSrcPortID);
                        destPort = GetPortFromFrame(parsedFrame, this.UDPDestPortID);

                        //Get UDP payload length
                        payloadLength = GetUdpLength(parsedFrame, this.UDPTotalLengthID);
                    }

                    if (destPort > 0)
                    {
                        Packet2 pkt = new Packet2(
                                srcMAC,
                                destMAC,
                                new IPEndPoint(srcIP, srcPort),
                                new IPEndPoint(destIP, destPort),
                                protocol,
                                payloadLength,
                                frameTime
                                );

                        conn = new ConnectionPair2(pkt);

                        //Sort the flows with local IP
                        //if (!NetworkMonitor.IsLocalAddress(srcIP))
                        //    conn = conn.BackwardDirection;
                        if (conn != null)
                        {
                            if (!flowList.ContainsKey(conn))
                            {
                                Flow2 f = new Flow2(flowId++, conn);

                                //f.Type = Flow.GetLabelIndex(srcMAC);
                                flowList.Add(conn, f);
                            }

                            int flags = (int)GetTcpFlags(parsedFrame, this.TCPFlagID);
                            if ((flags | (int)TCPFlags.SYN) == (int)TCPFlags.SYN) { flowList[conn].reconnect++; } // if there is a reconnection 
                            flowList[conn].AddPacket(pkt);

                        }

                        //ConnectionPair2 backwardConn = conn.BackwardDirection;
                        //if (flowList.ContainsKey(backwardConn))
                        //{   // there are conn for backward direction of that flow

                        //    flowList[backwardConn].a
                        //}
                        
                        
                       

                    }
                }

            EndOfParsing:
                //Close frame handles
                NetmonAPI.NmCloseHandle(parsedFrame);
                NetmonAPI.NmCloseHandle(rawFrame);
                NetmonAPI.NmCloseHandle(insertRawFrame);
                
                long timeDiff = (long)frameTime.Ticks - (long)startTime.Ticks;

                // srcIp or DestIp were null
                //if (conn==null)  
                //{
                //    flowList = new Dictionary<ConnectionPair2, Flow2>();
                //}
                 if (Math.Abs(timeDiff) >= _timeWindow.Ticks) //|| (flowList.ContainsKey(conn) && flowList[conn].NbOfPkt >= 10000))
                {
                    startTime = DateTime.MinValue;

                    foreach (Flow2 f in flowList.Values){
                        
                        ConnectionPair2 backwardConn = conn.BackwardDirection;
                        if (flowList.ContainsKey(backwardConn))
                        {   // there are conn for backward direction of that flow

                            f.addBackwardPacket(flowList[backwardConn].NbOfPkt);
                        }

                        yield return f;
                    }
                    totalNumOfFlows += flowList.Count;
                    //Reset the flow list
                    flowList = new Dictionary<ConnectionPair2, Flow2>();
                }
            }

            if (flowList.Count != 0)
            {
                foreach (Flow2 f in flowList.Values)
                {

                    ConnectionPair2 backwardConn = conn.BackwardDirection;
                    if (flowList.ContainsKey(backwardConn))
                    {   // there are conn for backward direction of that flow

                        f.addBackwardPacket(flowList[backwardConn].NbOfPkt);
                    }

                    yield return f;
                }
                totalNumOfFlows += flowList.Count;

            }
            flowList = new Dictionary<ConnectionPair2, Flow2>();
            NetmonAPI.NmCloseHandle(this.captureFileHandle);
            this.captureFileHandle = IntPtr.Zero;
            System.Diagnostics.Debug.WriteLine("Total Number of Flows in sig file = " + totalNumOfFlows.ToString());

        }

        /// <summary>
        /// Generate network flows features from the captured trace file. This method is uesd to 
        /// parse a trace file and enqueue network flow features to be consumed later
        /// by the consumer thread. The call of this method should be made using an instance 
        /// of this calls instanciated using the NMPareser(string, TimeSpan, ProducerConsumer)
        /// constructor.
        /// </summary>
        /// <returns></returns>
        public void EnqueueNetworkFlowFeatures()  // for offline detection
        {
            uint errno = 0;

            int flowId = 0;
            IntPtr rawFrame = IntPtr.Zero;
            IntPtr parsedFrame = IntPtr.Zero;
            IntPtr insertRawFrame = IntPtr.Zero;
            DateTime frameTime = DateTime.MinValue;
            uint frameCnt;
            string conn_str = "";
            
            if (this.parserNPLSetHandle == IntPtr.Zero || this.parserHandle == IntPtr.Zero)
            {
                MessageBox.Show("NetMon is not ready.");
                return;
                //throw new Exception("NetMon is not ready.");
            }

            if (string.IsNullOrEmpty(_traceFile))
            {
                MessageBox.Show("Trace File is Null or Empty");
                return;
                //throw new ArgumentNullException();
            }

            if (!File.Exists(_traceFile))
            {
                MessageBox.Show("Trace File doesn't exist");
                return;
                //throw new FileNotFoundException();
            }

            // Open a capture file 
            errno = NetmonAPI.NmOpenCaptureFile(_traceFile, out this.captureFileHandle);
            if (errno != 0)
            {
                if (errno == 3775987721)
                    MessageBox.Show("Unsupported network trace file type, please use standard libpcap format");
                else
                    MessageBox.Show(string.Format("NmOpenCaptureFile() failed with Error: 0x{0:X} ", errno));
                return;
                //throw new Exception(FormatErrMsg("NmOpenCaptureFile() failed", errno));
            }

            errno = NetmonAPI.NmGetFrameCount(this.captureFileHandle, out frameCnt);
            if (errno != 0)
            {
                MessageBox.Show(string.Format("NmGetFrameCount() failed with Error: 0x{0:X}" , errno));
                //throw new ArgumentNullException(FormatErrMsg("NmGetFrameCount() failed", errno));
            }

            this._totalFrameCount = frameCnt;

            System.Diagnostics.Debug.WriteLine("Total Number of Frames in tracefile= " + frameCnt);

            Dictionary<ConnectionPair2, Flow2> flowList = new Dictionary<ConnectionPair2, Flow2>();

            DateTime startTime = DateTime.MinValue;

            long totalNumOfFlows = 0;


            // using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Elaheh_B\Desktop\offlineAnalysisfeature.csv"))
            //{
            //    RuntimeConfig config = new RuntimeConfig();
            //    String featuresNameList = config.featuresList();
            //    file.WriteLine(featuresNameList);



            //Iterating the captured frames.
            for (uint i = 0; i < frameCnt; i++)
            {

                ConnectionPair2 conn=null;
                this._curFrameNb = i;
                if (i % 10000 == 0)
                    System.Diagnostics.Debug.WriteLine("Processing Frame Number: " + i + " from: " + frameCnt + " @["+ DateTime.Now.ToString()+"]");

                errno = NetmonAPI.NmGetFrame(this.captureFileHandle, i, out rawFrame);
                if (errno != 0)
                {
                    ErrorMsg += FormatErrMsg("Error getting frame #" + i, errno);
                    goto EndOfParsing;
                }

                errno = NetmonAPI.NmParseFrame(
                    this.parserHandle,
                    rawFrame,
                    i,
                    NmFrameParsingOption.FieldFullNameRequired
                    | NmFrameParsingOption.ContainingProtocolNameRequired
                    | NmFrameParsingOption.DataTypeNameRequired
                    | NmFrameParsingOption.FieldDisplayStringRequired
                    | NmFrameParsingOption.FrameConversationInfoRequired,
                    out parsedFrame,
                    out insertRawFrame);
                if (errno != 0)
                {
                    ErrorMsg += FormatErrMsg("Error parsing frame #" + i, errno);
                    goto EndOfParsing;
                }

                //TODO: add a filter here if it is applied.
                //bool passedFilter = false;
                //errno = NetmonAPI.NmEvaluateFilter(parsedFrame, ulFilterId, out passedFileter);

                ulong frameTimeStamp;
                errno = NetmonAPI.NmGetFrameTimeStamp(parsedFrame, out frameTimeStamp);
                if (errno != 0)
                {
                    ErrorMsg += FormatErrMsg("NmGetFrameTimeStamp() failed", errno);
                    goto EndOfParsing;
                }

                frameTime = DateTime.FromFileTime((long)frameTimeStamp);

                if (startTime == DateTime.MinValue)
                {
                    startTime = frameTime;
                }

                //Get the MAC address of the frame
                byte[] srcMAC = GetMACAddree(parsedFrame, this.MACSrcID);
                byte[] destMAC = GetMACAddree(parsedFrame, this.MACDestID);

                //Get the transportation protocol
                Packet2.Protocol protocol = GetTransportProtocol(parsedFrame, this.ProtocolID);

                IPAddress destIP = null;
                IPAddress srcIP = null;

                //Try IPv4 first
                srcIP = GetIPAddrFromFrame(parsedFrame, this.IPv4SrcID);
                if (srcIP == null)
                {
                    //this is a IPv6 type address
                    destIP = GetIPAddrFromFrame(parsedFrame, this.IPv6DestID);
                    srcIP = GetIPAddrFromFrame(parsedFrame, this.IPv6SrcID);
                }
                else
                {
                    destIP = GetIPAddrFromFrame(parsedFrame, this.IPv4DestID);
                }

                uint payloadLength = 0;

                //Create a new packet and add it to the list.
                if (destIP != null && srcIP != null)
                {
                    // get port number
                    int srcPort = 0;
                    int destPort = 0;

                    if (protocol == Packet2.Protocol.TCP)
                    {
                        srcPort = GetPortFromFrame(parsedFrame, this.TCPSrcPortID);
                        destPort = GetPortFromFrame(parsedFrame, this.TCPDestPortID);

                        //Get TCP payload length
                        payloadLength = GetTcpPayloadLength(parsedFrame, this.TCPPayloadLengthID);
                    }
                    else if (protocol == Packet2.Protocol.UDP)
                    {
                        srcPort = GetPortFromFrame(parsedFrame, this.UDPSrcPortID);
                        destPort = GetPortFromFrame(parsedFrame, this.UDPDestPortID);

                        //Get UDP payload length
                        payloadLength = GetUdpLength(parsedFrame, this.UDPTotalLengthID);
                    }

                    if (destPort > 0)
                    {
                        IPEndPoint srcEndPoint = new IPEndPoint(srcIP, srcPort);
                        IPEndPoint destEndPoint = new IPEndPoint(destIP, destPort);
                       
                        Packet2 pkt = new Packet2(
                                srcMAC,
                                destMAC,
                                srcEndPoint,
                                destEndPoint,
                                protocol,
                                payloadLength,
                                frameTime
                                );

                         conn = new ConnectionPair2(pkt);
                        //conn_str = srcEndPoint.ToString() + "_" + destEndPoint.ToString() + "_" + protocol.ToString();

                        /*
                        //Sort the flows with local IP
                        if (!NetworkMonitor.IsLocalAddress(srcIP))
                            conn = conn.BackwardDirection;
                        */

                         if (!flowList.ContainsKey(conn))
                        {
                            Flow2 f = new Flow2(flowId++, conn);
                            flowList.Add(conn, f);
                            f = null;
                        }

                        int flags = (int)GetTcpFlags(parsedFrame, this.TCPFlagID);
                        if ((flags | (int)TCPFlags.SYN) == (int)TCPFlags.SYN) { flowList[conn].reconnect++; } // if there is a reconnection 

                        flowList[conn].AddPacket(pkt);
                        
                        //releasing memory
                        conn            = null;
                        //conn_str        = null;
                        pkt             = null;
                        srcEndPoint     = null;
                        destEndPoint    = null;
                        srcIP           = null;
                        destIP          = null;
                        srcMAC          = null;
                        destMAC         = null;
                    }
                }

            EndOfParsing:
                //Close frame handles
                NetmonAPI.NmCloseHandle(parsedFrame);
                NetmonAPI.NmCloseHandle(rawFrame);
                NetmonAPI.NmCloseHandle(insertRawFrame);

                long timeDiff = (long)frameTime.Ticks - (long)startTime.Ticks;


                if (Math.Abs(timeDiff) >= _timeWindow.Ticks) // || (flowList.ContainsKey(conn_str) && flowList[conn_str].NbOfPkt >= 10000))
                {
                        startTime = DateTime.MinValue;

                        //System.Diagnostics.Debug.WriteLine("Time Diff: " + timeDiff);

                        foreach (Flow2 f in flowList.Values)
                        {

                            ConnectionPair2 backwardConn = f.Connection.BackwardDirection;
                            
                            if (flowList.ContainsKey(backwardConn))
                            {   // there are conn for backward direction of that flow

                                f.addBackwardPacket(flowList[backwardConn].NbOfPkt);
                            }

                            FlowFeature ff = f.GenerateFeatuesInTimeWindow();
                            if (ff != null)
                            {
                                ff.LoggerIp = "Offline";
                            //    file.WriteLine(ff.featuresToString());
                                _producerConsumerRef.Enqueue(ff);
                            }
                            ff = null;
                            //f.Dispose();
                        }

                        foreach (Flow2 f in flowList.Values)
                        {
                            f.Dispose();
                        }


                        totalNumOfFlows += flowList.Count;
                        //Reset the flow list
                        flowList.Clear();
                        flowList = null;
                        flowList = new Dictionary<ConnectionPair2, Flow2>();
                        //System.Diagnostics.Debug.WriteLine("Disposing FlowList Dictionary...");
                        GC.Collect();
                }
                
                //System.Diagnostics.Debug.WriteLine("FlowList size = " + flowList.Count.ToString());
            }// end for loop
            
            //corner situation when all the frames in the pcap file fit into one flowlist 
            //and the time interval did not expire.

            //var rows = new List<String>();


            if (flowList.Count > 0)
            {
                foreach (Flow2 f in flowList.Values)
                {

                    ConnectionPair2 backwardConn = f.Connection.BackwardDirection;

                    if (flowList.ContainsKey(backwardConn))
                    {   // there are conn for backward direction of that flow

                        f.addBackwardPacket(flowList[backwardConn].NbOfPkt);
                    }

                    FlowFeature ff = f.GenerateFeatuesInTimeWindow();
                    if (ff != null)
                    {
                        ff.LoggerIp = "Offline";
                     //   rows.Add(ff.featuresToString());

                      //  file.WriteLine(ff.featuresToString());

                        _producerConsumerRef.Enqueue(ff);
                    }
                    ff = null;
                   // f.Dispose(); // since we need this flow for calculating backward direction 
                }


                foreach (Flow2 f in flowList.Values) {
                    f.Dispose();
                }

                totalNumOfFlows += flowList.Count;
                flowList.Clear();
            }


          ////  System.Diagnostics.Debug.WriteLine("inside get flow feature from trace file: ");
          //  using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\ebiglarb\Desktop\offlineAnalysisfeature.csv"))
          //  {
          //      RuntimeConfig config = new RuntimeConfig();
          //      String featuresNameList = config.featuresList();
          //      file.WriteLine(featuresNameList);
                //foreach (string line in rows)
                //{

                //    file.WriteLine(line);

                //}
       //     }

            System.Diagnostics.Debug.WriteLine("Total Number of Flows= " + totalNumOfFlows.ToString());

            NetmonAPI.NmCloseHandle(this.captureFileHandle);
            this.captureFileHandle = IntPtr.Zero;

        }


        private byte[] GetMACAddree(IntPtr frame, uint fieldId)
        {
            uint errno;
            byte[] MAC = null;

            if (frame == IntPtr.Zero)
                throw new ArgumentNullException();

            NmParsedFieldInfo dataFieldInfo = new NmParsedFieldInfo();
            dataFieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

            errno = NetmonAPI.NmGetParsedFieldInfo(frame, fieldId, (uint)0, ref dataFieldInfo);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
            }

            uint bufLength = dataFieldInfo.ValueBufferLength;
            if (bufLength > 0)
            {
                uint actualLength = 0;

                unsafe
                {
                    byte* buf = (byte*)Marshal.AllocCoTaskMem((int)bufLength);

                    errno = NetmonAPI.NmGetFieldInBuffer(frame, fieldId, bufLength, buf, out actualLength);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("NmGetFieldInBuffer() failed", errno));
                    }

                    if (actualLength > 0)
                    {
                        MAC = new byte[6];
                        for (int i = 0; i < 6; i++)
                            MAC[i] = buf[i];
                    }
                    else
                    {
                        throw new Exception("Invalid MAC address field.");
                    }

                    Marshal.FreeCoTaskMem((IntPtr)buf);
                }
            }
            else
                return MAC;
                //throw new Exception("Cannot get MAC address from the frame.");

            return MAC;
        }
        #endregion

        #region Events

        public event NewSessionEventHandler NewSessionEvent;

        public event EndSessionEventHandler EndSessionEvent;

        #endregion

        #region Private Methods

        /// <summary>
        /// Parser callback functin for loading NPL.
        /// </summary>
        /// <param name="callerContext">Context passed in by caller</param>
        /// <param name="statusCode">Status code if current message</param>
        /// <param name="description"></param>
        /// <param name="type">type of message, error, warning, information</param>
        private void ParserCallback(
            IntPtr callerContext,
            uint statusCode,
            [MarshalAs(UnmanagedType.LPWStr)]string description,
            NmCallbackMsgType type)
        {
            ErrorMsg += description + "\n";
        }

        /// <summary>
        /// Attempt to load the frame parser config, add filters and fields, and then create frame parser.
        /// </summary>
        /// <returns>Returns true if loading was succesful.</returns>
        private bool CreateParser()
        {
            uint errno;

            ErrorCallBack = new ParserCallbackDelegate(ParserCallback);

            //Use null to load the default NPL set.
            errno = NetmonAPI.NmLoadNplParser(
                null,
                NmNplParserLoadingOption.NmAppendRegisteredNplSets,
                //NmNplParserLoadingOption.NmLoadNplOptionNone,
                ErrorCallBack,
                IntPtr.Zero,
                out this.parserNPLSetHandle);

            // Error 57 is the normal error returned for parser errors.  We will
            // handle this gracefully by displaying the errors.     
            if (errno != 0 && errno != 0x57)
            {
                throw new Exception(FormatErrMsg("NmLoadNplParser() failed", errno));
            }

            //if (errno == 0x57)
            //{
            //    Error = true;
            //}

            errno = NetmonAPI.NmCreateFrameParserConfiguration(
                this.parserNPLSetHandle,
                ErrorCallBack,
                IntPtr.Zero,
                out this.parserConfigHandle);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmCreateFrameParserConfiguration() failed", errno));
            }

            errno = NetmonAPI.NmConfigConversation(
                this.parserConfigHandle,
                NmConversationConfigOption.None,
                true);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmConfigConversation() failed", errno));
            }

            try
            {
                this.AddDataFields();

                this.AddProperty();

                errno = NetmonAPI.NmCreateFrameParser(
                    this.parserConfigHandle,
                    out this.parserHandle,
                    NmFrameParserOptimizeOption.ParserOptimizeNone
                    );
                if (errno != 0)
                {
                    throw new Exception("NmCreateFrameParser() failed. Error=" + errno);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        /// <summary>
        /// Add Data Fields in the parser.
        /// </summary>
        private void AddDataFields()
        {
            uint errno;

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "Ethernet.DestinationAddress", out this.MACDestID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "Ethernet.SourceAddress", out this.MACSrcID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }
            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "ipv4.SourceAddress", out this.IPv4SrcID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "ipv4.DestinationAddress", out this.IPv4DestID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "ipv6.SourceAddress", out this.IPv6SrcID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "ipv6.DestinationAddress", out this.IPv6DestID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "ipv4.NextProtocol", out this.ProtocolID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "TCP.SrcPort", out this.TCPSrcPortID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "TCP.DstPort", out this.TCPDestPortID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "TCP.Flags", out this.TCPFlagID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "UDP.SrcPort", out this.UDPSrcPortID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "UDP.DstPort", out this.UDPDestPortID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }

            errno = NetmonAPI.NmAddField(this.parserConfigHandle, "UDP.TotalLength", out this.UDPTotalLengthID);
            if (errno != 0)
            {
                throw new Exception("NmAddField(...) failed, errnor = " + errno);
            }
        }

        /// <summary>
        ///  Add Property index in the parser. 
        /// </summary>
        private void AddProperty()
        {
            uint errno;

            errno = NetmonAPI.NmAddProperty(this.parserConfigHandle, "Property.TCPPayloadLength", out this.TCPPayloadLengthID);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmAddProperty() failed", errno));
            }
        }

        /// <summary>
        /// This method reads the MAC address of the frame. If the source MAC and destination MAC are 'AA-AA-AA...' or 'BB-BB-BB',
        /// this flow is a Malicious flow, otherwise it is Normalicious flow.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        private bool IsMaliciousFlow(IntPtr frame)
        {
            //return GetFlowType(frame) != Flow.FlowType.Normal;
            return GetFlowTypeIndex(frame) != 0;
        }
#if false
        private Flow.FlowType GetFlowType(IntPtr frame)
        {
            uint errno;
            Flow.FlowType fType = Flow.FlowType.Normal;

            if (frame == IntPtr.Zero)
                throw new ArgumentNullException();

            NmParsedFieldInfo dataFieldInfo = new NmParsedFieldInfo();
            dataFieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

            errno = NetmonAPI.NmGetParsedFieldInfo(frame, this.MACSrcID, (uint)0, ref dataFieldInfo);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
            }

            uint bufLength = dataFieldInfo.ValueBufferLength;
            if (bufLength > 0)
            {
                uint actualLength = 0;

                unsafe
                {
                    byte* buf = (byte*)Marshal.AllocCoTaskMem((int)bufLength);

                    errno = NetmonAPI.NmGetFieldInBuffer(frame, this.MACSrcID, bufLength, buf, out actualLength);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("NmGetFieldInBuffer() failed", errno));
                    }

                    if (actualLength > 0)
                    {
                        byte[] MAC = new byte[6];
                        for (int i = 0; i < 6; i++)
                            MAC[i] = buf[i];

                        fType = GetFlowType(MAC);
                    }
                    else
                    {
                        throw new Exception("Invalid MAC address field.");
                    }

                    Marshal.FreeCoTaskMem((IntPtr)buf);
                }
            }
            else
                throw new Exception("Cannot get MAC address from the frame.");

            return fType;
        }
#endif
        private int GetFlowTypeIndex(IntPtr frame)
        {
            uint errno;
            int typeIndex = 0;

            if (frame == IntPtr.Zero)
                throw new ArgumentNullException();

            NmParsedFieldInfo dataFieldInfo = new NmParsedFieldInfo();
            dataFieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

            errno = NetmonAPI.NmGetParsedFieldInfo(frame, this.MACSrcID, (uint)0, ref dataFieldInfo);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
            }

            uint bufLength = dataFieldInfo.ValueBufferLength;
            if (bufLength > 0)
            {
                uint actualLength = 0;

                unsafe
                {
                    byte* buf = (byte*)Marshal.AllocCoTaskMem((int)bufLength);

                    errno = NetmonAPI.NmGetFieldInBuffer(frame, this.MACSrcID, bufLength, buf, out actualLength);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("NmGetFieldInBuffer() failed", errno));
                    }

                    if (actualLength > 0)
                    {
                        byte[] MAC = new byte[6];
                        for (int i = 0; i < 6; i++)
                            MAC[i] = buf[i];

                        typeIndex = Flow2.GetLabelIndex(MAC);
                        
                        MAC = null;
                    }
                    else
                    {
                        throw new Exception("Invalid MAC address field.");
                    }

                    Marshal.FreeCoTaskMem((IntPtr)buf);
                }
            }
            else
                throw new Exception("Cannot get MAC address from the frame.");
            
            return typeIndex;
        }

#if false
        internal Flow.FlowType GetFlowType(byte[] MACAddr)
        {
            if (MACAddr.Length != 6)
            {
                throw new ArgumentException("Invalid MAC Address");
            }

            int i;
            Flow.FlowType fType;

            switch (MACAddr[0])
            {
                case 0xAA:
                    fType = Flow.FlowType.SMTPSpam;
                    break;
                case 0xBB:
                    fType = Flow.FlowType.UDPStorm;
                    break;
                case 0xCC:
                    fType = Flow.FlowType.Zeus;
                    break;
                default:
                    return Flow.FlowType.Normal;
            }

            for (i = 1; i < 6; i++)
            {
                switch (fType)
                {
                    case Flow.FlowType.SMTPSpam:
                        if (MACAddr[i] != 0XAA)
                            return Flow.FlowType.Normal;
                        break;

                    case Flow.FlowType.UDPStorm:
                        if (MACAddr[i] != 0xBB)
                            return Flow.FlowType.Normal;
                        break;
                    case Flow.FlowType.Zeus:
                        if (MACAddr[i] != 0xCC)
                        {
                            if (MACAddr[i] == 0xDD && i > 2)
                                fType = Flow.FlowType.ZeusControl;
                            else
                                return Flow.FlowType.Normal;
                        }
                        break;

                    case Flow.FlowType.ZeusControl:
                        if (MACAddr[i] != 0xDD)
                            return Flow.FlowType.ZeusControl;
                        break;

                    default:
                        return Flow.FlowType.Normal;
                }
            }

            return fType;
        }
#endif

        /// <summary>
        /// Returns the IP address from the frame. Use ID to differentiate the Source and Destination address.
        /// </summary>
        /// <param name="frameHandle"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        private void GetIPAddrFromFrame(IntPtr frameHandle, uint fieldId, ref IPAddress ipaddr)
        {
            uint errno;

            ipaddr = null;

            if (frameHandle == IntPtr.Zero)
                throw new ArgumentNullException();

            NmParsedFieldInfo dataFieldInfo = new NmParsedFieldInfo();
            dataFieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

            errno = NetmonAPI.NmGetParsedFieldInfo(frameHandle, fieldId, (uint)0, ref dataFieldInfo);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
            }

            byte[] buffer = null;
            uint bufLength = dataFieldInfo.ValueBufferLength;
            if (bufLength > 0)
            {
                uint actualLength = 0;
                unsafe
                {
                    //byte* buf = (byte*)Marshal.AllocCoTaskMem((int)bufLength);

                    //allocating memory from the stack, so when the function is done 
                    // the memory is released automatically.
                    byte* buf = stackalloc byte[(int)bufLength];

                    errno = NetmonAPI.NmGetFieldInBuffer(frameHandle, fieldId, bufLength, buf, out actualLength);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("NmGetFieldInBuffer() failed", errno));
                    }

                    //if (actualLength < bufLength)
                    //{ 
                    //check the reasons.
                    //}

                    buffer = new byte[actualLength];
                    Marshal.Copy((IntPtr)buf, buffer, 0, buffer.Length);

                    //Marshal.FreeCoTaskMem((IntPtr)buf);
                }
            }

            if (buffer != null)
            {
                ipaddr = new IPAddress(buffer);
            }
            //release the buffer
            buffer = null;

            //return ipaddr;
        }



        /// <summary>
        /// Returns the IP address from the frame. Use ID to differentiate the Source and Destination address.
        /// </summary>
        /// <param name="frameHandle"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        private IPAddress GetIPAddrFromFrame(IntPtr frameHandle, uint fieldId)
        {
            uint errno;

            IPAddress ipaddr = null;

            if (frameHandle == IntPtr.Zero)
                throw new ArgumentNullException();

            NmParsedFieldInfo dataFieldInfo = new NmParsedFieldInfo();
            dataFieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

            errno = NetmonAPI.NmGetParsedFieldInfo(frameHandle, fieldId, (uint)0, ref dataFieldInfo);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
            }

            byte[] buffer = null;
            uint bufLength = dataFieldInfo.ValueBufferLength;
            if (bufLength > 0)
            {
                uint actualLength = 0;
                unsafe
                {
                    //byte* buf = (byte*)Marshal.AllocCoTaskMem((int)bufLength);

                    //allocating memory from the stack, so when the function is done 
                    // the memory is released automatically.
                    byte* buf = stackalloc byte[(int)bufLength];

                    errno = NetmonAPI.NmGetFieldInBuffer(frameHandle, fieldId, bufLength, buf, out actualLength);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("NmGetFieldInBuffer() failed", errno));
                    }

                    //if (actualLength < bufLength)
                    //{ 
                    //check the reasons.
                    //}

                    buffer = new byte[actualLength];
                    Marshal.Copy((IntPtr)buf, buffer, 0, buffer.Length);

                    //Marshal.FreeCoTaskMem((IntPtr)buf);
                }
            }

            if (buffer != null)
            {
                ipaddr = new IPAddress(buffer);
            }
            //release the buffer
            buffer = null;

            return ipaddr;
        }

        /// <summary>
        /// Parse the frame and return the TCP port number 
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="fieldId"></param>
        /// <returns>Port number </returns>
        private int GetPortFromFrame(IntPtr frame, uint fieldID)
        {
            uint errno;
            ushort port = 0;

            NmParsedFieldInfo fieldInfo = new NmParsedFieldInfo();
            fieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

            errno = NetmonAPI.NmGetParsedFieldInfo(frame, fieldID, (uint)0, ref fieldInfo);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
            }

            if (fieldInfo.ValueType == FieldType.VT_UI2)
            {
                errno = NetmonAPI.NmGetFieldValueNumber16Bit(frame, fieldID, out port);
                if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("NmGetFieldValueNumber16Bit", errno));
                }
            }

            return port;
        }

        /// <summary>
        /// Returns the TCP payload length value from the parsed frame.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="propertyID"></param>
        /// <returns></returns>
        private uint GetTcpPayloadLength(IntPtr frame, uint propertyID)
        {
            uint errno;
            NmPropertyValueType type;
            uint payloadLength = 0;

            unsafe
            {
                uint retlen;
                errno = NetmonAPI.NmGetPropertyById(
                    this.parserHandle,
                    this.TCPPayloadLengthID,
                    sizeof(uint),
                    (byte*)(&payloadLength),
                    out retlen,
                    out type,
                    0,
                    null
                    );
                if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("NmGetPropertyById() failed", errno));
                }
                else if (type == NmPropertyValueType.PropertyValueNone)
                {
                    retlen = 0;
                    payloadLength = 0;
                }

                return payloadLength;
            }
        }

        /// <summary>
        /// Get UDP total length field from the parsed frame.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="fieldID"></param>
        /// <returns></returns>
        private uint GetUdpLength(IntPtr frame, uint fieldID)
        {
            uint errno;
            ushort len = 0;

            NmParsedFieldInfo fieldInfo = new NmParsedFieldInfo();
            fieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

            errno = NetmonAPI.NmGetParsedFieldInfo(frame, fieldID, (uint)0, ref fieldInfo);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
            }

            if (fieldInfo.ValueType == FieldType.VT_UI2)
            {
                errno = NetmonAPI.NmGetFieldValueNumber16Bit(frame, fieldID, out len);
                if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("NmGetFieldValueNumber16Bit", errno));
                }
            }

            return len;
        }

        /// <summary>
        /// Returns the transport layer protocol of the IP frame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="fieldID"></param>
        /// <returns>Protocol.Mixed - if the protocol is neither TCP nor UDP. </returns>
        private Packet2.Protocol GetTransportProtocol(IntPtr frame, uint fieldID)
        {
            uint errno;
            byte prot = 0;

            NmParsedFieldInfo fieldInfo = new NmParsedFieldInfo();
            fieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

            errno = NetmonAPI.NmGetParsedFieldInfo(frame, fieldID, (uint)0, ref fieldInfo);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
            }

            if (fieldInfo.ValueType == FieldType.VT_UI1)
            {
                errno = NetmonAPI.NmGetFieldValueNumber8Bit(frame, fieldID, out prot);
                if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("NmGetFieldValueNumber8Bit", errno));
                }
            }

            switch (prot)
            {
                case 0x06:
                    return Packet2.Protocol.TCP;
                case 0x11:
                    return Packet2.Protocol.UDP;
                default:
                    return Packet2.Protocol.Mixed;
            }
        }

        /// <summary>
        /// Get the TCP flags
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="fieldID"></param>
        /// <returns></returns>
        private byte GetTcpFlags(IntPtr frame, uint fieldID)
        {
            uint errno;
            byte flags = 0;

            NmParsedFieldInfo fieldInfo = new NmParsedFieldInfo();
            fieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

            errno = NetmonAPI.NmGetParsedFieldInfo(frame, fieldID, (uint)0, ref fieldInfo);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
            }

            if (fieldInfo.ValueType == FieldType.VT_UI1)
            {
                errno = NetmonAPI.NmGetFieldValueNumber8Bit(frame, fieldID, out flags);
                if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("NmGetFieldValueNumber8Bit", errno));
                }
            }

            return flags;
        }

        /// <summary>
        /// The method returns a list of index of the Ethernet adapters. 
        /// </summary>
        /// <param name="adapters"></param>
        /// <returns></returns>
        private List<int> FindEthernetApdapterIndex(IEnumerable<NM_NIC_ADAPTER_INFO> adapters)
        {
            List<int> indexList = new List<int>();

            if (adapters != null)
            {
                for (int i = 0; i < adapters.Count(); i++)
                {
                    if (adapters.ElementAt(i).MediaType == NDIS_MEDIUM.Ndis_802_3 ||
                        adapters.ElementAt(i).MediaType == NDIS_MEDIUM.Ndis_Native802_11)
                    {
                        indexList.Add(i);
                    }
                }
            }

            return indexList;
        }


        private void RaiseSessionEndEvent(IPEndPoint dstEnd)
        {
            if (EndSessionEvent != null)
            {
                EndSessionEvent(this, new SessionEventArgs(SessionEventArgs.EventType.EndSession, dstEnd));
            }
        }

        private void RaiseNewSessionEvent(IPEndPoint dstEnd)
        {
            if (NewSessionEvent != null)
            {
                NewSessionEvent(this, new SessionEventArgs(SessionEventArgs.EventType.NewSession, dstEnd));
            }
        }

        internal UInt16 ConvertPort(UInt32 dwPort)
        {
            byte[] b = new Byte[2];
            // high weight byte
            b[0] = byte.Parse((dwPort >> 8).ToString());
            // low weight byte
            b[1] = byte.Parse((dwPort & 0xFF).ToString());
            return BitConverter.ToUInt16(b, 0);
        }

        private bool IsLocalIP(IPAddress ip)
        {
            if (NetworkMonitor.LocalIP != null)
                return NetworkMonitor.LocalIP.Contains(ip);
            else
                return false;
        }

        #endregion

        #region Get Process Name related functions

        /// <summary>
        /// Get a Dictionary of TCP connection pair and Owner PID from the extended TCP table.
        /// </summary>
        /// <param name="tcls">TCP_TABLE_CLASS enumeration.</param>
        /// <returns></returns>
        private SortedList<ConnectionPair2, int> GetTCPProcessTable(TCP_TABLE_CLASS tcls)
        {
            SortedList<ConnectionPair2, int> retList = new SortedList<ConnectionPair2, int>();
            uint errno;
            IntPtr lpTable = IntPtr.Zero;
            try
            {
                // the size of the MIB_EXTCPROW struct =  6*DWORD
                int bufferSize = 6 + 4; // 1 record 

                // allocate a dumb memory space in order to retrieve  nb of connexion
                lpTable = Marshal.AllocHGlobal((int)bufferSize);

                //getting infos
                errno = GetExtendedTcpTable(
                    lpTable,
                    ref bufferSize,
                    true,
                    (int)System.Net.Sockets.AddressFamily.InterNetwork, //AF_INET: 2, AF_INET6: 23
                    tcls,
                    0);
                if (errno == 0x7A) //Insufficient_buffer
                {
                    // free allocated space in memory
                    Marshal.FreeHGlobal(lpTable);
                    lpTable = IntPtr.Zero;

                    //the bufferSize is the correct size of the returned structure
                    lpTable = Marshal.AllocHGlobal((int)bufferSize);
                    errno = GetExtendedTcpTable(
                                lpTable,
                                ref bufferSize,
                                true,
                                (int)System.Net.Sockets.AddressFamily.InterNetwork, //AF_INET: 2, AF_INET6: 23
                                tcls,
                                0);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("GetExtendedTcpTable", (uint)errno));
                    }
                }
                else if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("GetExtendedTcpTable", (uint)errno));
                }

                int currentIndex = 0;

                //get the number of entries in the table
                int numEntries = (int)Marshal.ReadInt32(lpTable, currentIndex);

                // for each entries
                for (int i = 0; i < numEntries; i++)
                {
                    // iterate the pointer of 4 (the size of the DWORD dwNumEntries)
                    currentIndex += 4;
                    // The state of the connexion (in string)
                    uint state = (uint)Marshal.ReadInt32(lpTable, currentIndex);

                    // iterate the pointer of 4
                    currentIndex += 4;

                    // get the local IPv4 address 
                    uint localAddr = (uint)Marshal.ReadInt32(lpTable, currentIndex);

                    // iterate the pointer of 4
                    currentIndex += 4;

                    // get the local port of the connexion
                    int localPort = ConvertPort((uint)Marshal.ReadInt32(lpTable, currentIndex));

                    // Store the local endpoint in the struct and convertthe port in decimal (ie convert_Port())
                    IPEndPoint localEndpoint = new IPEndPoint(new IPAddress(localAddr), localPort);

                    // iterate the pointer of 4
                    currentIndex += 4;

                    // get the remote address of the connexion
                    uint RemoteAddr = (uint)Marshal.ReadInt32(lpTable, currentIndex);

                    // iterate the pointer of 4
                    currentIndex += 4;
                    int RemotePort = 0;
                    // if the remote address = 0 (0.0.0.0) the remote port is always 0
                    // else get the remote port
                    if (RemoteAddr != 0)
                    {
                        RemotePort = ConvertPort((uint)Marshal.ReadInt32(lpTable, currentIndex));
                    }
                    // store the remote endpoint in the struct  and convertthe port in decimal (ie convert_Port())
                    IPEndPoint remoteEndpoint = new IPEndPoint(RemoteAddr, RemotePort);

                    currentIndex += 4;
                    // store the process ID
                    uint procId = (uint)Marshal.ReadInt32(lpTable, currentIndex);

                    if (IPAddress.IsLoopback(localEndpoint.Address)
                        || IPAddress.IsLoopback(remoteEndpoint.Address))
                        continue;
                    //Add an new entry to the list
                    retList.Add(new ConnectionPair2(localEndpoint, remoteEndpoint, Packet2.Protocol.TCP), (int)procId);
                }
            }
            catch (Exception ex)
            {
                // free the buffer
                if (lpTable != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpTable);

                throw ex;
            }

            // free the buffer
            if (lpTable != IntPtr.Zero)
                Marshal.FreeHGlobal(lpTable);

            return retList;
        }

        /// <summary>
        /// Get a Dictionary of UDP session and Owner PID from the extended UDP table.
        /// </summary>
        /// <returns></returns>
        private SortedList<UDPConnection, int> GetUDPProcessTable(UDP_TABLE_CLASS ucls)
        {
            SortedList<UDPConnection, int> retList = new SortedList<UDPConnection, int>();
            uint errno;
            IntPtr lpTable = IntPtr.Zero;
            try
            {
                // the size of the MIB_EXTCPROW struct =  3*DWORD
                int bufferSize = 16;

                // allocate a dumb memory space in order to retrieve  nb of connexion
                lpTable = Marshal.AllocHGlobal(bufferSize);

                //getting infos
                errno = GetExtendedUdpTable(
                    lpTable,
                    ref bufferSize,
                    true,
                    (int)System.Net.Sockets.AddressFamily.InterNetwork,
                    ucls,
                    0);
                if (errno == 0x7A) //Insufficient_buffer
                {
                    // free allocated space in memory
                    Marshal.FreeHGlobal(lpTable);

                    //the bufferSize is the correct size of the returned structure
                    lpTable = Marshal.AllocHGlobal((int)bufferSize);
                    errno = GetExtendedUdpTable(
                            lpTable,
                            ref bufferSize,
                            true,
                            (int)System.Net.Sockets.AddressFamily.InterNetwork,
                            ucls,
                            0);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("GetExtendedUdpTable", (uint)errno));
                    }
                }
                else if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("GetExtendedUdpTable", (uint)errno));
                }

                int currentIndex = 0;

                //get the number of entries in the table
                int numEntries = Marshal.ReadInt32(lpTable);

                // for each entries
                for (int i = 0; i < numEntries; i++)
                {
                    // iterate the pointer of 4 (the size of the DWORD dwNumEntries)
                    currentIndex += 4;
                    // get the local IPv4 address 
                    uint localAddr = (uint)Marshal.ReadInt32(lpTable, currentIndex);

                    // iterate the pointer of 4
                    currentIndex += 4;
                    // get the local port of the connexion
                    int localPort = ConvertPort((uint)Marshal.ReadInt32(lpTable, currentIndex));

                    // Store the local endpoint in the struct and convertthe port in decimal (ie convert_Port())
                    IPEndPoint localEndpoint = new IPEndPoint(new IPAddress(localAddr), localPort);

                    // iterate the pointer of 4
                    currentIndex += 4;
                    // store the process ID
                    uint procId = (uint)Marshal.ReadInt32(lpTable, currentIndex);

                    if (IPAddress.IsLoopback(localEndpoint.Address))
                        continue;

                    UDPConnection conn = new UDPConnection(localEndpoint);

                    //Add an new entry to the list
                    if (!retList.ContainsKey(conn))
                        retList.Add(conn, (int)procId);
                }
            }
            catch (Exception ex)
            {
                // free the buffer
                if (lpTable != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpTable);

                throw ex;
            }

            // free the buffer
            if (lpTable != IntPtr.Zero)
                Marshal.FreeHGlobal(lpTable);

            return retList;
        }
        #endregion

        #region Test Methods

#if DEBUG
        public string DisplayFieldString(IntPtr frame)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                uint fieldCnt;
                uint errno;

                errno = NetmonAPI.NmGetFieldCount(frame, out fieldCnt);
                if (errno != 0)
                {
                    throw new Exception(FormatErrMsg("NmGetFieldCount() failed", errno));
                }

                unsafe
                {
                    sb.Append("Fields: ");
                    for (uint i = 0; i < fieldCnt; i++)
                    {
                        NmParsedFieldInfo fieldInfo = new NmParsedFieldInfo();
                        fieldInfo.Size = (ushort)Marshal.SizeOf(typeof(NmParsedFieldInfo));

                        errno = NetmonAPI.NmGetParsedFieldInfo(frame, i, (uint)0, ref fieldInfo);
                        if (errno != 0)
                        {
                            throw new Exception(FormatErrMsg("NmGetParsedFieldInfo() failed", errno));
                        }
                        else
                        {

                            char* fieldName = (char*)Marshal.AllocCoTaskMem((fieldInfo.NamePathLength + 1) * sizeof(char));
                            NetmonAPI.NmGetFieldName(
                                frame,
                                i,
                                NmParsedFieldNames.NamePath,
                                (uint)(fieldInfo.NamePathLength + 1),
                                fieldName);

                            string nameString = string.Empty;

                            char* tp = fieldName;

                            while (tp[0] != 0)
                            {
                                nameString += tp[0].ToString();
                                tp += 1;
                            }

                            sb.AppendFormat("\t[{0}]:{1}", i, nameString);

                            Marshal.FreeCoTaskMem((IntPtr)fieldName);
                        }


                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
#endif

        #endregion
    }


}
