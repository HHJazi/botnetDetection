using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace Biotracker.Client.ProcessMonitor
{
    [ServiceBehavior(
        InstanceContextMode=InstanceContextMode.PerCall, 
        AutomaticSessionShutdown=true, 
        ConcurrencyMode=ConcurrencyMode.Multiple, 
        IncludeExceptionDetailInFaults=true)]
    public class FlowFeatureDataSvc : IFlowFeatureDataSvc
    {
        private ServiceHost _svcHost;

        private static List<FlowFeature> _features;

        private static object _syncObj = new object();
        private static int _totalFeaturesReceived = 0;
        public static int TotalFeatuerRecieved
        {
            get { return _totalFeaturesReceived; }
        }


        public FlowFeatureDataSvc()
            : base()
        {
            _features = new List<FlowFeature>();
        }

        #region Interface Implementation

        public void SendFlowFeatures(IEnumerable<FlowFeature> features)
        {
            OperationContext context = OperationContext.Current;
            RemoteEndpointMessageProperty endpoint
                = context.IncomingMessageProperties[RemoteEndpointMessageProperty.Name]
                as RemoteEndpointMessageProperty;

            string ip = endpoint.Address;

            lock (_syncObj)
            {
                foreach (FlowFeature f in features)
                {
                    f.LoggerIp = ip;
                    
                    _features.Add(f);

                    _totalFeaturesReceived++;
                }
            }
        }

        #endregion

        #region Public Methods

        public IEnumerable<FlowFeature> GetFlowFeatures()
        {
            List<FlowFeature> features = null;
            lock (_syncObj)
            {
                features = _features;
                _features = new List<FlowFeature>();
            }

            return features;
        }

        public void StartDataService()
        {
            try
            {
#if true
                NetTcpBinding tcpBinding = new NetTcpBinding();
                tcpBinding.Name = "flowNetTcpBinding";
                tcpBinding.ReceiveTimeout = new TimeSpan(0, 5, 0);
                tcpBinding.SendTimeout = new TimeSpan(0, 5, 0);
                tcpBinding.ListenBacklog = 100;
                tcpBinding.MaxConnections = 3000;
                tcpBinding.PortSharingEnabled = false;
                tcpBinding.ReliableSession.Enabled = true;
                tcpBinding.ReliableSession.Ordered = true;
                tcpBinding.ReliableSession.InactivityTimeout = new TimeSpan(0, 5, 0);

                tcpBinding.Security.Mode = SecurityMode.None;
                //tcpBinding.Security.Mode = SecurityMode.Transport;
                //tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
                //tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
                //tcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;

                //Init Service host
                _svcHost = new ServiceHost(typeof(FlowFeatureDataSvc));

                _svcHost.CloseTimeout = new TimeSpan(0, 1, 0);
                _svcHost.OpenTimeout = new TimeSpan(0, 1, 0);

                String uri = String.Format("net.tcp://localhost:{0}/flowFeatureDataSvc", Properties.Settings.Default.DataSvcPort);

                _svcHost.AddServiceEndpoint(typeof(IFlowFeatureDataSvc), tcpBinding, uri);

                _svcHost.Description.Name = "Biotracker.Server.Core.DataService";

                ServiceThrottlingBehavior throttlingBehavior = new ServiceThrottlingBehavior();
                throttlingBehavior.MaxConcurrentSessions = 3000;
                throttlingBehavior.MaxConcurrentInstances = 3000;
                throttlingBehavior.MaxConcurrentCalls = 100;

                _svcHost.Description.Behaviors.Add(throttlingBehavior);

                //Add meta data 
                ServiceMetadataBehavior smb =
                    _svcHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

                if (smb == null)
                    smb = new ServiceMetadataBehavior();

                smb.HttpGetEnabled = true;
                smb.HttpGetUrl = new Uri("http://localhost:8702/flowFeatureDataSvc/");
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                _svcHost.Description.Behaviors.Add(smb);

                _svcHost.AddServiceEndpoint(
                    ServiceMetadataBehavior.MexContractName,
                    MetadataExchangeBindings.CreateMexHttpBinding(),
                    "http://localhost:8702/flowFeatureDataSvc/mex");

#else
                _svcHost = new ServiceHost(typeof(FlowFeatureDataSvc));
#endif   
                       
                _svcHost.Open();
            }
            catch (Exception ex)
            {
             if(_svcHost != null)
                    _svcHost.Abort();

                //remove this.
                System.Windows.Forms.MessageBox.Show(ex.ToString());

                throw ex;
            }
        }

        public void StopDataService()
        {
            try
            {
                if (_svcHost.State == CommunicationState.Opened)
                {
                    _svcHost.Close();
                }
            }
            catch (Exception)
            {
                ;
            }
            finally 
            {
                _svcHost = null;
            }
        }
        
        #endregion

        #region Private methods

        private int GetServerPort()
        {
            return Properties.Settings.Default.DataSvcPort;
        }

        #endregion
    }

}
