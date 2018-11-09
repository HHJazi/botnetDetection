using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Biotracker.Client.ProcessMonitor
{
    [ServiceContract(Namespace="http://plurilock.com/ProcessMonitor")]
    public interface IFlowFeatureDataSvc
    {
        [OperationContract]
        void SendFlowFeatures(IEnumerable<FlowFeature> features);
    }
}
