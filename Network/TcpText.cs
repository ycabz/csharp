using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Network
{
    public delegate void TcpTextMessageEventHandler(string message);

    /// <summary>
    /// TCP/IP Text Server/Client
    /// </summary>
    public class TcpText : ITcpTextServer, ITcpTextClient
    {
        /// <summary>
        /// Message Encoding
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.Unicode;

        /// <summary>
        /// Connect To Server
        /// </summary>
        /// <param name="ipString">Server IPAddress</param>
        /// <param name="port">Server Port</param>
        /// <returns></returns>
        public async Task<Socket> Connect(string ipString, int port)
        {
            return await Task.Run(() =>
             {
                 if (IPAddress.TryParse(ipString, out var ipAddress) == false)
                 {
                     throw new InvalidCastException($"Invalid Cast IpString: {ipString}");
                 }

                 var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                 try
                 {
                     socket.Connect(ipAddress, port);
                     return socket;
                 }
                 catch (SocketException)
                 {
                     return null;
                 }
                 catch (Exception ex)
                 {
                     throw ex;
                 }
             });
        }

        /// <summary>
        /// Server Listen
        /// </summary>
        /// <param name="port">Server Port</param>
        /// <param name="ipString">Server IPAddress (if NullOrEmpty then IPAddress.Any)</param>
        /// <returns>Listener</returns>
        public Socket Listen(int port, string ipString = "")
        {
            var ipAddress = IPAddress.Any;
            if (string.IsNullOrEmpty(ipString) == false && IPAddress.TryParse(ipString, out ipAddress) == false)
            {
                throw new InvalidCastException($"Invalid Cast IpString: {ipString}");
            }

            // Socket
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(ipAddress, port));
            socket.Listen(1);

            return socket;
        }

        /// <summary>
        /// Server Accept Client
        /// </summary>
        /// <param name="listener">Listener</param>
        /// <returns>Client</returns>
        public async Task<Socket> Accept(Socket listener)
        {
            return await Task.Run(() =>
            {
                if (listener == null)
                {
                    throw new NullReferenceException("Listener is Null");
                }

                return listener.Accept();
            });
        }

        /// <summary>
        /// Send Message
        /// </summary>
        /// <param name="socket">Server/Client</param>
        /// <param name="message">message</param>
        /// <param name="bufferSize">buffer Size</param>
        /// <returns></returns>
        public async Task SendText(Socket socket, string message, int bufferSize)
        {
            await Task.Run(() =>
            {
                if (socket != null && socket.Connected == true)
                {
                    var buffer = Encoding.GetBytes(message);
                    var offset = 0;

                    do
                    {
                        var length = Math.Min(buffer.Length - offset, bufferSize);

                        socket.Send(buffer, offset, length, SocketFlags.None);
                        offset += length;
                    } while (offset < buffer.Length);
                }
            });
        }

        /// <summary>
        /// Receive Message
        /// </summary>
        /// <param name="socket">Server/Client</param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public async Task<string> ReceiveText(Socket socket, int bufferSize)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (socket != null && socket.Connected == true)
                    {
                        var buffer = new byte[bufferSize];
                        var count = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                        var message = Encoding.GetString(buffer, 0, count);
                        return message;
                    }
                }
                catch (SocketException)
                {
                    // Nothing
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return null;
            });
        }
    }
}
