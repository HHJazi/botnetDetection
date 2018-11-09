using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Biotracker.Client.ProcessMonitor
{
    [ServiceContract(Namespace = "http://plurilock.com/Biotracker/", Name="FlowDataService")]
    public interface IFlowExchange
    {
        [OperationContract]
        bool SendFlowFeatures(IEnumerable<FlowFeature> features);
    }
}
