using PanaTrace;
using PanaTrace.Communication;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Vortex.Connector;
using x_template_xPlc;

namespace x_template_xPlc
{
    public partial class TcoPscPanaTrace : INotifyPropertyChanged
    {

        private string logFileName;

        public string LogFileName
        {
            get { return logFileName; }
            set { logFileName = value; }
        }


        private int connectionTimeout;

        public int ConnectionTimeout
        {
            get { return connectionTimeout; }
            set { connectionTimeout = value; }
        }

        private PscPanaTraceDataPacket request;
        private PscPanaTraceDataPacket response;

        public PscPanaTraceDataPacket Request
        {
            get { return request; }
            set
            {
                request = value;
                OnPropertyChanged(nameof(Request));
            }
        }
        public PscPanaTraceDataPacket Response 
        {
            get { return response; }
            set
            {
                response = value;
                OnPropertyChanged(nameof(Response));
            }
        }


        public Logger Logger { get; private set; }

        public PanaTraceTcpClient Client { get; set; }

        #region Methods
        /// <summary>
        /// Initializes all remote execs calls
        /// > [!IMPORTANT]
        /// > Initialize has to be called only on one instance. If you are running multiple applications, only one should call Initialize method.
        /// </summary>
        public void Initialize(GetCsvDataDelegate getCsvDataDelegate)
        {
            this._config.Read();
            var config = new PlainTcoPscPanaTraceConfig();
          
            _config.FlushOnlineToPlain(config);
            

            Logger = new LoggerConfiguration().WriteTo.RollingFile("Logs\\Station-" + config.Workplace + "-{Date}.txt",
                                                     retainedFileCountLimit: 1,
                                                     fileSizeLimitBytes: null,
                                                     flushToDiskInterval: new TimeSpan(0, 1, 0))
                                                     .MinimumLevel.Information()
                                                     .CreateLogger();

            Logger.Information(@"{Workplace} - Initialized", _config.Workplace);
            
            GetCsvData = getCsvDataDelegate;

            _generateSerialNo.InitializeExclusively(GenerateSerialNo);
            _confirmSerialNo.InitializeExclusively(ConfirmSerialNo);
            _sendReport.InitializeExclusively(SendReport);
            _pcbWedding.InitializeExclusively(PcbWedding);
            _getStatus.InitializeExclusively(GetStatus);

            Request = new PscPanaTraceDataPacket();
            Response = new PscPanaTraceDataPacket();

            connectionTimeout = 10;

        }

        /// <summary>
        /// Initializes all remote execs calls
        /// > [!IMPORTANT]
        /// > Initialize has to be called only on one instance. If you are running multiple applications, only one should call Initialize method.
        /// > <paramref name="connectionTimeout"/> is used to define tcp connection timeout.
        /// </summary>
        /// <param name="connectionTimeout"></param>
        public void Initialize(GetCsvDataDelegate getCsvDataDelegate, int connectionTimeout)
        {
            this._config.Read();
            var config = new PlainTcoPscPanaTraceConfig();

            Logger = new LoggerConfiguration().WriteTo.RollingFile("Logs\\Station-" + _config.Workplace + "-{Date}.txt",
                                                     retainedFileCountLimit: 1,
                                                     fileSizeLimitBytes: null,
                                                     flushToDiskInterval: new TimeSpan(0, 1, 0))
                                                     .MinimumLevel.Information()
                                                     .CreateLogger();

            Logger.Information(@"{Workplace} - Initialized",_config.Workplace);

            GetCsvData = getCsvDataDelegate;

            _generateSerialNo.InitializeExclusively(GenerateSerialNo);
            _confirmSerialNo.InitializeExclusively(ConfirmSerialNo);
            _sendReport.InitializeExclusively(SendReport);
            _pcbWedding.InitializeExclusively(PcbWedding);
            _getStatus.InitializeExclusively(GetStatus);

            Request = new PscPanaTraceDataPacket();
            Response = new PscPanaTraceDataPacket();

            this.connectionTimeout = connectionTimeout;

        }


        public void Connect()
        {
            this._config.Read();
            var config = new PlainTcoPscPanaTraceConfig();

            var address = config.IpAddress;
            var port = config.Port;

            try
            {

                Client = new PanaTraceTcpClient().Connect(address, port);

                Logger.Information(@"{Workplace} - Connected to {address}:{port}", config.Workplace, address, port);

                _errorMessage.Cyclic = "";
                this.Write();

            }
            catch (Exception ex)
            {
                Logger.Error(@"{Workplace} - No connection to {address}:{port}", config.Workplace, address, port);

                _errorMessage.Cyclic = $"ERR - No connection to device {address}:{port}";
                this.Write();

                if (Client != null)
                {
                    Client.Dispose();
                }
                throw;
            }
        }


        public delegate List<CsvData> GetCsvDataDelegate();

        public GetCsvDataDelegate GetCsvData { get; set; }


