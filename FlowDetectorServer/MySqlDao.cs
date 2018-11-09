using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace Biotracker.Client.ProcessMonitor
{
    public class MySqlDao : IDisposable
    {
        /// <summary>
        /// Default database connection string
        /// </summary>
        private string _connectionString = "server=localhost;user=biotracker;database=botnetFlow.db;password=biotracker;";

        private static readonly string INSERT_SQL = "INSERT INTO `alertsbuffer`(`FLD_LOGGERIDENTITY`, `FLD_TIMESTAMP`, `FLD_APL`, `FLD_PV` , `FLD_PX`, `FLD_PPS`, `FLD_FPS`, `FLD_DPL`, `FLD_FLOWTYPE`, `FLD_SOURCEIP`, `FLD_SOURCEPORT`, `FLD_SOURCEMAC`, `FLD_DESTINATIONIP`, `FLD_DESTINATIONPORT`, `FLD_DESTINATIONMAC`, `FLD_TRASPORTTYPE`, `FLD_AB`, `FLD_TBT`, `FLD_BS`, `FLD_PS`, `FLD_NNP`,`FLD_NSP`,`FLD_PSP`,`FLD_Duration`,`FLD_AIT`,`FLD_IOPR`, FLD_RECONNECT) VALUES";

        private static readonly string QUERY_FLOW_LABEL = "SELECT `name`, `label` FROM `FlowTypeTable`";

        private MySqlConnection _dbConnection = null;

        #region Constructors

        public MySqlDao()
        {
            if(_dbConnection != null)
                _dbConnection.Close();

            _connectionString = string.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                Properties.Settings.Default.DBSvrAddr,
                Properties.Settings.Default.DBUser,
                Properties.Settings.Default.DBPwd,
                Properties.Settings.Default.DBSchema
                );

            try
            {
                _dbConnection = new MySqlConnection(_connectionString);

                _dbConnection.Open();

            }
            catch (MySqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //we have to close the db connection every time we open it
                //so that we can use it later somewhere else.
                if (_dbConnection != null)
                    _dbConnection.Close();
            }
        }


        #endregion

        #region Public Methods
        /// <summary>
        /// Insert/Update alert records
        /// </summary>
        /// <param name="features"></param>
        /// <returns></returns>
        public void InsertAlerts(IEnumerable<FlowFeature> features)
        {
            StringBuilder sbSql = null;

            try
            {
                string flowType, srcMAC, dstMAC, protocol,timestamp;
               //// long srcIP, dstIP;
                string srcIP, dstIP;
                foreach (FlowFeature f in features)
                {
                    sbSql = new StringBuilder();
                    flowType = Flow2.GetFlowTypeName(f.Type);
                   // srcIP = ConvertIpToLong(f.SrcIP);
                    srcIP = ConvertIpToString(f.SrcIP);
                    srcMAC = ConvertMACAddressToString(f.SrcMAC);
                   // dstIP = ConvertIpToLong(f.DestIP);
                    dstIP = ConvertIpToString(f.DestIP);
                    dstMAC = ConvertMACAddressToString(f.DestMAC);
                    protocol = GetProtocolName(f.Protocol);
                  //  timestamp = DateTime.UtcNow.ToString("o");//o means: 2008-06-15T21:15:07.0000000
                    //Just getting the milliseconds percision
                    timestamp = f.DetectionTimeStamp.ToString("o");
                    timestamp = timestamp.Substring(0,timestamp.Length - 5);


                    if (Double.IsInfinity(f.PPS))
                        f.PPS = 0.0d;



                    sbSql.Append(INSERT_SQL);
                    sbSql.AppendFormat(" (\"{0}\", \"{1}\", {2}, {3}, {4}, {5}, {6}, {7}, \"{8}\", \"{9}\", {10}, \"{11}\", \"{12}\", {13}, \"{14}\", \"{15}\", {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26});",
                        f.LoggerIp,
                        //f.DetectionTimeStamp.ToString("o"), 
                        timestamp, 
                        f.APL,
                        f.PV,
                        f.PX,
                        f.PPS,
                        f.FPS,
                        f.DPL,
                        flowType,
                        srcIP,
                        f.SrcPort,
                        srcMAC,
                        dstIP,
                        f.DestPort,
                        dstMAC,
                        protocol,
                        f.AB,
                        f.TBT,
                        f.BS,
                        f.PS, 
                        f.NNP, 
                        f.NSP,
                        f.PSP,
                        f.Duration,
                        f.AIT,
                        f.IOPR,
                        f.Reconnect
                        );

                    sbSql.AppendLine();
                    
                    //System.Diagnostics.Debug.WriteLine(sbSql.ToString());

                    //using the connection inside the loop is intentional to introduce a bit
                    //of delay so that we don't have overlaping timestamps for the alerts
                    using (MySqlConnection _dbConnection = new MySqlConnection(_connectionString))
                    {
                        _dbConnection.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sbSql.ToString(), _dbConnection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        _dbConnection.Close();
                    }

                }
            }//end for
            catch (MySqlException sqlEx)
            {
                //throw sqlEx;
                System.Diagnostics.Debug.WriteLine(sqlEx.ToString());
            }
            catch (Exception ex)
            {
                //throw ex;
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally 
            {
                ;
            }

            sbSql.Clear();
            sbSql = null;
        }

        /// <summary>
        /// Retrieve the flow labels from the database table "FlowTypeTable"
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> GetFlowLabels()
        {
            try
            {
                List<KeyValuePair<string, string>> labelDictionary
                    = new List<KeyValuePair<string, string>>();

                using (MySqlConnection _dbConnection = new MySqlConnection(_connectionString))
                {
                    _dbConnection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(QUERY_FLOW_LABEL, _dbConnection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                labelDictionary.Add(
                                    new KeyValuePair<string, string>(reader.GetString(0), reader.GetString(1)));

                            }
                        }
                    }
                    _dbConnection.Close();
                }

                return labelDictionary;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
            try
            {
                _dbConnection.Close();
                _dbConnection.Dispose();
            }
            catch (Exception)
            {
                ;
            }
        }
        #endregion

        #region Helper functions

        private string ConvertMACAddressToString(byte[] mac)
        {
            if(mac.Length != 6)
                return default(string);
            
            StringBuilder sb = new StringBuilder();
            int i = 0;
            do
            {
                sb.Append(mac[i].ToString("x2"));
                i++;
                if (i >= mac.Length)
                    break;
                else
                {
                    sb.Append(":");
                }
            }
            while (true);

            return sb.ToString();
        }



        private string ConvertIpToString(byte[] ip)
        {
            if (ip.Length != 4)
                return default(string);

            StringBuilder sb = new StringBuilder();
            int i = 0;
            do
            {
                sb.Append(ip[i].ToString());
                i++;
                if (i >= ip.Length)
                    break;
                else
                {
                    sb.Append(".");
                }
            }
            while (true);

            return sb.ToString();
        }




        private Int64 ConvertIpToLong(byte[] ip)
        {
            return (Int64)BitConverter.ToInt32(ip, 0);
        }

        private string GetProtocolName(int protocol)
        {
            switch (protocol)
            { 
                case 0:
                    return "TCP";
                case 1:
                    return "UDP";
                default:
                    return "mixed";
            }
        }

        #endregion
    }
}
