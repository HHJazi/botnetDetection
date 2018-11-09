using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

using Microsoft.NetworkMonitor;

namespace Biotracker.Client.ProcessMonitor
{
    public class NMBase : IDisposable
    {
        #region Private members

        protected bool Disposed = false;
        protected bool Disposing = false;

        /// <summary>
        /// Error message.
        /// </summary>
        protected string ErrorMsg = default(string);

        /// <summary>
        /// Capture File Name that is passed in.
        /// </summary>
        protected string captureFileName = default(string);

        /// <summary>
        /// Capture file NMAPI handle
        /// </summary>
        protected IntPtr captureFileHandle = IntPtr.Zero;

        /// <summary>
        /// NetMon capture engine handle.
        /// </summary>
        protected IntPtr captureEngineHandle = IntPtr.Zero;

        /// <summary>
        /// The return size of the capture file.
        /// </summary>
        protected uint captureFileSize = 0;
        
        #endregion

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

        #region Constructor

        public NMBase()
        { 
            
        }

        public NMBase(string traceFile)
        {
            if (string.IsNullOrEmpty(traceFile))
            {
                throw new ArgumentNullException("No capture file defined.");
            }

            this.captureFileName = traceFile;
        }

        #endregion

        #region Public Methods

        public string GetErrorMsg()
        {
            return ErrorMsg;
        }

        public void Dispose()
        {
            if (this.Disposed == false)
            {
                this.Disposed = true;

                if (Disposing)
                {
                    NetmonAPI.NmCloseHandle(this.captureEngineHandle);
                    NetmonAPI.NmCloseHandle(this.captureFileHandle);
                }

                // Free the unmanaged resource ...
                this.captureEngineHandle = IntPtr.Zero;
                this.captureFileHandle = IntPtr.Zero;
            }
        }

        #endregion

        #region Private Methods

        protected string FormatErrMsg(string msg, uint errno)
        {
            return string.Format("{0}. Error=0x{1}\n", msg, errno.ToString("x"));
        }
        #endregion

    }
}
