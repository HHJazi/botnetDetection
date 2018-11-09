using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Client.ProcessMonitor
{
    public class NetworkEventArgs : EventArgs
    {
        
        public IEnumerable<FlowFeature> MalFlowFeatures { get; set; }
        public int TotalNbFlows {get;set;}
    
        public NetworkEventArgs(IEnumerable<FlowFeature> malFlows, int totalNb)
        {
            MalFlowFeatures = malFlows;
            TotalNbFlows = totalNb;
        }

    }
}
