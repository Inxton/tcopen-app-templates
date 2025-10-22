using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace PanaTrace.Communication
{
    public class PanaTraceTcpClient
        : IDisposable
    {
        private Thread _rxThread = null;
        private List<byte> _queuedMsg = new List<byte>();
        private TcpClient clientSocket = null;
        public TcpClient ClientSocket { get { return clientSocket; } }

        internal bool QueueStop { get; set; }

        public event EventHandler<byte[]> DataReceived;

        public PanaTraceTcpClient Connect(string serverIp, int serverPort)
        {
            if (string.IsNullOrEmpty(serverIp))
            {
                throw new ArgumentNullException("serverIp");
            }

            try
            {
                clientSocket = new TcpClient();
                clientSocket.Connect(serverIp, serverPort);
            }
            catch (Exception)
            {

                throw;
            }

            StartRxThread();

            return this;
        }

        public bool isClientConnected()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation connection in tcpConnections)
            {
                TcpState stateOfConnection = connection.State;

                if (clientSocket != null)
                {
                    if (connection.LocalEndPoint.Equals(clientSocket.Client.LocalEndPoint) && connection.RemoteEndPoint.Equals(clientSocket.Client.RemoteEndPoint))
                    {
                        if (stateOfConnection == TcpState.Established)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                }
                

            }

            return false;


        }

        private void StartRxThread()
        {
            if (_rxThread != null) { return; }

            _rxThread = new Thread(ListenerLoop);
            _rxThread.IsBackground = true;
            _rxThread.Start();
        }

        public PanaTraceTcpClient Disconnect()
        {
            if (clientSocket == null) { return this; }
            clientSocket.Close();
            clientSocket = null;
            return this;
        }

        private void ListenerLoop(object state)
        {
            while (!QueueStop)
            {
                try
                {
                    RunLoopStep();
                }
                catch
                {

                }

                System.Threading.Thread.Sleep(10);
            }

            _rxThread = null;
        }

        private void RunLoopStep()
        {
            if (clientSocket == null) { return; }
            if (clientSocket.Connected == false) { return; }

            var c = clientSocket;

            int bytesAvailable = c.Available;
            if (bytesAvailable == 0)
            {
                System.Threading.Thread.Sleep(10);
                return;
            }

            List<byte> bytesReceived = new List<byte>();

            while (c.Available > 0 && c.Connected)
            {
                byte[] nextByte = new byte[1];
                c.Client.Receive(nextByte, 0, 1, SocketFlags.None);
                bytesReceived.AddRange(nextByte);
                //if (nextByte[0] == 0x13)
                //{
                //    byte[] msg = _queuedMsg.ToArray();
                //    _queuedMsg.Clear();
                //    NotifyEndTransmissionRx(msg);
                //}
                //else
                //{
                _queuedMsg.AddRange(nextByte);
                //}
            }

            if (bytesReceived.Count > 0)
            {
                byte[] msg = _queuedMsg.ToArray();
                _queuedMsg.Clear();
                NotifyEndTransmissionRx(msg);

                //NotifyEndTransmissionRx(bytesReceived.ToArray());
            }
        }

        private void NotifyEndTransmissionRx(byte[] msg)
        {
            if (DataReceived != null)
            {
                DataReceived(this, msg);
            }
        }

        public void Write(byte[] data)
        {
            if (clientSocket == null) { throw new Exception("Cannot send data to a null TcpClient (check to see if Connect was called)"); }
            clientSocket.GetStream().Write(data, 0, data.Length);
        }

        public byte[] WriteAndGetReply(byte[] data, TimeSpan timeout)
        {
            byte[] mReply = null;
            this.DataReceived += (s, e) => { mReply = e; };
            Write(data);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (mReply == null && sw.Elapsed < timeout)
            {
                System.Threading.Thread.Sleep(10);
            }

            return mReply;
        }

        public void Dispose()
        {
            QueueStop = true;

            if (clientSocket != null)
            {
                try
                {
                    clientSocket.Close();
                }
                catch { }
                clientSocket = null;
            }
        }
    }
}
