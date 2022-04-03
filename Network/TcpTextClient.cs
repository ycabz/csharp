using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Network
{
    public class TcpTextClient : IDisposable
    {
        public event TcpTextMessageEventHandler TextMessageReceived;
        public event TcpTextMessageEventHandler TextMessageSent;

        public bool IsConnected { get => (connectedSocket != null && connectedSocket.Connected == true); }

        public string ConnectedServerIpString { get; private set; }

        public int ConnectedServerPort { get; private set; }

        public int ReconnectDelayMilliSecond { get; set; } = 1000;

        public bool ShoudReconnection { get; set; } = true;

        public Encoding Encoding { get; set; } = Encoding.Unicode;

        public int MaxBufferSize { get; set; } = 1024;


        private readonly ITcpTextClient iTcpClinet;
        private Socket connectedSocket;


        public TcpTextClient()
        {
            iTcpClinet = new TcpText();
        }


        public async void Connect(string ipString, int port)
        {
            // 현재 연결된 서버 IP/Port 초기화
            ConnectedServerIpString = null;
            ConnectedServerPort = -1;

            while (IsConnected == false)
            {
                connectedSocket = await iTcpClinet.Connect(ipString, port);

                if (IsConnected == true)
                {
                    ConnectedServerIpString = ipString;
                    ConnectedServerPort = port;

                    PrepareToReceive();
                    break;
                }

                if (ShoudReconnection == false)
                {
                    break;
                }

                Task.Delay(ReconnectDelayMilliSecond).Wait();
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                connectedSocket.Shutdown(SocketShutdown.Both);
            }

            connectedSocket?.Dispose();
            connectedSocket = null;

            if (ShoudReconnection == true   // 재연결이 필요하고
                && string.IsNullOrEmpty(ConnectedServerIpString) == false)  // 이전 연결정보가 있으면
            {
                Connect(ConnectedServerIpString, ConnectedServerPort);
            }
        }

        public void Dispose()
        {
            connectedSocket.Dispose();
        }

        public async void SendText(string message)
        {
            if (IsConnected == true)
            {
                await iTcpClinet.SendText(connectedSocket, Encoding, message, MaxBufferSize);
                TextMessageSent?.Invoke(message);
            }
        }

        private async void PrepareToReceive()
        {
            while (IsConnected == true)
            {
                var message = await iTcpClinet.ReceiveText(connectedSocket, Encoding, MaxBufferSize);

                if (string.IsNullOrEmpty(message) == true)  // Disconnected
                {
                    Disconnect();
                    return;
                }

                TextMessageReceived?.Invoke(message);
            }
        }
    }
}
