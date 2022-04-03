using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Network
{
    public interface ITcpTextClient
    {
        Task<Socket> Connect(string ipString, int port);
        Task<string> ReceiveText(Socket socket, Encoding encoding, int bufferMaxSize);
        Task SendText(Socket socket, Encoding encoding, string message, int bufferMaxSize);
    }
}