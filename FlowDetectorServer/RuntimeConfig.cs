using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Client.ProcessMonitor
{
    class RuntimeConfig
    {

   /// <summary>
        /// Parameterless constructor
        /// </summary>
        public RuntimeConfig()
        {

        }

        #region Public Methods

        public Dictionary<string, Boolean> liveFeatures()
        {

            Dictionary<string, Boolean> liveFeatures = new Dictionary<string, Boolean>();
            liveFeatures.Add("SrcMAC" , true);
            liveFeatures.Add("DestMAC", true);
            liveFeatures.Add("SrcIP2", true);
            liveFeatures.Add("DestIP2", true);
            liveFeatures.Add("SrcPort", true);
            liveFeatures.Add("DestPort", true);
            liveFeatures.Add("Protocol", true);
            liveFeatures.Add("TBT", true);
            liveFeatures.Add("FPS", true);
            liveFeatures.Add("APL", true);
            liveFeatures.Add("AB", true);
            liveFeatures.Add("BS", true);
            liveFeatures.Add("PS", true);
            liveFeatures.Add("DPL", true);
            liveFeatures.Add("PPS", true);
            liveFeatures.Add("PV", true);
            liveFeatures.Add("PX", true);
            liveFeatures.Add("NNP", true);
            liveFeatures.Add("NSP", true);
            liveFeatures.Add("PSP", true);
            liveFeatures.Add("Duration", true);
            liveFeatures.Add("type", true);

            return liveFeatures;

         
        }


        public string featuresList() // return all features name as a  line format
        {
            String featuresList = "SrcMAC, DestMAC, SrcIP2 , DestIP2 , SrcPort, DestPort, Protocol, TBT, FPS, APL, AB, BS,PS, DPL,PPS,PV,PX, NNP,NSP,PSP,Duration, type, IOPR, AIT, reconnect";
            return featuresList;
        }
        #endregion

    }
}
