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

    public class TcpText : ITcpTextServer, ITcpTextClient
    {
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

        public Socket Listen(int port, string ipString = "", int backLog = 1)
        {
            var ipAddress = IPAddress.Any;
            if (string.IsNullOrEmpty(ipString) == false && IPAddress.TryParse(ipString, out ipAddress) == false)
            {
                throw new InvalidCastException($"Invalid Cast IpString: {ipString}");
            }

            // Socket
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(ipAddress, port));
            socket.Listen(backLog);

            return socket;
        }

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

        public async Task SendText(Socket socket, Encoding encoding, string message, int bufferMaxSize)
        {
            await Task.Run(() =>
            {
                if (socket != null && socket.Connected == true)
                {
                    var buffer = encoding.GetBytes(message);
                    var offset = 0;

                    do
                    {
                        var length = Math.Min(buffer.Length - offset, bufferMaxSize);

                        socket.Send(buffer, offset, length, SocketFlags.None);
                        offset += length;
                    } while (offset < buffer.Length);
                }
            });
        }

        public async Task<string> ReceiveText(Socket socket, Encoding encoding, int bufferMaxSize)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (socket != null && socket.Connected == true)
                    {
                        var buffer = new byte[bufferMaxSize];
                        var count = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                        var message = encoding.GetString(buffer, 0, count);
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
