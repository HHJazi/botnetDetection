using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Client.ProcessMonitor
{
    /// <summary>
    /// This class implement some utility functions for the whole project
    /// </summary>
    class Utilities
    {

         /// <summary>
        /// Parameterless constructor
        /// </summary>
        public Utilities()
        {

        }

        #region Public Methods
        public string convertIPtoString(byte[] inputIP)
        {
            string outputIP;
            outputIP = inputIP[0].ToString() + "." + inputIP[1].ToString() + "." + inputIP[2].ToString() + "." + inputIP[3].ToString();
            return outputIP;
        }

        public string convertMACtoString(byte[] inputMAC)
        {
            string outputMAC;
            outputMAC = inputMAC[0].ToString() + ":" + inputMAC[1].ToString() + ":" + inputMAC[2].ToString() + ":" + inputMAC[3].ToString()
                    + ":" + inputMAC[4].ToString() + ":" + inputMAC[5].ToString();
            return outputMAC;
        }


        public string featuresList() // return all features name as a  line format
        {
            String featuresList = "SrcMAC, DestMAC, SrcIP2 , DestIP2 , SrcPort, DestPort, Protocol, TBT, FPS, APL, AB, BS,PS, DPL,PPS,PV,PX, NNP,NSP,PSP,Duration";
            return featuresList;
        }
        #endregion

    }
}
