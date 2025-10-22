using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using x_template_xPlc;

namespace x_template_xPlc
{
    public class PscPanaTraceDataPacket : INotifyPropertyChanged
    {
        public const byte Separator = 0x7C;

        public PscPanaTraceDataPacket()
        {
            //Data = new List<CsvData>();
        }

        public PscPanaTraceDataPacket(eTcoPscPanaTraceCommandType command, string workplace, string rfid)
        {
            Command = command;
            Workplace = workplace;
            Rfid = rfid;
            //Data = new List<CsvData>();
        }

        public PscPanaTraceDataPacket(eTcoPscPanaTraceCommandType command, string workplace, string rfid, string masterNo)
        {
            Command = command;
            Workplace = workplace;
            Rfid = rfid;
            MasterNo = masterNo;
            //Data = new List<CsvData>();
        }

        #region Properties
        private string length;

        public string Length
        {
            get { return length; }
            set
            {
                length = value;
                OnPropertyChanged(nameof(Length));
            }
        }

        private eTcoPscPanaTraceCommandType command;

        public eTcoPscPanaTraceCommandType Command 
        {
            get { return command; }
            set
            {
                command = value;
                OnPropertyChanged(nameof(Command));
            }
        }

        private string workplace = "";

        public string Workplace
        {
            get { return workplace; }
            set
            {
                workplace = value;
                OnPropertyChanged(nameof(Workplace));
            }
        }

        private string rfid = "";

        public string Rfid
        {
            get { return rfid; }
            set
            {
                rfid = value;
                OnPropertyChanged(nameof(Rfid));
            }
        }


        private string masterNo = "";

        public string MasterNo
        {
            get { return masterNo; }
            set
            {
                masterNo = value;
                OnPropertyChanged(nameof(MasterNo));
            }
        }

        private string dps1 = "";

        public string Dps1
        {
            get { return dps1; }
            set
            {
                dps1 = value;
                OnPropertyChanged(nameof(Dps1));
            }
        }

        private string dps2 = "";

        public string Dps2
        {
            get { return dps2; }
            set
            {
                dps2 = value;
                OnPropertyChanged(nameof(Dps2));
            }
        }

        private string dps3 = "";

        public string Dps3
        {
            get { return dps3; }
            set
            {
                dps3 = value;
                OnPropertyChanged(nameof(Dps3));
            }
        }

        private string dps4 = "";

        public string Dps4
        {
            get { return dps4; }
            set
            {
                dps4 = value;
                OnPropertyChanged(nameof(Dps4));
            }
        }

        private string dps5 = "";

        public string Dps5
        {
            get { return dps5; }
            set
            {
                dps5 = value;
                OnPropertyChanged(nameof(Dps5));
            }
        }

        private string operatorId = "";

        public string OperatorId
        {
            get { return operatorId; }
            set
            {
                operatorId = value;
                OnPropertyChanged(nameof(OperatorId));
            }
        }


        private string data = "";

        public string Data
        {
            get { return data; }
            set
            {
                data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

        private string result = "";

        public string Result
        {
            get { return result; }
            set
            {
                result = value;
                OnPropertyChanged(nameof(Result));
            }
        }   

        private byte checksum;

        /// <summary>
        /// Calculated as XOR of fields from Length (include) to Checksum (exclude)
        /// </summary>
        public byte Checksum
        {
            get { return checksum; }
            set 
            { 
                checksum = value;
                OnPropertyChanged(nameof(Checksum));
            }
        }

        #endregion

        #region Public methods

        public void CalculateChecksum()
        {
            Checksum = 0x00;

            //Checksum = Checksum ^ Length ^ (int)Command ^ (int)Workplace ^ Id;
            var trimmedData = ToStringForChecksumCalculation();

            foreach (var item in trimmedData)
            {
                Checksum = (byte)((byte)Checksum ^ (byte)item);
            }

            return;
        }

        /// <summary>
        /// Parse data from <paramref name="data"/> to separate fields of PscTracabilityDataPacket object.
        /// </summary>
        /// <param name="data"></param>
        public void ParseResponse(byte[] data)
        {
            try
            {
                if (data != null)
                {
                    var recvString = Encoding.Default.GetString(data);

                    if (recvString[0] != 0x02)
                    {
                        throw new FormatException($"Received data doesn't start with 0x02! Received data: {recvString}");
                    }

                    // last 2 characters are allways checksum and 0x03, and checksum can have a byte value as separator
                    var splitData = recvString.Substring(0, recvString.Length - 3).Split((char)Separator);

                    Length = splitData[0].Trim((char)0x02);

                    int tempCommand;
                    int.TryParse(splitData[1], out tempCommand);
                    Command = (eTcoPscPanaTraceCommandType)tempCommand;
                    Workplace = splitData[2];
                    Rfid = splitData[3];
                    MasterNo = splitData[4];
                    Dps1 = splitData[5];
                    Dps2 = splitData[6];
                    Dps3 = splitData[7];
                    Dps4 = splitData[8];
                    Dps5 = splitData[9];
                    OperatorId = splitData[10];
                    Data = splitData[11];
                    Result = splitData[12];
                    Checksum = (byte)recvString[recvString.Length-2];
                }
                else
                {
                    
                    Data = "9 - No response from server";
                    Result = "NG";
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public string ToReadableHex(byte[] data)
        {
            return BitConverter.ToString(data);
        }

        public string ResponseToReadableHex()
        {
            return Data;// ToReadableHex(Data.ToArray());
        }

        public override string ToString()
        {
            return Encoding.Default.GetString(ToByteArray());
        }


        public string RequestToReadableHex()
        {
            return ToReadableHex(ToByteArray());
        }

        public byte[] ToByteArray()
        {
            List<byte> retVal = new List<byte>();

            CalculateLength();

            retVal.Add(0x02);

            foreach (var character in Length)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            retVal.AddRange(ToListFromCommandToResult());

            retVal.Add((byte)Checksum);
            retVal.Add(0x03);

            return retVal.ToArray<byte>();
        }

        /// <summary>
        /// Generates new List<byte> from Command to Result.
        /// </summary>
        /// <returns></returns>
        private List<byte> ToListFromCommandToResult()
        {
            List<byte> retVal = new List<byte>();
            retVal.Add((byte)(Command + 48));
            retVal.Add(Separator);

            foreach (var character in Workplace)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            foreach (var character in Rfid)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            foreach (var character in MasterNo)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            foreach (var character in Dps1)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            foreach (var character in Dps2)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            foreach (var character in Dps3)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            foreach (var character in Dps4)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            foreach (var character in Dps5)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            foreach (var character in OperatorId)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            ParseCsvData(retVal);

            retVal.Add(Separator);

            foreach (var character in Result)
            {
                retVal.Add((byte)character);
            }
            retVal.Add(Separator);

            return retVal;
        }

        private void ParseCsvData(List<byte> retVal)
        {
            foreach (var item in Data)
            {
                retVal.Add((byte)item);
            }
        }

        private string ToStringForChecksumCalculation()
        {
            var retVal = new List<byte>();

            CalculateLength();

            foreach (var character in Length)
            {
                retVal.Add((byte)character);
            }

            retVal.Add(Separator);

            retVal.AddRange(ToListFromCommandToResult());

            return Encoding.Default.GetString(retVal.ToArray());
        }

        private void CalculateLength()
        {
            var retVal = new List<byte>();

            retVal.AddRange(ToListFromCommandToResult());
            retVal.Add(0);

            Length = retVal.Count.ToString();
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
    }
}
