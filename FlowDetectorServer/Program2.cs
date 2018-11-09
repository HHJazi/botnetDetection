using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace Biotracker.Client.ProcessMonitor
{
    using Biotracker.Signature.DT;

    static class Program2
    {
        static FDSApplicationContext _appContext;

        [STAThread]
        static void Main(String[] args)
        {

            int temp = 3;
             

            System.Console.WriteLine("we start here in program2!!");
            Application.EnableVisualStyles();
            System.Console.WriteLine("6666666666");
            Application.SetCompatibleTextRenderingDefault(false);
            System.Console.WriteLine("777777777777777");
            Checker chObj = Checker.GetCheckerObject();
            System.Console.WriteLine("888888888");
            _appContext = new FDSApplicationContext();
            System.Console.WriteLine("9999999");
            Application.Run(_appContext);
        }
    }

    class Checker
    {
        public Checker()
        {

        }

        public static Checker GetCheckerObject()
        {
            return new Checker();
        }
    }

    class FDSApplicationContext : ApplicationContext
    {
        private NotifyIcon _notifyIcon;
        private ContextMenu _contextMenu;
        private MenuItem menuTraining;
        private MenuItem menuOfflineDetection;

        private IContainer _components;
        private OpenFileDialog _openTrainingSetDlg;
        private OpenFileDialog _openOfflineDetectionDlg;

        private Thread _trainingThread = null;
        private Thread _offlineDetectionThread = null;
    
        private TestDecisionTree _tree = null;

        private double _trainingPercentage = double.NaN;

        private List<FlowFeature> _maliciousFlowsDetected = new List<FlowFeature>();
        private int _totalNbFlowExamined = 0;
        private DateTime _lastMalFlowDetectedTime = DateTime.MinValue;

        private Thread _dbThread;

        private MalFlowDetectMon _malFlowDetector;

        private string _malFlowFile = "malflow.dat";
        private string _lastErrorMsg = default(string);

        private int _dataServicePort;

        private MySqlDao dao = null;

        private Thread _featureProducer = null;
        private NMParser _parser = null;
        private ProducerConsumer<FlowFeature> _flowFeatureProducerConsumer = null;

        public FDSApplicationContext()
            : base()
        {
            try
            {
                dao = new MySqlDao();
                Flow2.SetLabelTable(dao.GetFlowLabels());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("Please check DB connection, Exitting");
                Application.Exit();
            }
            
            //reading the default signature file
                string xmlSig = default(string);
                try
                {
                    System.Console.WriteLine("the path is:"+Properties.Settings.Default.SignatureFile);
                    using (StreamReader sr = new StreamReader(Properties.Settings.Default.SignatureFile))
                    {
                        xmlSig = sr.ReadToEnd();
                    }
                    System.Diagnostics.Debug.WriteLine("Deserializing Signature File: " + Properties.Settings.Default.SignatureFile);
                    _tree = TestDecisionTree.XmlDeserialize(xmlSig);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem with Signature File");
                    //terminate the thread, just return
                    return;
                }

            _openTrainingSetDlg = new OpenFileDialog();
            _openTrainingSetDlg.CheckFileExists = true;
            _openTrainingSetDlg.CheckPathExists = true;
            
            _openOfflineDetectionDlg = new OpenFileDialog();
            _openOfflineDetectionDlg.CheckFileExists = true;
            _openOfflineDetectionDlg.CheckPathExists = true;



            _trainingPercentage = Properties.Settings.Default.TrainingPercentage;

            _dataServicePort = Properties.Settings.Default.DataSvcPort;

            _malFlowDetector = new MalFlowDetectMon();

            ///TODO: We need to make this timewindow configrable
            _malFlowDetector.TimeWindow = new TimeSpan(0,0,Properties.Settings.Default.TimeWindow); 
            //_malFlowDetector.TimeWindow = new TimeSpan(0, 0, 1);
            _malFlowDetector.DetectionEvent += new NetworkDetectionEventHandler(ProcessNetworkEvent);
            _malFlowDetector.ErrorEvent += new PMErrorEventHandler(DisplayErrorEvent);

            _malFlowDetector.Start();

            _dbThread = CreateDatabaseThread();
            _dbThread.Start();

            _components = new System.ComponentModel.Container();
            _contextMenu = CreateContextMenu();
            _notifyIcon = CreateSystrayIcon("Flow Detector Server", true);
            _notifyIcon.ContextMenu = _contextMenu;

            this.ThreadExit += new EventHandler(this.SystrayIcon_ApplicationExit);

            this.ThreadExit += new EventHandler(this.Application_Exit);
        }

        private void ProcessNetworkEvent(object sender, NetworkEventArgs e)
        {
            lock (_maliciousFlowsDetected)
            {
                if (e.MalFlowFeatures.Count() > 0)
                {
                    _maliciousFlowsDetected.AddRange(e.MalFlowFeatures);
                }

                _totalNbFlowExamined += e.TotalNbFlows;
            }
        }

        /// <summary>
        /// Thread for training the decisition tree 
        /// </summary>
        private void TrainingThread()
        {
            System.Console.WriteLine("Inside the training!!!!!!!!!!!!");
            List<FlowFeature> trainingFeatures;
          //  List<FlowFeature> trainingFeatures;

             //MySqlDao dao = null;


             try
             {
                 //dao = new MySqlDao();
                 // set data table
                 //Flow.SetLabelTable(dao.GetFlowLabels());

                 string traceFile = _openTrainingSetDlg.FileName;
                 System.Console.WriteLine("the training file is:"+traceFile);
                 if (File.Exists(traceFile) == false)
                     throw new FileNotFoundException("Trace file " + traceFile + " doesn't exist.");

                 NMParser parser = new NMParser();

                 IEnumerable<FlowFeature> allFeatures = GetFlowFeaturesFromTraceFile(parser, traceFile, -1);

                 trainingFeatures = allFeatures.ToList();

                 List<Attribute> attributes = new List<Attribute>();
                 attributes.Add(new NumericalAttribute("PX"));
                 attributes.Add(new NumericalAttribute("APL"));
                 attributes.Add(new NumericalAttribute("PV"));
                 attributes.Add(new NumericalAttribute("DPL"));
                 attributes.Add(new NumericalAttribute("PPS"));
                 attributes.Add(new IdSymbolicAttribute("Protocol", new List<string>() { "TCP", "UDP", "Mixed" }));
                 attributes.Add(new NumericalAttribute("FPS"));
                
                 attributes.Add(new NumericalAttribute("AB"));
                 attributes.Add(new NumericalAttribute("TBT"));
                 attributes.Add(new NumericalAttribute("BS"));
                 attributes.Add(new NumericalAttribute("PS"));
                 attributes.Add(new NumericalAttribute("NNP"));
                 attributes.Add(new NumericalAttribute("NSP"));
                 attributes.Add(new NumericalAttribute("PSP"));
                 attributes.Add(new NumericalAttribute("Duration"));
                 attributes.Add(new NumericalAttribute("AIT"));
                 attributes.Add(new NumericalAttribute("IOPR"));
                attributes.Add(new NumericalAttribute("Reconnect"));
                attributes.Add(new IdSymbolicAttribute("Type", Flow2.GetFlowTypeNames()));
               //  System.Diagnostics.Debug.WriteLine("TrainingThread1");



                 AttributeSet attrSet = new AttributeSet(attributes);

                 ItemSet itemSet = new ItemSet(attrSet);
                 Dictionary<int,int> maliciousFlowCounter = new Dictionary<int,int>();
                 int value;

              

                 foreach (FlowFeature feature in trainingFeatures)
                 {
                     List<AttributeValue> attrVals = new List<AttributeValue>();
                     attrVals.Add(new KnownNumericalValue(feature.PX));
                     attrVals.Add(new KnownNumericalValue(feature.APL));
                     attrVals.Add(new KnownNumericalValue(feature.PV));
                     attrVals.Add(new KnownNumericalValue(feature.DPL));
                     attrVals.Add(new KnownNumericalValue(feature.PPS));

                     attrVals.Add(new KnownSymbolicValue((int)feature.Protocol));
                     attrVals.Add(new KnownNumericalValue(feature.FPS));

                   


                     attrVals.Add(new KnownNumericalValue(feature.AB));
                     attrVals.Add(new KnownNumericalValue(feature.TBT));
                     attrVals.Add(new KnownNumericalValue(feature.BS));
                     attrVals.Add(new KnownNumericalValue(feature.PS));
                     attrVals.Add(new KnownNumericalValue(feature.NNP));
                     attrVals.Add(new KnownNumericalValue(feature.NSP));
                     attrVals.Add(new KnownNumericalValue(feature.PSP));
                     attrVals.Add(new KnownNumericalValue(feature.Duration));
                     attrVals.Add(new KnownNumericalValue(feature.AIT));
                     attrVals.Add(new KnownNumericalValue(feature.IOPR));
                     attrVals.Add(new KnownNumericalValue(feature.Reconnect));
                     attrVals.Add(new KnownSymbolicValue(feature.Type));
                   //  System.Diagnostics.Debug.WriteLine("TrainingThread2");
                 //    attrVals.Add(new ((DateTime)feature.DetectionTimeStamp));



                     Item it = new Item(attrVals.ToArray());

                     if (feature.Type > 0) // if the flow is not normal, count
                     {
                         if (!maliciousFlowCounter.TryGetValue(feature.Type, out value))
                             maliciousFlowCounter.Add(feature.Type, 1);
                         else
                             maliciousFlowCounter[feature.Type]++;
                     }
                     
                     itemSet.Add(it);
                 }


                 foreach(int index in maliciousFlowCounter.Keys)
                    System.Diagnostics.Debug.WriteLine("Number of Malicious Flows for type: "+ Flow2.GetFlowTypeName(index)+ "  is: "+ maliciousFlowCounter[index].ToString());



                 SymbolicAttribute goalAttribute = attrSet.FindByName("Type") as SymbolicAttribute;

                 List<Attribute> testAttributes = new List<Attribute>();

                 testAttributes.Add(attrSet.FindByName("PX"));
                 testAttributes.Add(attrSet.FindByName("APL"));
                 testAttributes.Add(attrSet.FindByName("PV"));
                 testAttributes.Add(attrSet.FindByName("DPL"));
                 testAttributes.Add(attrSet.FindByName("PPS"));
                 testAttributes.Add(attrSet.FindByName("Protocol"));
                 testAttributes.Add(attrSet.FindByName("FPS"));
             //    testAttributes.Add(attrSet.FindByName("Type"));
                 testAttributes.Add(attrSet.FindByName("AB"));
                 testAttributes.Add(attrSet.FindByName("TBT"));
                 testAttributes.Add(attrSet.FindByName("BS"));
                 testAttributes.Add(attrSet.FindByName("PS"));
                 testAttributes.Add(attrSet.FindByName("NNP"));
                 testAttributes.Add(attrSet.FindByName("NSP"));
                 testAttributes.Add(attrSet.FindByName("PSP"));
                 testAttributes.Add(attrSet.FindByName("Duration"));
                 testAttributes.Add(attrSet.FindByName("AIT"));
                 testAttributes.Add(attrSet.FindByName("IOPR"));
                 testAttributes.Add(attrSet.FindByName("Reconnect"));

              //   System.Diagnostics.Debug.WriteLine("TrainingThread3");


                 SimpleDecisionTreeBuilder builder = new SimpleDecisionTreeBuilder(   /// create tree hear!
                     new WeightedItemSet(itemSet),
                     new AttributeSet(testAttributes),
                     goalAttribute);

                 builder.ScoreThreshold = 0.0001d; // 0.0001 * itemSet.Size();
                 System.Diagnostics.Debug.WriteLine("DT ScoreThreshold is " + builder.ScoreThreshold.ToString());

                 LearningDecisionTree dt = builder.Build();

                 TestDecisionTree tdt = new TestDecisionTree(dt);

                 StoreDecisionTree(tdt);
             }
             catch (ThreadInterruptedException)
             {
                 ;
             }
             catch (ThreadAbortException)
             {
                 ;
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.ToString(), "Error");
             }
             finally 
             {
                 menuTraining.Text = Properties.Resources.StartTrainingText;
             }
        }

        private void OfflineDetectionThread()
        {
            string traceFile = null;
            try
            {
                traceFile = _openOfflineDetectionDlg.FileName;
                if (File.Exists(traceFile) == false)
                    throw new FileNotFoundException("Trace file " + traceFile + " doesn't exist.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
            _flowFeatureProducerConsumer = new ProducerConsumer<FlowFeature>(consumeFeature,false,true,true);
            _parser = new NMParser(traceFile,_malFlowDetector.TimeWindow,_flowFeatureProducerConsumer);
            System.Diagnostics.Debug.WriteLine("Created new parser");

            //Get all the flows from the trace file
            System.Diagnostics.Debug.WriteLine("Extracting features from tracefile");
            _featureProducer = new Thread(new ThreadStart(_parser.EnqueueNetworkFlowFeatures));

            ///// inja print konnnnnnnnnnnnnnnnnnnnnnnnnn

            _featureProducer.Start();
            _featureProducer.Join();

            menuOfflineDetection.Text = "Start Offline Detection";
            _parser = null;
            _flowFeatureProducerConsumer.Stop();
            _featureProducer.Abort();
            _featureProducer = null;
            _flowFeatureProducerConsumer = null;

            if (traceFile != null)
                MessageBox.Show("Finished Processing " + traceFile + " trace file...");
        } 


        /// <summary>
        /// Extract FlowFeatures from a trace file. Once the extract number of flow features are obtained, it should stop 
        /// </summary>
        /// <param name="nmc"></param>
        /// <param name="traceFileName"></param>
        /// <param name="requiredCount">if the number is less than 0, extract all flows from the trace file.</param>
        /// <returns></returns>
        private IEnumerable<FlowFeature> GetFlowFeaturesFromTraceFile(NMParser nmc, string traceFileName, int requiredCount)
        {
            List<FlowFeature> features = new List<FlowFeature>();

            //The Network Flows are extract from the tracefile by the time window.
            //The GetNetworkFlows() function uses yield return.
            IEnumerable<Flow2> temp = nmc.GetNetworkFlows(traceFileName, _malFlowDetector.TimeWindow);
            
            
            
            var rows = new List<String>();

            foreach (Flow2 f in temp)
            //foreach(Flow f in nmc.GetNetworkFlows())
            {

                FlowFeature feature = f.GenerateFeatuesInTimeWindow();
                if (feature != null)
                {
                    feature.LoggerIp = "Offline";
               
                    rows.Add(feature.featuresToString());
                    features.Add(feature);
                }

                if (features.Count >= requiredCount && requiredCount > 0)
                    break;
            }

            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\ebiglarb\Desktop\signature.csv"))
            //{
            //    RuntimeConfig config = new RuntimeConfig();
            //    String featuresNameList = config.featuresList();
                   
            //    file.WriteLine(featuresNameList);
            //    foreach (string line in rows)
            //    {

            //        file.WriteLine(line);

            //    }
            //}
            return features;
        }

        private void consumeFeature(FlowFeature feature)
        {
            try{
                    if (this._tree == null)
                    {
                        throw new Exception("No signature was found.");
                    }

                        //OnErrorEvent(new Exception("Detecting on " + features.Count + " flows"));

                            List<AttributeValue> attrVals = new List<AttributeValue>();
                            attrVals.Add(new KnownNumericalValue(feature.PX));
                            attrVals.Add(new KnownNumericalValue(feature.APL));
                            attrVals.Add(new KnownNumericalValue(feature.PV));
                            attrVals.Add(new KnownNumericalValue(feature.DPL));
                            attrVals.Add(new KnownNumericalValue(feature.PPS));
                            attrVals.Add(new KnownSymbolicValue(feature.Protocol));
                            attrVals.Add(new KnownNumericalValue(feature.FPS));
                            attrVals.Add(new KnownNumericalValue(feature.AB));
                            attrVals.Add(new KnownNumericalValue(feature.TBT));
                            attrVals.Add(new KnownNumericalValue(feature.BS));
                            attrVals.Add(new KnownNumericalValue(feature.PS));
                            attrVals.Add(new KnownNumericalValue(feature.NNP));
                            attrVals.Add(new KnownNumericalValue(feature.NSP));
                            attrVals.Add(new KnownNumericalValue(feature.PSP));
                            attrVals.Add(new KnownNumericalValue(feature.Duration));
                            attrVals.Add(new KnownNumericalValue(feature.AIT));
                            attrVals.Add(new KnownNumericalValue(feature.IOPR));
                            attrVals.Add(new KnownNumericalValue(feature.Reconnect));
                           



                          //  System.Diagnostics.Debug.WriteLine("consume feature 1");
                            //attrVals.Add(new KnownSymbolicValue((int)(feature.Type)));

                            Item it = new Item(attrVals.ToArray());

                            KnownSymbolicValue guessedVal = _tree.GuessGoalAttribute(it);

                            feature.Type = guessedVal.IntValue;
                            

                            if (feature.Type != 0)
                            {
                              //  feature.DetectionTimeStamp = DateTime.UtcNow;

                                //update malicious flows and total flows count
                                lock (_maliciousFlowsDetected)
                                {
                                    _maliciousFlowsDetected.Add(feature);
                                }
                            }
                           
                            feature = null;
                            it = null;
                            attrVals = null;

                            _totalNbFlowExamined += 1;
                    }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }

}


        private void StoreDecisionTree(TestDecisionTree tree)
        {
            
            SaveFileDialog saveDlg = new SaveFileDialog();
            try
            {
                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    string xml = tree.XmlSerialize();
                    if (!string.IsNullOrEmpty(xml))
                    {
                        using (StreamWriter sw = new StreamWriter(saveDlg.OpenFile()))
                        {
                            sw.WriteLine(xml);
                            sw.Flush();
                        }
                    }
                }
            }
            catch (FieldAccessException fae)
            {
                MessageBox.Show(saveDlg.FileName + " is not accessible.", "Error", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK);
            }
            finally
            {
                System.Console.WriteLine("!!!!!!!!!!!want to save tree in:");
                saveDlg.Dispose();
            }
        }

        private void Application_Exit(Object o, EventArgs e)
        {
            if (_malFlowDetector != null)
            {
                _malFlowDetector.RequestStop();
            }

            if (_trainingThread != null)
            {
                _trainingThread.Interrupt();            
            }

            if (_offlineDetectionThread != null)
            {
                _offlineDetectionThread.Interrupt();            
            }

            if (_dbThread != null)
            {
                _dbThread.Interrupt();
            }

            if (_components != null)
                _components.Dispose();
        }

        private void SystrayIcon_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
            catch (Exception)
            {
                ;
            }
        }

        /// <summary>
        /// Create the Systray icon
        /// </summary>
        /// <param name="text"></param>
        /// <param name="visible">true - icon visible; false - icon invisible</param>
        /// <returns></returns>
        private NotifyIcon CreateSystrayIcon(string text, bool visible)
        {
            NotifyIcon icon = new NotifyIcon(_components);
            //icon.Icon = new System.Drawing.Icon("icon_transparent.ico");
            icon.Icon = GetProcessIcon();
            icon.Text = text;
            icon.BalloonTipText = text;
            icon.Visible = visible;
            icon.DoubleClick += new EventHandler(NotifyIcon_DoubleClick);
            icon.MouseMove += new MouseEventHandler(NotifyIcon_MouseHover);
            icon.BalloonTipClosed += new EventHandler(NotifyIcon_BallonTipClosed);
            return icon;
        }

        private ContextMenu CreateContextMenu()
        {
            List<MenuItem> items = new List<MenuItem>();

            //add context menu items here.
            MenuItem miLastError = new MenuItem("Display Last Error");
            miLastError.Click += new EventHandler(
                (o, e) =>
                {
                    MessageBox.Show(
                        string.IsNullOrEmpty(_lastErrorMsg) ? "There is no error message."
                        : _lastErrorMsg
                        , "Last Error Message", MessageBoxButtons.OK);
                }
                );
            items.Add(miLastError);

            // Change Decision Tree Signature menu item
            // this can be done through the xml configuration file
            /*
            MenuItem miChangeSignature = new MenuItem("Change Detection Signature");
            miChangeSignature.Click += new EventHandler(OnChangeSignatureFile);
            items.Add(miChangeSignature);
            */

            // Training Signature menu item
            menuTraining = new MenuItem();
            menuTraining.Text = Properties.Resources.StartTrainingText;
            menuTraining.Click += new EventHandler(
                (o, e) =>
                {
                    if(menuTraining.Text.Equals(Properties.Resources.CancelTrainingText) && 
                        _trainingThread.ThreadState == System.Threading.ThreadState.Running)
                    {
                        _trainingThread.Abort();
                        menuTraining.Text = Properties.Resources.StartTrainingText;
                        _trainingThread = null;
                        return;
                    }

                    if (_openTrainingSetDlg.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    if (_trainingThread == null
                        || _trainingThread.ThreadState == System.Threading.ThreadState.Stopped)
                    {
                        _trainingThread = new Thread(new ThreadStart(this.TrainingThread));
                        _trainingThread.SetApartmentState(ApartmentState.STA);
                        _trainingThread.Name = "Training Thread";
                        _trainingThread.Start();

                        menuTraining.Text = Properties.Resources.CancelTrainingText;
                    }
                    
                    /*
                    else if (_trainingThread.ThreadState == System.Threading.ThreadState.Running)
                    {
                        _trainingThread.Abort();
                        menuTraining.Text = Properties.Resources.StartTrainingText;
                    }
                    * */

                });
            items.Add(menuTraining);

            // Display CPU usage menu item
            MenuItem menuCpuUsage = new MenuItem("CPU Usage");
            menuCpuUsage.Click += new EventHandler(
                (o, e) =>
                {
                    if (_notifyIcon != null)
                    {
                        _notifyIcon.BalloonTipText = "CPU: " + CpuUsage().ToString("P1");
                        _notifyIcon.ShowBalloonTip(1000);
                    }
                });
            items.Add(menuCpuUsage);

            /*
            // System setting menu item
            MenuItem miServerSetting = new MenuItem();
            miServerSetting.Text = Properties.Resources.ServerSettingText;
            miServerSetting.Click += new EventHandler(
                (o, e) =>
                {
                    // TODO: Implement the Server menu setting

                }
                );
            items.Add(miServerSetting);
           */
 
            // Offline Detection Menu Item
            menuOfflineDetection = new MenuItem("Start Offline Detection");
            menuOfflineDetection.Click += new EventHandler(
                (o, e) =>
                {
                    if (menuOfflineDetection.Text.Equals("Stop Offline Detection") &&
                        _offlineDetectionThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                    {
                        _offlineDetectionThread.Abort();
                        _offlineDetectionThread = null;
                        menuOfflineDetection.Text = "Start Offline Detectiont";

                        _featureProducer.Abort();
                        _featureProducer = null;
                        _flowFeatureProducerConsumer.Stop();
                        _flowFeatureProducerConsumer = null;
                        _parser = null;

                        return;
                    }

                    if (_openOfflineDetectionDlg.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    if (_offlineDetectionThread == null
                        || _offlineDetectionThread.ThreadState == System.Threading.ThreadState.Stopped)
                    {
                        _offlineDetectionThread = new Thread(new ThreadStart(this.OfflineDetectionThread));
                        _offlineDetectionThread.SetApartmentState(ApartmentState.STA);
                        _offlineDetectionThread.Name = "Offline Detection Thread";
                        _offlineDetectionThread.Start();

                        menuOfflineDetection.Text = "Stop Offline Detection";
                    }
                    /*
                    else if (_offlineDetectionThread.ThreadState == System.Threading.ThreadState.Running)
                    {
                        _offlineDetectionThread.Interrupt();

                        menuOfflineDetection.Text = "Start Offline Detectiont";
                    }
                    */
                });
            items.Add(menuOfflineDetection);

            // About dialog menu
            MenuItem menuAbout = new MenuItem("About");
            menuAbout.Click += new EventHandler(
                (o, e) =>
                {
                    MessageBox.Show("Network Flow Detector Server application. Version: "
                        + Properties.Settings.Default.Version,
                        "About",
                        MessageBoxButtons.OK);
                }
                );
            items.Add(menuAbout);

            // Exit menu item
            MenuItem menuExit = new MenuItem("E&xit");
            menuExit.Click += new EventHandler(MenuExit_Click);
            items.Add(menuExit);

            return new ContextMenu(items.ToArray());
        }

        private System.Drawing.Icon GetProcessIcon()
        {
            Process thisProcess = Process.GetCurrentProcess();

            System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon("FlowDetectorServer.exe");

            return ico;
        }
     
        private void OnChangeSignatureFile(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.CheckFileExists = true;

            try
            {
                if (openDlg.ShowDialog() == DialogResult.OK)
                {
                    _malFlowDetector.ReloadSignature(openDlg.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                openDlg.Dispose();
            }
        }

        private void DisplayErrorEvent(object sender, ErrorEventArgs2 e)
        {
            //FIXME: this is a shortcut to get the number of flows examined.
            string msg = AddNumberOfFlows(e.Message);

            _lastErrorMsg = e.Message;

            UpdateNotification(msg);
        }

        private int _totalFlows = 0;
        private string _pattern = @"Detecting on (\d+) flows";
        private string AddNumberOfFlows(string msg)
        {
            try
            {
                Match match = Regex.Match(msg, _pattern);
                if (match.Success)
                {
                    int nbOfFlows = Convert.ToInt32(match.Groups[1].Value);
                    _totalFlows += nbOfFlows;

                    return "Detecting on " + nbOfFlows + " flows";
                }
                else
                    return msg;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
                return default(string);
            }
        }

        private void SaveErrorMsg(string errMsg)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.ErrorLog))
                {
                    sw.WriteLine(errMsg);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                _lastErrorMsg = ex.ToString();
            }
        }

        private void UpdateSystrayIcon(string text)
        {
            if (_notifyIcon != null)
            {
                lock (_notifyIcon)
                {
                    _notifyIcon.Text = text;
                }
            }
        }

        private void UpdateNotification(string text)
        {
            if (_notifyIcon != null && !string.IsNullOrEmpty(text))
            {
                lock (_notifyIcon)
                {
                    _notifyIcon.BalloonTipText = text;
                    _notifyIcon.ShowBalloonTip(1000);
                }
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            _notifyIcon.ShowBalloonTip(5000);
        }

        private void NotifyIcon_MouseHover(object sender, MouseEventArgs e)
        {
            //_notifyIcon.Text = string.Format("{0} malicious flow out of {1} flows\n{2}",
            //    _maliciousFlowsDetected.Count,
            //    _totalFlows,
            //    this._lastMalFlowDetectedTime == DateTime.MinValue
            //    ? "" : "Last malicious flow detected at " + this._lastMalFlowDetectedTime.ToString("HH:mm:ss.f"));

            //if we are processing a file offline print progress.
            if(_parser != null && _parser._totalFrameCount != 0)
                _notifyIcon.Text = string.Format("{0} flows Rx. Proc. frame {1:0,0} from {2:0,0} ({3:0.0}%)", 
                    FlowFeatureDataSvc.TotalFeatuerRecieved,_parser._curFrameNb,_parser._totalFrameCount,((decimal)_parser._curFrameNb/(decimal)_parser._totalFrameCount)*100 );
            else
                _notifyIcon.Text = string.Format("Total {0} flow features received.", FlowFeatureDataSvc.TotalFeatuerRecieved);
        }

        private void NotifyIcon_BallonTipClosed(object sender, EventArgs e)
        {
            _notifyIcon.BalloonTipText = string.Empty;
        }

        private void MenuExit_Click(object sender, EventArgs e)
        {
            SaveFlow(_malFlowFile);

            Application.Exit();
        }

        /// <summary>
        /// Return the CPU usage of this process
        /// </summary>
        /// <returns></returns>
        private double CpuUsage()
        {
            Process currentProcess = Process.GetCurrentProcess();

            double usage = (1.0d * currentProcess.TotalProcessorTime.TotalSeconds)
                / ((DateTime.Now - currentProcess.StartTime).TotalSeconds);

            return usage;
        }

        private void SaveFlow(string filepath)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filepath))
                {
                    foreach (FlowFeature f in _maliciousFlowsDetected)
                    {
                        sw.WriteLine(f.ToString());
                    }

                    sw.WriteLine("Total " + _maliciousFlowsDetected.Count + " malicious flows detected out of " + _totalFlows + " flows.");

                    sw.WriteLine("CPU usage: " + CpuUsage().ToString("p2"));

                    sw.WriteLine("Network Monitor Error Message:" + _malFlowDetector.GetErrorMessage());

                    //sw.WriteLine("Total " +  _processMonitor.ProcessTable.Count + " entries in process table.");

                    //foreach (ProcessInfo pInfo in _processMonitor.ProcessTable.Values)
                    //{
                    //    sw.WriteLine(pInfo);
                    //}

                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                UpdateNotification(ex.ToString());
            }
        }

        private Thread CreateDatabaseThread()
        {
            Thread dbThread = new Thread(new ThreadStart(this.SaveFlowToDatabaseThread));

            dbThread.SetApartmentState(ApartmentState.STA);
            dbThread.IsBackground = true;

            return dbThread;
        }

        private void SaveFlowToDatabaseThread()
        {
            //MySqlDao dao = null;
            try
            {
                //dao = new MySqlDao();
                //Flow.SetLabelTable(dao.GetFlowLabels());

                do
                {
                    Thread.Sleep(5000);

                    List<FlowFeature> features = null;
                    lock (_maliciousFlowsDetected)
                    {
                        features = _maliciousFlowsDetected;
                        _maliciousFlowsDetected = new List<FlowFeature>();
                    }

                    if (features.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("@["+DateTime.Now.ToString() +"]"+" Saving " + features.Count + " Malicious Flows to DB");
                        dao.InsertAlerts(features);
                        features.Clear();
                        features = null;
                    }
                }
                while (true);
            }
            catch (ThreadInterruptedException tie)
            {
                ;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                /*
                if (dao != null)
                {
                    dao.Dispose();
                }
                 */
            }
        }
    }
}
