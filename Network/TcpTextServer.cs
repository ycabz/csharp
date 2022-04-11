using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YCabz.Network
{
    /// <summary>
    /// TCP/IP 통신 - Server
    /// </summary>
    public class TcpTextServer : IDisposable
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
        /// Client와 연결상태
        /// </summary>
        public bool IsConnected { get => (client != null && client.Connected == true); }

        /// <summary>
        /// Message Max Buffer
        /// </summary>
        public int MaxBufferSize { get; set; } = 1024;

        /// <summary>
        /// Message Encoding
        /// </summary>
        public Encoding Encoding { set => iTcpServer.Encoding = value; }

        // true : 초기 쓰레드 통과, false : 초기 쓰레드 블럭
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private readonly ITcpTextServer iTcpServer;
        private Socket listener;
        private Socket client;


        /// <summary>
        /// 생성자
        /// </summary>
        public TcpTextServer()
        {
            iTcpServer = new TcpText();
        }


        /// <summary>
        /// Server Listen And Accept Client
        /// </summary>
        /// <param name="port">Server Port</param>
        /// <param name="ipString">Server IPAddress (if NullOrEmpty then IPAddress.Any)</param>
        public async void ListenAndAccept(int port, string ipString = "")
        {
            listener = iTcpServer.Listen(port, ipString);

            while (listener != null)
            {
                client = await iTcpServer.Accept(listener);
                if (IsConnected == true)
                {
                    PrepareToReceive();
                }

                try
                {
                    await Task.Run(() => { autoResetEvent.WaitOne(); });
                }
                catch (ObjectDisposedException)
                {
                    // When ResetEventDisposed
                    break;
                }
            }
        }

        /// <summary>
        /// 서버와 연결 해제
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                client.Shutdown(SocketShutdown.Both);
            }

            client?.Dispose();
            client = null;

            autoResetEvent.Set();
        }

        /// <summary>
        /// 연결 소켓 해제
        /// </summary>
        public void Dispose()
        {
            client?.Dispose();
            listener?.Dispose();
            autoResetEvent.Dispose();
        }

        public async void SendText(string message)
        {
            if (IsConnected == true)
            {
                await iTcpServer.SendText(client, message, MaxBufferSize);
                TextMessageSent?.Invoke(message);
            }
        }

        private async void PrepareToReceive()
        {
            while (IsConnected == true)
            {
                var message = await iTcpServer.ReceiveText(client, MaxBufferSize);

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
