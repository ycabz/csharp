using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YCabz.Network
{
    public class TcpTextServer : IDisposable
    {
        public event TcpTextMessageEventHandler TextMessageReceived;
        public event TcpTextMessageEventHandler TextMessageSent;

        public bool IsConnected { get => (client != null && client.Connected == true); }

        public Encoding Encoding { get; set; } = Encoding.Unicode;

        public int MaxBufferSize { get; set; } = 1024;


        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);     // true : 초기 쓰레드 통과, false : 초기 쓰레드 블럭
        private readonly ITcpTextServer iTcpServer;
        private Socket listener;
        private Socket client;

        public TcpTextServer()
        {
            iTcpServer = new TcpText();
        }


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

                await Task.Run(() => { autoResetEvent.WaitOne(); });
            }
        }

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
                await iTcpServer.SendText(client, Encoding, message, MaxBufferSize);
                TextMessageSent?.Invoke(message);
            }
        }

        private async void PrepareToReceive()
        {
            while (IsConnected == true)
            {
                var message = await iTcpServer.ReceiveText(client, Encoding, MaxBufferSize);

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
