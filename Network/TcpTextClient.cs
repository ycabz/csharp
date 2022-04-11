using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Network
{
    /// <summary>
    /// TCP/IP 통신 - Client
    /// </summary>
    public class TcpTextClient : IDisposable
    {
        /// <summary>
        /// Message 수신 이벤트
        /// </summary>
        public event TcpTextMessageEventHandler TextMessageReceived;

        /// <summary>
        /// Message 송신 이벤트
        /// </summary>
        public event TcpTextMessageEventHandler TextMessageSent;

        /// <summary>
        /// Server와 연결상태
        /// </summary>
        public bool IsConnected { get => (client != null && client.Connected == true); }

        /// <summary>
        /// 서버 IPAddress
        /// </summary>
        public string ConnectedServerIpString { get; private set; }

        /// <summary>
        /// 서버 포트
        /// </summary>
        public int ConnectedServerPort { get; private set; }

        /// <summary>
        /// Delay If Reconnect
        /// </summary>
        public int ReconnectDelayMilliSecond { get; set; } = 1000;

        /// <summary>
        /// Reconnection Flag
        /// </summary>
        public bool ShoudReconnection { get; set; } = true;

        /// <summary>
        /// Message Max Buffer
        /// </summary>
        public int MaxBufferSize { get; set; } = 1024;

        /// <summary>
        /// Message Encoding
        /// </summary>
        public Encoding Encoding { set => iTcpClinet.Encoding = value; }


        private readonly ITcpTextClient iTcpClinet;
        private Socket client;


        /// <summary>
        /// 생성자
        /// </summary>
        public TcpTextClient()
        {
            iTcpClinet = new TcpText();
        }


        /// <summary>
        /// 서버와 연결
        /// </summary>
        /// <param name="ipString">IPAddress</param>
        /// <param name="port">Port</param>
        public async void Connect(string ipString, int port)
        {
            // 현재 연결된 서버 IP/Port 초기화
            ConnectedServerIpString = null;
            ConnectedServerPort = -1;

            while (IsConnected == false)
            {
                client = await iTcpClinet.Connect(ipString, port);

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

        /// <summary>
        /// 서버와 연결 해제
        /// (ShoudReconnection = true면, 재연결 시도)
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                client.Shutdown(SocketShutdown.Both);
            }

            client?.Dispose();
            client = null;

            if (ShoudReconnection == true   // 재연결이 필요하고
                && string.IsNullOrEmpty(ConnectedServerIpString) == false)  // 이전 연결정보가 있으면
            {
                Connect(ConnectedServerIpString, ConnectedServerPort);
            }
        }

        /// <summary>
        /// 연결 소켓 해제
        /// </summary>
        public void Dispose()
        {
            client?.Dispose();
        }

        public async void SendText(string message)
        {
            if (IsConnected == true)
            {
                await iTcpClinet.SendText(client, message, MaxBufferSize);
                TextMessageSent?.Invoke(message);
            }
        }

        private async void PrepareToReceive()
        {
            while (IsConnected == true)
            {
                var message = await iTcpClinet.ReceiveText(client, MaxBufferSize);

                // Disconnected
                if (string.IsNullOrEmpty(message) == true)
                {
                    Disconnect();
                    return;
                }

                TextMessageReceived?.Invoke(message);
            }
        }
    }
}
