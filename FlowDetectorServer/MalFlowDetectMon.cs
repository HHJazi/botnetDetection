using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.IsolatedStorage;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

using Biotracker.Signature.DT;

namespace Biotracker.Client.ProcessMonitor
{
    public delegate void NetworkDetectionEventHandler(object sender, NetworkEventArgs e);
    public delegate void PMErrorEventHandler(object sender, ErrorEventArgs2 e);

    public class MalFlowDetectMon : WorkerThread, IDisposable
    {
        private TestDecisionTree _tree = null;

        private string _nmErrorMsg = default(string);

        private string _traceFile = default(string);

        private string _sigFile = default(string); //default Signature

        //Signal for Process category change arrival.
        private static AutoResetEvent _procEvent = new AutoResetEvent(false);

        private FlowFeatureDataSvc _dataService = null;

        #region Event

        public event NetworkDetectionEventHandler DetectionEvent;

        public event PMErrorEventHandler ErrorEvent;

        #endregion

        #region Properties

        /// <summary>
        /// The Time Window parameter for capturing network flows.
        /// </summary>
        public TimeSpan TimeWindow
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public MalFlowDetectMon()
            : base()
        {
            _sigFile = Properties.Settings.Default.SignatureFile;

            _dataService = new FlowFeatureDataSvc();

            _procEvent = new AutoResetEvent(false);

            BuildDecisionTree(GetDecisionTreeSignature());
        }

        #endregion

        #region Public Methods

        public void ReloadSignature(string signatureFile)
        { 
            //TODO: stop the current running thread
            
            string xml = GetDecisionTreeSignature();

            if (BuildDecisionTree(xml) == false)
            {
                throw new ArgumentException("Signature file is corrupted.");
            }
        }

        public override void RunThread()
        {
            this.SetThreadState(WorkerState.STARTING);

            TimeSpan capTimeWindow = new TimeSpan(0, 0, 5);

            try
            {
                _dataService.StartDataService();

                this.SetThreadState(WorkerState.RUNNING);
                
                //Checking the 
                while (WaitHandle.WaitAny(_eventArray, capTimeWindow)  != 0)
                {
                    //Do Flow Detection
                    List<FlowFeature> inputFeatures = _dataService.GetFlowFeatures().ToList();
                    if (inputFeatures != null)
                    {
                        DetectNetworkFlows(inputFeatures);
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorEvent(ex);
            }

            this.SetThreadState(WorkerState.STOPPED);
        }

        public override void RequestStop()
        {
            base.RequestStop();

            if (_dataService != null)
            {
                _dataService.StopDataService();
                _dataService = null;
            }
        }

        public void Dispose()
        {
            try
            {
                File.Delete(_traceFile);
            }
            catch (Exception)
            { }
        }

        public string GetErrorMessage()
        {
            return _nmErrorMsg;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Build the Network event Signature decisiontree from the XML signature  string
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        private bool BuildDecisionTree(string signature)
        {
            try
            {
                _tree = TestDecisionTree.XmlDeserialize(signature);

                return _tree != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Read the stored signature from a file.
        /// </summary>
        /// <returns></returns>
        private string GetDecisionTreeSignature()
        {
            string xmlSig = default(string);

            string isoFilename = _sigFile;
            try
            {
                //using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
                //{ 
                //FIXME: change the signature file name to a datetime related string.
                //if (isoFile.FileExists(isoFilename))
                //{
                //    using (IsolatedStorageFileStream isoStream =
                //        new IsolatedStorageFileStream(isoFilename, FileMode.Open, FileAccess.Read, isoFile))
                //{
                //using (StreamReader sr = new StreamReader(isoStream))
                using (StreamReader sr = new StreamReader(isoFilename))
                {
                    xmlSig = sr.ReadToEnd();
                }
                //}
                //}
                //}

            }
            catch (Exception ex)
            {
                OnErrorEvent(ex);
            }

            return xmlSig;
        }

        /// <summary>
        /// This method examines the captured network flows using the DecisionTree classification.
        /// </summary>
        /// <param name="flows"></param>
        private void DetectNetworkFlows(List<FlowFeature> features)
        {
           
            try
            {
                if (_tree == null)
                {
                    throw new Exception("No signature was found.");
                }

                if (features.Count > 0)
                {
                    List<FlowFeature> malFlows = new List<FlowFeature>();

                    //OnErrorEvent(new Exception("Detecting on " + features.Count + " flows"));

                    foreach (FlowFeature feature in features)
                    {
                        List<AttributeValue> attrVals = new List<AttributeValue>();
                        attrVals.Add(new KnownNumericalValue(feature.PX));
                        attrVals.Add(new KnownNumericalValue(feature.APL));
                        attrVals.Add(new KnownNumericalValue(feature.PV));
                        attrVals.Add(new KnownNumericalValue(feature.DPL));
                        attrVals.Add(new KnownNumericalValue(feature.PPS));
                        attrVals.Add(new KnownSymbolicValue(feature.Protocol));
                        attrVals.Add(new KnownNumericalValue(feature.AB));
                        attrVals.Add(new KnownNumericalValue(feature.TBT));
                        attrVals.Add(new KnownNumericalValue(feature.BS));
                        attrVals.Add(new KnownNumericalValue(feature.PS));
                        attrVals.Add(new KnownNumericalValue(feature.NNP));
                        attrVals.Add(new KnownNumericalValue(feature.NSP));
                        attrVals.Add(new KnownNumericalValue(feature.Duration));
                        attrVals.Add(new KnownNumericalValue(feature.AIT));
                        attrVals.Add(new KnownNumericalValue(feature.IOPR));
                        attrVals.Add(new KnownNumericalValue(feature.Reconnect));
      


                        throw new Exception("inside malflowdetectmon");

                        
                        //attrVals.Add(new KnownSymbolicValue((int)(feature.Type)));

                        Item it = new Item(attrVals.ToArray());

                        KnownSymbolicValue guessedVal = _tree.GuessGoalAttribute(it);

                        feature.Type = guessedVal.IntValue;
                        
                        if (feature.Type != 0) 
                        {
                            feature.DetectionTimeStamp = DateTime.UtcNow;

                            malFlows.Add(feature);
                        }
                    }

                    OnDetectMaliciousFlows(malFlows, features.Count);
                }
            }
            catch (Exception ex)
            {
                OnErrorEvent(ex);
            }
        }

        /// <summary>
        /// This methods raises events when a detection occurs. 
        /// </summary>
        /// <param name="flowFeature"></param>
        /// <param name="totalNbFlows"></param>
        private void OnDetectMaliciousFlows(IEnumerable<FlowFeature> malFlowFeatures, int totalNbFlows)
        {
            if (this.DetectionEvent != null) 
            {
                this.DetectionEvent.Invoke(this, new NetworkEventArgs(malFlowFeatures, totalNbFlows));
            }
        }

        private void OnErrorEvent(Exception ex)
        {
            if (ex != null)
            {
                if (this.ErrorEvent != null)
                {
                    ErrorEvent.Invoke(this, new ErrorEventArgs2(ex.ToString()));
                }
            }
        }
        #endregion
    }
}
