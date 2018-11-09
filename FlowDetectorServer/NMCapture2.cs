using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.NetworkMonitor;

namespace Biotracker.Client.ProcessMonitor
{
    public class NMCapture2 : NMBase, IDisposable
    {
        #region Private Members
        private static readonly uint CaptureFileSize = 30000000; //Capture temp file size: 20M

        private static readonly uint CapturedFrameSize = 64; //The size of shortened frame size.

        /// <summary>
        /// Callback for parser loading.
        /// </summary>
        private CaptureCallbackDelegate CaptureCb;

        #endregion

        #region Events

        public event EventHandler NewFrameEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of NMCapture class. The trace file will be provided when startcapture()
        /// </summary>
        public NMCapture2()
            : base()
        {
            uint errno;

            errno = ConfigureCaptureEngine(0); //try to configure threading model to multi-threaded.
            if (errno == 0x80010106)
            {
                errno = ConfigureCaptureEngine(2);
            }

            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("Unable to Open Capture engine.", errno));
            }

            //CaptureCallback handle. 
            this.CaptureCb = new CaptureCallbackDelegate(CaptureCallBack);

        }

        /// <summary>
        ///  Initialize a new instance of NMCapture class. 
        /// </summary>
        /// <param name="traceFile"></param>
        public NMCapture2(string traceFile)
            : base(traceFile)
        {
            uint errno;

            errno = ConfigureCaptureEngine(0); //try to configure threading model to multi-threaded.
            if (errno == 0x80010106)
            {
                errno = ConfigureCaptureEngine(2);
            }

            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("Unable to Open Capture engine.", errno));
            }

            //CaptureCallback handle. 
            this.CaptureCb = new CaptureCallbackDelegate(this.CaptureCallBack);

            // Create a caputre file for storing trace
            errno = NetmonAPI.NmCreateCaptureFile(
                    traceFile,
                    CaptureFileSize,
                    NmCaptureFileFlag.WrapAround,
                    out this.captureFileHandle,
                    out this.captureFileSize);

            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmCreateCaptureFile() failed", errno));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the NMCapture engine to collect traffic data. The trace file handle must be provided.
        /// </summary>
        /// <param name="adapterIndex">index of the target adapter.</param>
        /// <returns></returns>
        public bool StartCapture(List<uint> adapters)
        {
            uint errno;

            try
            {
                foreach (uint adapterIndex in adapters)
                {
                    //Configure Adapter for capturing
                    errno = NetmonAPI.NmConfigAdapter(
                        this.captureEngineHandle,
                        adapterIndex,
                        CaptureCb,
                        this.captureFileHandle,
                        NmCaptureCallbackExitMode.DiscardRemainFrames);

                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("NmConfigAdapter() failed", errno));
                    }

                    errno = NetmonAPI.NmStartCapture(this.captureEngineHandle, adapterIndex, NmCaptureMode.LocalOnly);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("NmStartCapture() failed", errno));
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                NetmonAPI.NmCloseHandle(this.captureEngineHandle);
                NetmonAPI.NmCloseHandle(this.captureFileHandle);
                this.captureEngineHandle = IntPtr.Zero;
                this.captureFileHandle = IntPtr.Zero;

                ErrorMsg += ex.ToString();

                return false;
            }
        }

        /// <summary>
        /// Start the NMCapture engine to collect traffic data
        /// </summary>
        /// <param name="adapterIndex">index of the target adapter.</param>
        /// <returns></returns>
        public bool StartCapture(List<uint> adapters, string captureFile)
        {
            uint errno;

            // Create a caputre file for storing trace
            errno = NetmonAPI.NmCreateCaptureFile(
                    captureFile,
                    CaptureFileSize,
                    NmCaptureFileFlag.WrapAround,
                    out this.captureFileHandle,
                    out this.captureFileSize);

            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("NmCreateCaptureFile() failed", errno));
            }

            return StartCapture(adapters);
        }

        public bool ResumeCapture(List<uint> adapters)
        {
            uint errno;

            try
            {
                foreach (uint adapterIndex in adapters)
                {
                    errno = NetmonAPI.NmResumeCapture(this.captureEngineHandle, adapterIndex);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("NmResumeCapture() failed", errno));
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                NetmonAPI.NmCloseHandle(this.captureEngineHandle);
                NetmonAPI.NmCloseHandle(this.captureFileHandle);
                this.captureEngineHandle = IntPtr.Zero;
                this.captureFileHandle = IntPtr.Zero;

                ErrorMsg += ex.ToString();

                return false;
            }

        }

        /// <summary>
        /// Pause the capturing.
        /// </summary>
        /// <param name="adapterIndex"></param>
        /// <returns></returns>
        public bool PauseCapture(List<uint> adapters)
        {
            uint errno;

            try
            {
                foreach (uint adapterIndex in adapters)
                {
                    errno = NetmonAPI.NmPauseCapture(this.captureEngineHandle, adapterIndex);
                    if (errno != 0)
                    {
                        throw new Exception(FormatErrMsg("NmPauseCapture() failed", errno));
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                NetmonAPI.NmCloseHandle(this.captureEngineHandle);
                NetmonAPI.NmCloseHandle(this.captureFileHandle);
                this.captureEngineHandle = IntPtr.Zero;
                this.captureFileHandle = IntPtr.Zero;

                ErrorMsg += ex.ToString();

                return false;
            }
        }

        /// <summary>
        /// Stops the capture and clean the capture engine handle.
        /// </summary>
        /// <param name="adapterIndex"></param>
        /// <returns></returns>
        public bool StopCapture(List<uint> adapters)
        {
            if (this.captureEngineHandle != IntPtr.Zero)
            {
                foreach (uint adapterIndex in adapters)
                    NetmonAPI.NmStopCapture(this.captureEngineHandle, adapterIndex);

                NetmonAPI.NmCloseHandle(this.captureFileHandle);
                this.captureFileHandle = IntPtr.Zero;
            }

            return true;
        }

        /// <summary>
        /// Returns a list of adpater index those are likely the external LAN or WiFi adapters.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPossibleAdapters()
        {
            try
            {
                return (from i in this.FindEthernetApdapterIndex(this.GetAdapters())
                        select (uint)i).ToList<uint>();
            }
            catch (Exception)
            {
                return new List<uint>();
            }
        }

        public IntPtr GetCaptureFileHanle()
        {
            return this.captureFileHandle;
        }

        #endregion

        #region Private Methods

        private uint ConfigureCaptureEngine(ushort threadingModel)
        {
            uint errno;

            NM_API_CONFIGURATION apiConfig = new NM_API_CONFIGURATION();

            apiConfig.Size = (ushort)Marshal.SizeOf(apiConfig);
            errno = NetmonAPI.NmGetApiConfiguration(ref apiConfig);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("Unable to retrieve configuration.", errno));
            }

            apiConfig.ThreadingMode = threadingModel; //threading model:

            errno = NetmonAPI.NmApiInitialize(ref apiConfig);
            if (errno != 0)
            {
                throw new Exception(FormatErrMsg("Unable to initialize configuration.", errno));
            }

            // Open a Capture Engine.
            return NetmonAPI.NmOpenCaptureEngine(out this.captureEngineHandle);
        }

        /// <summary>
        /// Get all adapters in the system.
        /// 
        /// For the NDIS_MEDIUM enumeration: 0 - NdisMedium802_3; 16 - NdisMediumNative802_11
        /// </summary>
        /// <returns></returns>
        private List<NM_NIC_ADAPTER_INFO> GetAdapters()
        {
            uint errno;
            uint adapterCnt;
            List<NM_NIC_ADAPTER_INFO> adapters = new List<NM_NIC_ADAPTER_INFO>();

            if (this.captureEngineHandle != IntPtr.Zero)
            {
                errno = NetmonAPI.NmGetAdapterCount(this.captureEngineHandle, out adapterCnt);
                if (errno == 0)
                {

                    for (uint i = 0; i < adapterCnt; i++)
                    {
                        NM_NIC_ADAPTER_INFO adapterInfo = new NM_NIC_ADAPTER_INFO();
                        adapterInfo.Size = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NM_NIC_ADAPTER_INFO));

                        errno = NetmonAPI.NmGetAdapter(this.captureEngineHandle, i, ref adapterInfo);
                        if (errno == 0)
                        {
                            //Only take Ethernet or 802.11 wireless adapters.
                            if (adapterInfo.MediaType == 0 || (uint)adapterInfo.MediaType == 16)
                            {
                                adapters.Add(adapterInfo);
                            }
                        }
                        else
                        {
                            throw new Exception("NmGetAdapter() failed. Error=" + errno);
                        }
                    }
                }
            }

            return adapters;
        }

        /// <summary>
        /// Callback function for capture.
        /// </summary>
        /// <param name="hCapEngine"></param>
        /// <param name="adapterIndex"></param>
        /// <param name="callerContext"></param>
        /// <param name="hRawFrame"></param>
        private void CaptureCallBack(IntPtr hCapEngine, uint adapterIndex, IntPtr callerContext, IntPtr hRawFrame)
        {
            if (callerContext != IntPtr.Zero)
            {
                uint errno;

                unsafe
                {
                    uint frameLen = 0;

                    errno = NetmonAPI.NmGetRawFrameLength(hRawFrame, out frameLen);
                    if (errno != 0)
                    {
                        return;
                    }

                    byte[] frameBuf = new byte[CapturedFrameSize];

                    fixed (byte* pBuf = frameBuf)
                    {
                        if (frameLen >= CapturedFrameSize)
                        {
                            NM_TIME pTime = new NM_TIME();
                            //Get the TimeStamp of the frame for building the new shortened frame
                            errno = NetmonAPI.NmGetFrameTimeStampEx(hRawFrame, ref pTime);
                            if (errno != 0)
                            {
                                return;
                            }

                            uint captureSize = 0;
                            //use NmGetPartiaRawlFrame() to get the wanted length of the raw frame, 
                            errno = NetmonAPI.NmGetPartialRawFrame(
                                hRawFrame, 
                                0, //offset
                                CapturedFrameSize, 
                                pBuf, 
                                out captureSize
                                );
                            if (errno != 0)
                            {
                                return;
                            }

                            IntPtr hPartialRawFrame;
                            errno = NetmonAPI.NmBuildRawFrameFromBufferEx(
                                (IntPtr)pBuf, 
                                CapturedFrameSize, 
                                0, //media type, optional
                                ref pTime, 
                                out hPartialRawFrame
                                );
                            if (errno != 0)
                            {
                                return;
                            }

                            NetmonAPI.NmAddFrame(callerContext, hPartialRawFrame);
                        }
                        else
                        {
                            NetmonAPI.NmAddFrame(callerContext, hRawFrame);
                        }
                    }
                }


            }

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

        #endregion

        #region Test Methods

#if DEBUG
        public string DisplayAdapters()
        {
            List<NM_NIC_ADAPTER_INFO> adapters = GetAdapters();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < adapters.Count; i++)
            {
                NM_NIC_ADAPTER_INFO ad = adapters[i];

                sb.AppendFormat("NIC #{0}: {1}\n", i, new string(adapters[i].FriendlyName));
            }

            return sb.ToString();
        }


#endif

        #endregion
    }


}