        /// <summary>
        /// Sends request to server to generate a new serial number. If request was successfull new serial no is located in Response.MasterNo
        /// </summary>
        /// <returns></returns>
        public bool GenerateSerialNo()
        {
            this._control.Read();
            var control = new PlainTcoPscPanaTraceControl();

            Request.Command = eTcoPscPanaTraceCommandType.GenerateSerialNo;
            Request.Data = string.Empty;
            Request.Dps1 = string.Empty;
            Request.Dps2 = string.Empty;
            Request.Dps3 = string.Empty;
            Request.Dps4 = string.Empty;
            Request.Dps5 = control.ReferenceId;


            var retVal = TracabilityCommand();
           
            _status.GeneratedSerialNo.Cyclic = Response.MasterNo;
            this.Write();

            return retVal;
        }

        /// <summary>
        /// Sends request to server to pack serial numbers from DPS1 - DPS5 with main serial number sent in parameter Request.MasterNo.
        /// </summary>
        /// <returns></returns>
        public bool PcbWedding()
        {
            this._control.Read();
            var control = new PlainTcoPscPanaTraceControl();
          

            Request.Command = eTcoPscPanaTraceCommandType.PcbWedding;
            Request.Data = "";
            Request.Dps1 = control.PcbSerials[1];
            Request.Dps2 = control.PcbSerials[2];
            Request.Dps3 = control.PcbSerials[3];
            Request.Dps4 = control.PcbSerials[4];
            Request.Dps5 = control.PcbSerials[5];

            var retVal = TracabilityCommand();

            return retVal;
        }

        
        /// <summary>
        /// Sends data to server with csv data from process
        /// </summary>
        /// <returns></returns>
        public bool SendReport()
        {
            this._control.Read();
            var control = new PlainTcoPscPanaTraceControl();

            Request.Command = eTcoPscPanaTraceCommandType.SendReport;
            var csv = GetCsvData?.Invoke();
            var sb = new StringBuilder();

            foreach (var item in csv)
            {
                sb.AppendLine(item.ToString());
            }

            Request.Data = sb.ToString();
            Request.Dps1 = control.PcbSerials[1];
            Request.Dps2 = control.PcbSerials[2];
            Request.Dps3 = control.PcbSerials[3];
            Request.Dps4 = control.PcbSerials[4];
            Request.Dps5 = control.PcbSerials[5];

            var retVal = TracabilityCommand();

            return retVal;
        }

        /// <summary>
        /// Checks with server wheter Request.MasterNo can be processesed in a station
        /// </summary>
        /// <returns></returns>
        public bool ConfirmSerialNo()
        {
            this._control.Read();
            var control = new PlainTcoPscPanaTraceControl();

            Logger.Information("--------New part--------");


            Request.Command = eTcoPscPanaTraceCommandType.ConfirmSerialNo;
            Request.Data = string.Empty;
            Request.Dps1 = string.Empty;
            Request.Dps2 = string.Empty;
            Request.Dps3 = string.Empty;
            Request.Dps4 = string.Empty;
            Request.Dps5 = control.ReferenceId;


            var retVal = TracabilityCommand();

            return retVal;
        }

        /// <summary>
        /// Requests status of Pana Trace
        /// </summary>
        /// <returns></returns>
        public bool GetStatus()
        {

            Request.Command = eTcoPscPanaTraceCommandType.GetStatus;
            Request.Data = string.Empty;
            Request.Dps1 = string.Empty;
            Request.Dps2 = string.Empty;
            Request.Dps3 = string.Empty;
            Request.Dps4 = string.Empty;
            Request.Dps5 = string.Empty;


            var retVal = TracabilityCommand();

            return retVal;
        }

        private void LogRequest(PscPanaTraceDataPacket request)
        {
            //Logger.Information($"REQ: {request.ToString()}" );

            Logger.Information($"REQ: {request.RequestToReadableHex()}");
            Logger.Information($"Command: {request.ToString()}");
        }




        private void LogResponse(PscPanaTraceDataPacket response, byte[] message)
        {
            Logger.Information($"RES: {response.ToReadableHex(message)}");
            Logger.Information($"Data: {response.ToString()}");
        }




        private bool TracabilityCommand()
        {
            try
            {

                if (Client == null)
                {
                    Connect();
                }
                else if (!Client.isClientConnected())
                {
                    Client.Dispose();
                    Connect();
                }
                this._config.Read();
                var config = new PlainTcoPscPanaTraceConfig();
                this._control.Read();
                var control = new PlainTcoPscPanaTraceControl();

                //this.Read();

                Response.Result = string.Empty;

                Request.Workplace = config.Workplace;

                if (string.IsNullOrEmpty(control.Rfid))
                {
                    Request.Rfid = "0";
                }
                else
                {
                    Request.Rfid = control.Rfid;
                }
                
                Request.MasterNo = control.PartSerialNo;
                Request.OperatorId = control.OperatorId;
                Request.Result = control.Result;

                Request.CalculateChecksum();

                LogRequest(Request);

                var message = Client.WriteAndGetReply(Request.ToByteArray(), new System.TimeSpan(0, 0, connectionTimeout));              

                Response.ParseResponse(message);

                LogResponse(Response, message);
            }
            catch (Exception ex)
            {
                Response.Data = "9 - No response from server";
                Response.Result = "NG";

                Logger.Error($"{ex.Message} ");
                throw;
            }

            _tracabilityResponse.Cyclic = Response.Result;
            _dataFromTracability.Cyclic = Response.Data;
            this.Write();

            return true;
        }
        #endregion

        #region NotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        ~TcoPscPanaTrace()
        {
            if (Client != null)
                Client.Dispose();
        }
    }
}
