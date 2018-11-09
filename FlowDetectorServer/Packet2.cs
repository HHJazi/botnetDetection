using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Biotracker.Client.ProcessMonitor
{
    /// <summary>
    /// This class implement a Internet IP packet.
    /// </summary>
    [Serializable]
    public class Packet2
    {
        public enum Protocol
        {
            TCP,
            UDP,
            Mixed
        }

        /// <summary>
        /// The timestamp of receiving this packet.
        /// </summary>
        private DateTime _packetTime;

        private byte[] _srcMAC;

        private byte[] _destMAC;

        /// <summary>
        /// (srcIP, srcPort, AddressFamily)
        /// </summary>
        private IPEndPoint _src;

        /// <summary>
        /// (destIP, destPort, AddressFamily)
        /// </summary>
        private IPEndPoint _dest;

        /// <summary>
        /// TCP/UDP or Mixed
        /// </summary>
        private Protocol _protocol;

        /// <summary>
        /// TCP/UDP payload length in bytes
        /// </summary>
        private uint _payloadLength;
        private uint _Length;

        private int _srcPort;
        private int _dstPort;

        #region Properties

        //public int SrcPort
        //{
        //    get { return _srcPort; }
        //}

        //public int DstPort
        //{
        //    get { return _dstPort; }
        //}


        public Protocol PacketProtocol
        {
            get { return _protocol; }
        }

        public IPEndPoint SrcEnd
        {
            get { return _src; }
        }

        public IPEndPoint DestEnd
        {
            get { return _dest; }
        }

        public byte[] SrcMAC 
        {
            get { return _srcMAC; }
        }

        public byte[] DestMAC
        {
            get { return _destMAC; }
        }

        public uint PayloadLength
        {
            get { return _payloadLength; }
        }

        public uint Length
        {
            get { return _Length; }
        }

        public DateTime ReceivingTime
        {
            get { return _packetTime; }
        }

        public bool SynFlag { get; set; }
        public bool FinFlag { get; set; }
        public bool AckFlag { get; set; }

        #endregion


        #region Constructor

        public Packet2()
        { }

        public Packet2(IPEndPoint src, IPEndPoint dest, Protocol prot, uint payload, DateTime pktReicevingTime) // THERE WAS AN EXTRA INPUT (INT S) THAT NEVER BEEN USED SO I REMOVED IT!!!!!!  
        {
            this._srcMAC = null;
            this._destMAC = null;

            this._src = src;
            this._dest = dest;
            this._protocol = prot;
            this._payloadLength = payload;
            this._packetTime = pktReicevingTime;
        }

        public Packet2(byte[] srcMAC, byte[] destMAC, IPEndPoint src, IPEndPoint dest, Protocol prot, uint payload, DateTime pktReicevingTime)
        {
            this._srcMAC = srcMAC;
            this._destMAC = destMAC;

            this._src = src;
            this._dest = dest;
            this._protocol = prot;
            this._payloadLength = payload;
            this._packetTime = pktReicevingTime;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion

    }
}
